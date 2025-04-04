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

const string environment = "j1d";
async Task Main() {
	var settings = JsonDocument.Parse(File.ReadAllText(@$"C:\Users\deschaseauxr\Documents\MAFlyDoc\Get all envois\settings_{environment}.json")).RootElement;
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
					//envoi.TransportId,
					//envoi.DocumentsArTelechargesGedId,
				};
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
