<Query Kind="Program">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string ENVIRONMENT_CODE = "int";
const string sqlServer = $"bdd-MAFlyDoc.{ENVIRONMENT_CODE}.maf.local";
const string webApiAddress = $"https://api-maflydoc-intra.{ENVIRONMENT_CODE}.maf.local";
async Task Main() {
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
			//.Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
			//.OrderBy(envoi => envoi.LastEtatEnvoiHistoryEntry.DateTime)
			//.Take(3)
			//.Where(envoi => envoi.LastEtatEnvoiHistoryEntry.DateTime > DateTimeOffset.Parse("01/07/2024"))
			.Select(envoi => envoi.EnvoiId);
	//envoisIdsList = new int[] { 1, 2, 3 }.AsQueryable();
	const bool withEtatEnvoiHistory = true;
	(await Task.WhenAll(
		GroupIntegersByMaxNbDigitsInGroups(items: envoisIdsList, maxGroupSize: 1400, groupsSeparatorLength: 1)
			.Select(envoisIdList => string.Join(',', envoisIdList))
			.Select(async formattedEnvoisIdsList => {
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
		.OrderByDescending(envoi => envoi.LastEtatEnvoiHistoryEntry.DateTime)
		.Select(
			(envoi, index) => new {
				index = index + 1,
				envoi.EnvoiId,
				Etat = envoi.LastEtatEnvoiHistoryEntry.Etat,
				Etat_date =
					DateOnly.FromDateTime(envoi.LastEtatEnvoiHistoryEntry.DateTime.Date),
				Creation_date =
					DateOnly.FromDateTime(envoi.EtatsEnvoiHistoryEntriesList.Last().DateTime.Date),
				envoi.MailPostage,
				envoi.TransportId,
				envoi.DocumentsArTelechargesGedId,
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
		var groupSize = 0;
		foreach (var item in items) {
			state = getNextState(state, item);
			if (!statePredicate(state)) {
				yield return group.ToArray();
				group = new List<T>{ item };
				groupSize = 1;
				state = resetState();
				state = getNextState(state, item);
			} else {
				group.Add(item);
				groupSize++;
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
