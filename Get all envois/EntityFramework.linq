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
const string sqlServer = $"bdd-MAFlyDoc.{ENVIRONMENT_CODE}.maf.local";
const string webApiAddress = $"https://api-maflydoc-intra.{ENVIRONMENT_CODE}.maf.local";
static readonly DateTimeFormatInfo dateTimeFormat = CultureInfo.GetCultureInfo("FR-fr").DateTimeFormat;
async Task Main() {
	var dbContextOptions =
		new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
			.EnableSensitiveDataLogging()
			.UseSqlServer(
				$"Server={sqlServer};Database=MAFlyDoc;Trusted_Connection=True;",
				providerOptions => providerOptions.CommandTimeout(1))
		    .Options;
	using var context = new EnvoiCourrierDbContext(dbContextOptions);
	var minDateCreation =
		DateTimeOffset.ParseExact(
			input: "25-03-2025 17:25 +01",
			format: "dd-MM-yyyy HH:mm zz",
			dateTimeFormat);
	var envoisList =
		context
			.Set<MAFlyDoc.WebApi.Database.Model.Envoi>()
			.Select(envoi => new { envoi, creationDate = envoi.EtatsEnvoiHistory.Select(etat => etat.DateTime).Min() })
			.Where(envoiItem => envoiItem.creationDate > minDateCreation)
			.OrderBy(envoiItem => envoiItem.creationDate);
	const bool withEtatEnvoiHistory = true;
	(await Task.WhenAll(
		GroupIntegersByMaxNbDigitsInGroups(
			items: envoisList.Select(envoiItem => envoiItem.envoi.EnvoiId),
			maxGroupSize: 1400,
			groupsSeparatorLength: 1)
				.Select(async envoisIdList => {
					var formattedEnvoisIdsList = string.Join(',', envoisIdList);
					var allEnvois =
						await httpClient.GetStringAsync($"/v1/Envois/Envois-from-envois-id-list?comma-separated-envois-id-list={formattedEnvoisIdsList}&with-etat-envoi-history={withEtatEnvoiHistory}");
					return JsonDocument
						.Parse(allEnvois)
						.RootElement
						.EnumerateArray()
						.Select(item =>
							JsonSerializer.Deserialize<EnvoiQueryResult>(item, jsonSerializerOptions));
				})))
		.SelectMany(envois => envois)
		.Select(
			(envoi, index) => new {
				index = index + 1,
				envoi.EnvoiId,
				date_creation =
					FormatDateTimeOffset(envoi.EtatsEnvoiHistoryEntriesList.Last().DateTime),
				etat_actuel = envoi.LastEtatEnvoiHistoryEntry.Etat,
				date_etat_actuel =
					FormatDateTimeOffset(envoi.LastEtatEnvoiHistoryEntry.DateTime),
				affranchissement = envoi.MailPostage,
				sujet = envoi.Subject,
				raison_si_echec = envoi.EtatFinalErrorMessage
			})
		.Dump();
}

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

static HttpClient httpClient =
	new HttpClient { BaseAddress = new Uri(webApiAddress) };
static JsonSerializerOptions jsonSerializerOptions =
	new JsonSerializerOptions {
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};

static string FormatDateTimeOffset(DateTimeOffset dateTime) =>
	dateTime.ToString("ddd dd/MM/yyyy HH:mm:ss zz\\h");

DateOnly ToDateOnly(DateTimeOffset dateTime) =>
	DateOnly.FromDateTime(dateTime.Date);