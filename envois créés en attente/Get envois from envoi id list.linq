<Query Kind="Program">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string ENVIRONMENT_CODE = "j1d";
async Task Main() {
	var envois_id_list =
		JsonDocument.Parse(
			File.ReadAllText(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\envois créés en attente\résultat_nouveaux_envois.json"))
				.RootElement.EnumerateArray()
					.Select(resultat_envoi =>
						new {
							original_envoi_id = resultat_envoi.GetProperty("original_envoi_id").GetInt32(),
							new_envoi_id = resultat_envoi.GetProperty("result").GetInt32()
						})
					.ToArray();
	//$"nombre d'envois: {envois_id_list.Length}".Dump();
	var envoisOriginauxByEnvoiId =
		await SelectManyAsync(
			GetItemsByPages(envois_id_list, pageSize: 400)
				.Select(chunk => chunk.Select(envoi => envoi.original_envoi_id)),
			GetEnvoisByEnvoisIdList)
				.Select(
					(envoi, index) => new {
						//index = index + 1,
						envoi.EnvoiId,					
						date_creation = envoi.EtatsEnvoiHistoryEntriesList.Last().DateTime,
						//envoi.LastEtatEnvoiHistoryEntry,
						//envoi.TransportId,
						//envoi.MailPostage,
						//envoi.Subject,
						envoi.TransportId,
					})		
				.ToDictionaryAsync(envoi => envoi.EnvoiId);
	
	var new_envois_list =
		await SelectManyAsync(
			GetItemsByPages(envois_id_list, pageSize: 400)
				.Select(chunk => chunk.Select(envoi => envoi.new_envoi_id)),
			GetEnvoisByEnvoisIdList)
				.Select(
					envoi => new {
						envoi.EnvoiId,					
						envoi.EtatsEnvoiHistoryEntriesList,
						envoi.LastEtatEnvoiHistoryEntry,
						//envoi.TransportId,
						envoi.MailPostage,
						envoi.Subject,
						envoi.MainDocumentGedId,
						envoi.TransportId,
					})
				.ToArrayAsync();
	var envois_liés_list =
		new_envois_list
			//.Where(envoi => envoi.EtatsEnvoiHistoryEntriesList.First().Etat == EtatEnvoiEnum.ENVOYE)
			.Select(envoi => {
				var original_envoi_id = envois_id_list.First(_envoi => _envoi.new_envoi_id == envoi.EnvoiId).original_envoi_id;
				return new {
					original_envoi_id,
					envoi_Original_TransportId = envoisOriginauxByEnvoiId[original_envoi_id].TransportId,
					new_envoi_id = envoi.EnvoiId,
					dernier_état = new { envoi.LastEtatEnvoiHistoryEntry.Etat, Date = FormatDateTime(envoi.LastEtatEnvoiHistoryEntry.DateTime) },
					//historique = envoi.EtatsEnvoiHistoryEntriesList,
					date_envoi = FormatDateTime(envoi.EtatsEnvoiHistoryEntriesList.Last().DateTime),			
					affranchissement = envoi.MailPostage,
					sujet = envoi.Subject,
					mainDocumentGedId = envoi.MainDocumentGedId,
					transportId = envoi.TransportId,
				};
			})
			.OrderBy(envoi => envoisOriginauxByEnvoiId[envoi.original_envoi_id].date_creation)
			.Select((envoi, index) =>
				new {
					index = index + 1,
					envoi.original_envoi_id,
					envoi.envoi_Original_TransportId,
					envoi.new_envoi_id,
					envoi.dernier_état,
					origine_date_envoi = FormatDateTime(envoisOriginauxByEnvoiId[envoi.original_envoi_id].date_creation),
					new_date_envoi = envoi.date_envoi,
					envoi.sujet,
					envoi.affranchissement,
					envoi.mainDocumentGedId,
					new_envoi_transport_id = envoi.transportId,
				})
		.GroupBy(envoi => envoi.original_envoi_id)
		.SelectMany( group =>
			group.Select( item =>
				new {
					item.index,
					item.original_envoi_id,
					item.envoi_Original_TransportId,
					item.new_envoi_id,
					item.dernier_état,
					item.origine_date_envoi,
					item.new_date_envoi,
					item.sujet,
					item.affranchissement,
					doublon = group.Count() > 1 ? "OUI" : "NON",
					item.mainDocumentGedId,
					new_envoi_transport_id = item.new_envoi_transport_id,
				}));
	var documents_GED_id_list = envois_liés_list.Select(envoi => envoi.mainDocumentGedId);
	//new { documents_GED_id_list }.Dump();
	var numeroSinistreByDocumentId =
		(await GetDocumentsByDocumentsIdList(documents_GED_id_list))
			.ToDictionary(
				document => document[DocProperty.DocumentId],
				document => new { NumeroSinistre = document[DocProperty.NumeroSinistre], Libelle = document[DocProperty.Libelle] });
	var envois_list_3 =
		envois_liés_list.Select(envoi => new {
			envoi.index,
			envoi.original_envoi_id,
			envoi.envoi_Original_TransportId,
			envoi.new_envoi_id,
			//envoi.dernier_état,
			//envoi.origine_date_envoi,
			//envoi.new_date_envoi,
			//envoi.sujet,
			//envoi.affranchissement,
			//envoi.doublon,
			//numeroSinistre = numeroSinistreByDocumentId[envoi.mainDocumentGedId].NumeroSinistre,
			//libelle = numeroSinistreByDocumentId[envoi.mainDocumentGedId].Libelle,
			new_envoi_transport_id = envoi.new_envoi_transport_id,
		});	
	envois_list_3.Dump();
	
	string FormatDateTime(DateTimeOffset dateTime) => dateTime.ToString(format: "ddd dd/MM/yyyy HH:mm:ss zzzz", dateFormatter);
}

static readonly JsonElement settings = JsonDocument.Parse(File.ReadAllText(@$"C:\Users\deschaseauxr\Documents\MAFlyDoc\Get all envois\settings_{ENVIRONMENT_CODE}.json")).RootElement;
static readonly string webApiAddress = settings.GetProperty("webApiAddress").GetString();
static readonly HttpClient maflydocHttpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };
static readonly JsonSerializerOptions jsonSerializerOptions = 
	new JsonSerializerOptions {
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};
static readonly DateTimeFormatInfo dateFormatter = CultureInfo.GetCultureInfo("FR-fr").DateTimeFormat;

static async Task<IEnumerable<EnvoiQueryResult>> GetEnvoisByEnvoisIdList(IEnumerable<int> envoisIdList) {
	var commaSeparatedEnvoisIdList = string.Join(',', envoisIdList);
	var getAllEnvoisResponse =
		await maflydocHttpClient.GetAsync(
			$"/v1/Envois/Envois-from-envois-id-list?comma-separated-envois-id-list={commaSeparatedEnvoisIdList}&with-etat-envoi-history=true");
	getAllEnvoisResponse.EnsureSuccessStatusCode();
	var content = await getAllEnvoisResponse.Content.ReadAsStringAsync();
	return JsonSerializer.Deserialize<EnvoiQueryResult[]>(content, jsonSerializerOptions);
}

static readonly HttpClient mafGedHttpClient = new HttpClient { BaseAddress = new Uri($"https://api-ged-intra.{ENVIRONMENT_CODE}.maf.local/v2/Documents/") };
static async Task<IEnumerable<Dictionary<DocProperty, object>>> GetDocumentsByDocumentsIdList(IEnumerable<string> documentsIdList) {
	var nbDocumentsIdInEachGroup = GetNbDocumentsIdInEachGroup();
	const char separator = ',';
	var documents =
		(await Task.WhenAll(
	        documentsIdList
	        .Chunk(nbDocumentsIdInEachGroup)
	        .Select(async documentsIdList => {
	            var commaSeparatedDocumentsIdList = string.Join(separator, documentsIdList.Select(documentId => $"'{documentId}'"));
				//var queryString = $"?$filter=documentId in ({commaSeparatedDocumentsIdList})&$select=assigneRedacteur,assureurId,canalPrincipal,categoriesCote,categoriesFamille,categoriesTypeDocument,chantierId,codeOrigine,commentaire,compteId,dateDocument,dateNumerisation,deposeLe,deposePar,docn,documentId,extension,fichierNom,fichierNombrePages,fichierTaille,heureNumerisation,horodatage,important,libelle,modifieLe,modifiePar,numeroGc,numeroSinistre,periodeValiditeDebut,periodeValiditeFin,personneId,presenceAr,previewLink,qualiteValideeLe,qualiteValideePar,qualiteValideeValide,regroupementId,sens,sousDossierSinistre,statut,traiteLe,traitePar,typeGarantie,vuLe,vuPar";
				var queryString = $"?$filter=documentId in ({commaSeparatedDocumentsIdList})&$select=documentId,numeroSinistre,libelle";
				//Environment.Exit(0);
				//documentsIdList.Dump();
	            var responseContent = await mafGedHttpClient.GetStringAsync(queryString);
				return JsonDocument
					.Parse(responseContent)
					.RootElement
					.GetProperty("value")
					.EnumerateArray()
					.Select(jsonElement => JsonNode.Parse(jsonElement.ToString()))
					.Select(jsonNode =>
						new Dictionary<DocProperty, object> {
							//{ DocProperty.AssigneRedacteur, jsonNode["assigneRedacteur"]?.ToString() },
							{ DocProperty.DocumentId, jsonNode["documentId"]?.ToString() },
							{ DocProperty.Libelle, jsonNode["libelle"]?.ToString() },
							//{ DocProperty.Commentaire, jsonNode["commentaire"]?.ToString() },
							//{ DocProperty.TypeGarantie, jsonNode["typeGarantie"]?.ToString() },
							//{ DocProperty.FichierNom, jsonNode["fichierNom"]?.ToString() },
							//{ DocProperty.Famille, jsonNode["categoriesFamille"]?.ToString() },
							//{ DocProperty.Côte, jsonNode["categoriesCote"]?.ToString() },
							//{ DocProperty.TypeDocument, jsonNode["categoriesTypeDocument"]?.ToString() },
							//{ DocProperty.DeposeLe, GetDateOnly(jsonNode["deposeLe"]?.GetValue<DateTime>()) },
							//{ DocProperty.DeposePar, jsonNode["deposePar"]?.ToString() },
							//{ DocProperty.VuLe, GetDateOnly(jsonNode["vuLe"]?.GetValue<DateTime>()) },
							//{ DocProperty.VuPar, jsonNode["vuPar"]?.ToString() },
							//{ DocProperty.QualiteValideeLe, GetDateOnly(jsonNode["qualiteValideeLe"]?.GetValue<DateTime>()) },
							//{ DocProperty.QualiteValideePar, jsonNode["qualiteValideePar"]?.ToString() },
							//{ DocProperty.QualiteValideeValide, jsonNode["qualiteValideeValide"]?.ToString() },
							//{ DocProperty.TraiteLe, GetDateOnly(jsonNode["traiteLe"]?.GetValue<DateTime>()) },
							//{ DocProperty.TraitePar, jsonNode["traitePar"]?.ToString() },
							//{ DocProperty.ModifieLe, GetDateOnly(jsonNode["modifieLe"]?.GetValue<DateTime>()) },
							//{ DocProperty.ModifiePar, jsonNode["modifiePar"]?.ToString() },
							//{ DocProperty.NumeroContrat, jsonNode["numeroContrat"]?.ToString() },
							{ DocProperty.NumeroSinistre, jsonNode["numeroSinistre"]?.ToString() },
							//{ DocProperty.ChantierId, jsonNode["chantierId"]?.ToString() },
							//{ DocProperty.AssureurId, jsonNode["assureurId"]?.ToString() },
							//{ DocProperty.CompteId, jsonNode["compteId"]?.ToString() },
							//{ DocProperty.PersonneId, jsonNode["personneId"]?.ToString() },
							//{ DocProperty.Sens, jsonNode["sens"]?.ToString() },
							//{ DocProperty.SousDossierSinistre, jsonNode["sousDossierSinistre"]?.ToString() },
							//{ DocProperty.Important, jsonNode["important"]?.ToString() },
							//{ DocProperty.PeriodeValiditeDebut, GetDateOnly(jsonNode["periodeValiditeDebut"]?.GetValue<DateTime>()) },
							//{ DocProperty.PeriodeValiditeFin, GetDateOnly(jsonNode["periodeValiditeFin"]?.GetValue<DateTime>()) },
							//{ DocProperty.Statut, jsonNode["statut"]?.ToString() }
						});
			        })))
				.SelectMany(documents => documents);
	return documents;
	
	int GetNbDocumentsIdInEachGroup() {
		const int requestUrlAndQueryLengthLimit = 2048;
		var baseUrlLength = mafGedHttpClient.BaseAddress.AbsoluteUri.Length;
		const byte separatorLength = 1; // ","
		const int documentIdLength = 26; // "20241027021216767823360255".Length
		var nbDocumentsIdInEachGroup =
		    (requestUrlAndQueryLengthLimit - baseUrlLength) / (documentIdLength + separatorLength + 2);
		return nbDocumentsIdInEachGroup;
	}
	
	DateOnly? GetDateOnly(DateTime? dateTime) =>
		dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
}

enum DocProperty { AssigneRedacteur, AssureurId, ChantierId, Commentaire, CompteId, Côte, DeposeLe, DeposePar, DocumentId, Famille, FichierNom, Important, Libelle, ModifieLe, ModifiePar, NumeroContrat, NumeroSinistre, PeriodeValiditeDebut, PeriodeValiditeFin, PersonneId, QualiteValideeLe, QualiteValideePar, QualiteValideeValide, Sens, SousDossierSinistre, Statut, TraiteLe, TraitePar, TypeDocument, TypeGarantie, VuLe, VuPar }

static async IAsyncEnumerable<U> SelectManyAsync<T, U>(
	IEnumerable<IEnumerable<T>> sourceItemsPages,
	Func<IEnumerable<T>, Task<IEnumerable<U>>> func) {
	foreach (var page in sourceItemsPages) {
		var resultItems = await func(page);
		foreach (var resultItem in resultItems) {
			yield return resultItem;
		}
	}
}

static IEnumerable<IEnumerable<T>> GetItemsByPages<T>(IEnumerable<T> itemsSource, int pageSize) {
	var nbItemsTotal = itemsSource.Count();
	var skip = 0;
	while (skip + pageSize <= nbItemsTotal) {
		yield return itemsSource.Skip(skip).Take(pageSize);
		skip += pageSize;
	}
	if (skip < nbItemsTotal) {
		yield return itemsSource.Skip(skip).Take(pageSize);
	}
}
