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

const string ENVIRONMENT_CODE = "int";
const string maflyDocSqlServer = $"bdd-MAFlyDoc.{ENVIRONMENT_CODE}.maf.local";
const string maflyDocWebApiAddress = $"https://api-maflydoc-intra.{ENVIRONMENT_CODE}.maf.local";
static readonly DateTimeFormatInfo dateTimeFormat = CultureInfo.GetCultureInfo("FR-fr").DateTimeFormat;
async Task Main() {
	var dbContextOptions =
		new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
			.EnableSensitiveDataLogging()
			.UseSqlServer(
				$"Server={maflyDocSqlServer};Database=MAFlyDoc;Trusted_Connection=True;",
				providerOptions => providerOptions.CommandTimeout(1))
		    .Options;
	using var context = new EnvoiCourrierDbContext(dbContextOptions);
	var minDateCreation =
		DateTimeOffset.ParseExact(
			input: "25-03-2025 17:25 +01",
			format: "dd-MM-yyyy HH:mm zz",
			dateTimeFormat);
	var envoisByEnvoiId =
		context
			.Set<MAFlyDoc.WebApi.Database.Model.Envoi>()
			.Select(envoi =>
				new {
					envoi,
					creationDate = envoi.EtatsEnvoiHistory.Select(etat => etat.DateTime).Min(),
					application = envoi.Application,
				})
			.Where(envoiItem => envoiItem.creationDate > minDateCreation)
			.ToDictionary(envoiItem => envoiItem.envoi.EnvoiId);
	const bool withEtatEnvoiHistory = true;
	var envois =
		(await Task.WhenAll(
			GroupIntegersByMaxNbDigitsInGroups(items: envoisByEnvoiId.Keys, maxGroupSize: 1400, groupsSeparatorLength: 1)
				.Select(envoisIdList => string.Join(',', envoisIdList))
				.Select(async formattedEnvoisIdsList => {
					var allEnvois =
						await maflyDocHttpClient.GetStringAsync($"/v1/Envois/Envois-from-envois-id-list?comma-separated-envois-id-list={formattedEnvoisIdsList}&with-etat-envoi-history={withEtatEnvoiHistory}");
					return JsonDocument
						.Parse(allEnvois)
						.RootElement
						.EnumerateArray()
						.Select(item =>
							JsonSerializer.Deserialize<EnvoiQueryResult>(item, jsonSerializerOptions));
				})))
			.SelectMany(envois => envois)
			.OrderByDescending(envoi => envoi.LastEtatEnvoiHistoryEntry.DateTime)
			.ToArray();
	var documents_GED_id_list = envois.Select(envoi => envoi.MainDocumentGedId).Distinct();
	var documentDataByDocumentId =
		(await GetDocumentsByDocumentsIdList(documents_GED_id_list))
			.ToDictionary(
				document => document[DocProperty.DocumentId],
				document =>
					new {
						NumeroSinistre = document[DocProperty.NumeroSinistre],
						Libelle = document[DocProperty.Libelle],
						CodeRedacteur = document[DocProperty.AssigneRedacteur]
					});
	var codesUtilisateurList =
		documentDataByDocumentId
			.Values
			.Select(document => $"{document.CodeRedacteur}")
			.Distinct()
			.ToArray();
	var utilisateursByCodeUtilisateur =
		(await(GetUtilisateursByCodesUtilisateurList(codesUtilisateurList)))
			.DistinctBy(utilisateur => utilisateur[UtilisateurProperty.CodeUtilisateur])
			.ToDictionary(utilisateur => utilisateur[UtilisateurProperty.CodeUtilisateur]);
	envois
		.Select(
			(envoi, index) => {
				var envoi_document = documentDataByDocumentId[envoi.MainDocumentGedId];
				var main_document_redacteur = utilisateursByCodeUtilisateur.GetValueOrDefault(envoi_document.CodeRedacteur);
				var envoiFromDatabase = envoisByEnvoiId[envoi.EnvoiId];
				return new {
					index = index + 1,
					envoi.EnvoiId,
					date_creation =
						FormatDateTimeOffset(envoiFromDatabase.creationDate.DateTime),
					etat_actuel = envoi.LastEtatEnvoiHistoryEntry.Etat,
					date_etat_actuel =
						FormatDateTimeOffset(envoi.LastEtatEnvoiHistoryEntry.DateTime),
					affranchissement = envoi.MailPostage,
					sujet = envoi.Subject,
					raison_si_echec = envoi.EtatFinalErrorMessage,
					envoi_document.NumeroSinistre,
					redacteur_code = envoi_document.CodeRedacteur,
					redacteur_prenom = main_document_redacteur?[UtilisateurProperty.Prenom],
					redacteur_nom = main_document_redacteur?[UtilisateurProperty.Nom],
					redacteur_login = main_document_redacteur?[UtilisateurProperty.Login],
					redacteur_email = main_document_redacteur?[UtilisateurProperty.Email],
					main_doc_libelle = envoi_document.Libelle,
					application = envoiFromDatabase.application,
				};
			})
		.Dump();
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
					var queryString = $"?$filter=documentId in ({commaSeparatedDocumentsIdList})&$select=documentId,libelle,assigneRedacteur,numeroSinistre";
		            var responseMessage = await mafGedHttpClient.GetAsync(queryString);				
					var responseContent = await responseMessage.Content.ReadAsStringAsync();
					if (!responseMessage.IsSuccessStatusCode) {
						new { code = responseMessage.StatusCode, phrase = responseMessage.ReasonPhrase, error = responseContent }.Dump();
						throw new ApplicationException("erreur à la récupération des documents");
					}
					return JsonDocument
						.Parse(responseContent)
						.RootElement
						.GetProperty("value")
						.EnumerateArray()
						.Select(jsonElement => JsonNode.Parse(jsonElement.ToString()))
						.Select(jsonNode =>
							new Dictionary<DocProperty, object> {
								{ DocProperty.AssigneRedacteur, jsonNode["assigneRedacteur"]?.ToString() },
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
		const int requestUrlAndQueryLengthLimit = 1950;
		var baseUrlLength = mafGedHttpClient.BaseAddress.AbsoluteUri.Length;
		const byte separatorLength = 1; // ","
		const int documentIdLength = 26; // "20241027021216767823360255".Length
		var nbDocumentsIdInEachGroup =
		    (requestUrlAndQueryLengthLimit - baseUrlLength) / (documentIdLength + separatorLength + 2);
		return nbDocumentsIdInEachGroup;
	}
}

enum DocProperty { AssigneRedacteur, AssureurId, ChantierId, Commentaire, CompteId, Côte, DeposeLe, DeposePar, DocumentId, Famille, FichierNom, Important, Libelle, ModifieLe, ModifiePar, NumeroContrat, NumeroSinistre, PeriodeValiditeDebut, PeriodeValiditeFin, PersonneId, QualiteValideeLe, QualiteValideePar, QualiteValideeValide, Sens, SousDossierSinistre, Statut, TraiteLe, TraitePar, TypeDocument, TypeGarantie, VuLe, VuPar }

static readonly HttpClient butHttpClient = new HttpClient { BaseAddress = new Uri($"https://api-but-intra.{ENVIRONMENT_CODE}.maf.local/api/v2/Utilisateurs/") };
static async Task<IEnumerable<Dictionary<UtilisateurProperty, object>>> GetUtilisateursByCodesUtilisateurList(IEnumerable<string> codesUtilisateurList) {
	var nbCodeUtilisateurInEachGroup = 100;
	const char separator = ',';
	var utilisateurs =
		(await Task.WhenAll(
	        codesUtilisateurList
		        .Chunk(nbCodeUtilisateurInEachGroup)
		        .Select(async documentsIdList => {
		            var formattedCodesUtilisateur = string.Join(separator, codesUtilisateurList);
					var queryString = $"codes/{formattedCodesUtilisateur}";
		            var responseMessage = await butHttpClient.GetAsync(queryString);				
					var responseContent = await responseMessage.Content.ReadAsStringAsync();
					if (!responseMessage.IsSuccessStatusCode) {
						new { code = responseMessage.StatusCode, phrase = responseMessage.ReasonPhrase, error = responseContent }.Dump();
						throw new ApplicationException("erreur à la récupération des utilisateurs");
					}
					return JsonDocument
						.Parse(responseContent)
						.RootElement
						.EnumerateArray()
						.Select(jsonElement => JsonNode.Parse(jsonElement.ToString()))
						.Select(jsonNode =>
							new Dictionary<UtilisateurProperty, object> {
								{ UtilisateurProperty.CodeUtilisateur, jsonNode["codeUtilisateur"]?.ToString() },
								{ UtilisateurProperty.Prenom, jsonNode["prenom"]?.ToString() },
								{ UtilisateurProperty.Nom, jsonNode["nom"]?.ToString() },
								{ UtilisateurProperty.Login, jsonNode["login"]?.ToString() },
								{ UtilisateurProperty.Email, jsonNode["email"]?.ToString() },
							});
				        })))
					.SelectMany(utilisateurs => utilisateurs);
	return utilisateurs;
}

enum UtilisateurProperty { Login, CodeUtilisateur, Nom, Prenom, Email }

static IEnumerable<int[]> GroupIntegersByMaxNbDigitsInGroups(
	IEnumerable<int> items,
	int maxGroupSize,
	int groupsSeparatorLength) {
	return GroupItemsByPredicateOnState(
		items: items,
		statePredicate: state => state - groupsSeparatorLength <= maxGroupSize,
		getNextState:
			(state, item) =>
				state + groupsSeparatorLength + GetNumberLength(item),
		resetState: () => 0);

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

	static int GetNumberLength(int number) =>
		number == 0 ? 1 : (int)Math.Floor(Math.Log10(number) + 1);
}

static HttpClient maflyDocHttpClient =
	new HttpClient { BaseAddress = new Uri(maflyDocWebApiAddress) };
static JsonSerializerOptions jsonSerializerOptions =
	new JsonSerializerOptions {
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};

static string FormatDateTimeOffset(DateTimeOffset dateTime) =>
	dateTime.ToString("ddd dd/MM/yyyy HH:mm:ss zz\\h");

DateOnly GetDateOnly(DateTime dateTime) => DateOnly.FromDateTime(dateTime);
