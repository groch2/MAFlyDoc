<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string ENVIRONNEMENT_MAF = "int";
var settings = JsonDocument.Parse(File.ReadAllText(@$"C:\Users\deschaseauxr\Documents\MAFlyDoc\Get all envois\settings_{ENVIRONNEMENT_MAF}.json")).RootElement;
var webApiAddress = settings.GetProperty("webApiAddress").GetString();
var httpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };
var jsonSerializerOptions = 
	new JsonSerializerOptions {
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};

var envois_id_list = File.ReadAllLines(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\envoi_id_list.txt").Select(int.Parse).ToArray();
$"nombre d'envois: {envois_id_list.Length}".Dump();

var envois =
	await SelectManyAsync(
		GroupIntegersByMaxNbDigitsInGroups(items: envois_id_list, maxNbDigitsInGroups: 1500),
		GetEnvoisByEnvoisIdList)
			.Select(
				(envoi, index) => new {
					index = index + 1,
					envoi.EnvoiId,
					envoi.EtatsEnvoiHistoryEntriesList,
					//envoi.LastEtatEnvoiHistoryEntry,
					envoi.TransportId,
					envoi.MailPostage
				})		
			.ToArrayAsync();
envois.Dump();

async Task<IEnumerable<EnvoiQueryResult>> GetEnvoisByEnvoisIdList(IEnumerable<int> envoisIdList) {
	var commaSeparatedEnvoisIdList = string.Join(',', envoisIdList);
	var getAllEnvoisResponse =
		await httpClient.GetAsync(
			$"/v1/Envois/Envois-from-envois-id-list?comma-separated-envois-id-list={commaSeparatedEnvoisIdList}&with-etat-envoi-history=true");
	getAllEnvoisResponse.EnsureSuccessStatusCode();
	var content = await getAllEnvoisResponse.Content.ReadAsStringAsync();
	return JsonSerializer.Deserialize<EnvoiQueryResult[]>(content, jsonSerializerOptions);
}

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
