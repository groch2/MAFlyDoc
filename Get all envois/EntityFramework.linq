<Query Kind="Statements">
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

const string environment = "local";
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
		//.Where(envoi => envoi.LastEtatEnvoiHistoryEntry.DateTime > DateTimeOffset.Parse("01/07/2024"))
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
	.Select(
		envoi => new {
			envoi.EnvoiId,
			Etat = envoi.LastEtatEnvoiHistoryEntry.Etat,
			Date = DateOnly.FromDateTime(envoi.LastEtatEnvoiHistoryEntry.DateTime.Date),
			History =
				envoi
					.EtatsEnvoiHistoryEntriesList
					.Select(
						entry =>
							new {
								Date = DateOnly.FromDateTime(entry.DateTime.Date),
								entry.Etat
							}),
			envoi.MailPostage,
			envoi.TransportId,
			envoi.DocumentsArTelechargesGedId,
		})
	.OrderByDescending(envoi => envoi.Date)
	.Select(
		(envoi, index) => new {
			index = index + 1,
			envoi.EnvoiId,
			envoi.Etat,
			envoi.Date,
			envoi.History,
			envoi.MailPostage,
			envoi.TransportId,
			envoi.DocumentsArTelechargesGedId,
		})
	.Dump();

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
