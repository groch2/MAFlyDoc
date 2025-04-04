<Query Kind="Program">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Data.SqlClient</Namespace>
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
	var settings = JsonDocument.Parse(File.ReadAllText(@$"C:\Users\deschaseauxr\Documents\MAFlyDoc\Get all envois\settings_{ENVIRONMENT_CODE}.json")).RootElement;
	var sqlServer = settings.GetProperty("sqlServer").GetString();
	var webApiAddress = settings.GetProperty("webApiAddress").GetString();
	var dbContextOptions =
		new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
			.EnableSensitiveDataLogging()
			.UseSqlServer(
				$"Server={sqlServer};Database=MAFlyDoc;Trusted_Connection=True;",
				providerOptions => providerOptions.CommandTimeout(1))
		    .Options;
	using var context = new EnvoiCourrierDbContext(dbContextOptions);
	var envoisIdsList =
		context
			.Set<MAFlyDoc.WebApi.Database.Model.Envoi>()
			.Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
			.Include(envoi => envoi.EtatsEnvoiHistory)
			.Where(envoi => envoi.EtatsEnvoiHistory.First(etatEnvoi => etatEnvoi.EtatEnvoi == EtatEnvoiEnum.EN_COURS_D_ENVOI).DateTime > DateTimeOffset.Parse("25/03/2025", dateFormater))
			.Select(envoi => envoi.EnvoiId);
	var httpClient =
		new HttpClient { BaseAddress = new Uri(webApiAddress) };
	var jsonSerializerOptions = new JsonSerializerOptions {
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};
	var envois =
		(await Task.WhenAll(
			GroupIntegersByMaxNbDigitsInGroups(items: envoisIdsList, maxNbDigitsInGroups: 1500)
				.Select(envoisIdList => string.Join(',', envoisIdList))
				.Select(async formattedEnvoisIdsList => {
					var allEnvois =
						await httpClient.GetStringAsync($"/v1/Envois/Envois-from-envois-id-list?comma-separated-envois-id-list={formattedEnvoisIdsList}&with-etat-envoi-history=true");
					return JsonDocument
						.Parse(allEnvois)
						.RootElement
						.EnumerateArray()
						.Select(item =>
							JsonSerializer.Deserialize<EnvoiQueryResult>(item, jsonSerializerOptions));
				}))).SelectMany(envois => envois);
	var envois_with_main_document_ged_id =
		envois.Select(envoi => new { envoi.EnvoiId, envoi.MainDocumentGedId })
			.DistinctBy(envoi_document => envoi_document.MainDocumentGedId)
			.ToArray();
	var documentsByDocumentId =
		(await GetDocumentsByDocumentsIdList(envois_with_main_document_ged_id.Select(envoi => envoi.MainDocumentGedId)))
			.ToDictionary(
				doc => doc[DocProperty.DocumentId],
				doc => doc[DocProperty.NumeroSinistre]);
	//envois_with_main_document_ged_id.ToDictionary(envoi => envoi.EnvoiId,
	//	envoi => documentsByDocumentId[envoi.MainDocumentGedId]);
	envois
		//.Select(
		//	envoi => new {
		//		envoi.EnvoiId,
		//		Etat = envoi.LastEtatEnvoiHistoryEntry.Etat,
		//		Date = DateOnly.FromDateTime(envoi.LastEtatEnvoiHistoryEntry.DateTime.Date),
		//		History =
		//			envoi
		//				.EtatsEnvoiHistoryEntriesList
		//				.Select(
		//					entry =>
		//						new {
		//							Date = DateOnly.FromDateTime(entry.DateTime.Date),
		//							entry.Etat
		//						}),
		//		envoi.MailPostage,
		//		envoi.TransportId,
		//		envoi.DocumentsArTelechargesGedId,
		//	})
		//.OrderByDescending(envoi => envoi.Date)
		.Select(envoi => new { Envoi = envoi, DateCreation = envoi.EtatsEnvoiHistoryEntriesList.Last().DateTime })
		.OrderByDescending(envoiItem => envoiItem.DateCreation)
		.Select(
			(envoiItem, index) => {
				var envoi = envoiItem.Envoi;
				return new {
					index = index + 1,
					envoi_id = envoi.EnvoiId,
					sujet = envoi.Subject,
					affranchissment = FormatMailPostage(envoi.MailPostage),
					date_création = FormatDate(envoiItem.DateCreation),
					état_actuel = envoi.LastEtatEnvoiHistoryEntry.Etat,
					état_actuel_date = FormatDate(envoi.LastEtatEnvoiHistoryEntry.DateTime),
					message_si_envoi_echoué = envoi.EtatFinalErrorMessage,
					numéro_sinistre = documentsByDocumentId[envoi.MainDocumentGedId]
					//envoi.TransportId,
					//envoi.DocumentsArTelechargesGedId,
				};
			})
		.Where(envoiItem =>
			envoiItem.état_actuel switch {
				//EtatEnvoiEnum.EN_COURS_D_ENVOI or
				//EtatEnvoiEnum.EN_COURS_DE_TRAITEMENT or
				//EtatEnvoiEnum.ENVOYE or
				//EtatEnvoiEnum.NON_DISTRIBUE or
				//EtatEnvoiEnum.REMIS_AU_DESTINATAIRE or
				EtatEnvoiEnum.TRAITEMENT_ECHOUE or
				EtatEnvoiEnum.TRAITEMENT_ANNULE or
				EtatEnvoiEnum.TRAITEMENT_REJETE or
				EtatEnvoiEnum.ENVOI_ABANDONNE or
				EtatEnvoiEnum.ABANDONNE_PAR_LA_MAF => true,
				//EtatEnvoiEnum.AR_RECU_PAR_LA_MAF or
				//EtatEnvoiEnum.PND_RECU_PAR_LA_MAF => true,
				_ => false
			})
		.Dump();
}

static DateTimeFormatInfo dateFormater = CultureInfo.GetCultureInfo("FR-fr").DateTimeFormat;
static string FormatDate(DateTimeOffset dateTime) =>
	dateTime.ToString(@"ddd dd/MM/yyyy HH:mm zz\h", dateFormater);

static string FormatMailPostage(MailPostage mailPostage) =>
	mailPostage switch {
		MailPostage.ENVOI_AR => "recommandé",
		MailPostage.ENVOI_PRIORITAIRE => "prioritaire",
		MailPostage.ENVOI_SIMPLE => "économique",
		var other => $"{other}"
	};

static IEnumerable<int[]> GroupIntegersByMaxNbDigitsInGroups(
	IEnumerable<int> items,
	int maxNbDigitsInGroups) {
	return GroupItemsByPredicateOnState(
		items: items,
		statePredicate: state => state <= maxNbDigitsInGroups,
		getNextState: (state, item) => state + GetNumberLength(item),
		resetState: () => 0);

	static int GetNumberLength(int number) =>
		number == 0 ? 1 : (int)Math.Floor(Math.Log10(number) + 1);

	static IEnumerable<T[]> GroupItemsByPredicateOnState<T, U>(
		IEnumerable<T> items,
		Func<U, bool> statePredicate,
		Func<U, T, U> getNextState,
		Func<U> resetState) {
		var group = new List<T>();
		var state = resetState();
		foreach (var item in items) {
			state = getNextState(state, item);
			if (!statePredicate(state)) {
				yield return group.ToArray();
				group = new List<T>{ item };
				state = resetState();
				state = getNextState(state, item);
			} else {
				group.Add(item);
			}
		}
		if (group.Any()) {
			yield return group.ToArray();
		}
	}
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