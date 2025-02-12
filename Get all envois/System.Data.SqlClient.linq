<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string ENVIRONNEMENT_MAF = "int";
var settings = JsonDocument.Parse(File.ReadAllText(@$"C:\Users\deschaseauxr\Documents\MAFlyDoc\Get all envois\settings_{ENVIRONNEMENT_MAF}.json")).RootElement;
var sqlServer = settings.GetProperty("sqlServer").GetString();
var webApiAddress = settings.GetProperty("webApiAddress").GetString();
var httpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };
var jsonSerializerOptions = 
	new JsonSerializerOptions {
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};

var envoisId = GetAllEnvoisIdFromDatabase(sqlServer);
new { nbEnvoiTotal = envoisId.Count() }.Dump();

var envois =
	await SelectManyAsync(
		GetItemsByPages(envoisId, pageSize: 400),
		GetEnvoisByEnvoisIdList)
			.Select(
				(envoi, index) => new {
					index = index + 1,
					envoi.EnvoiId,
					envoi.LastEtatEnvoiHistoryEntry,
					envoi.TransportId,
					envoi.EtatFinalErrorMessage,
				})		
			.ToArrayAsync();
envois.Dump();

async IAsyncEnumerable<U> SelectManyAsync<T, U>(
	IEnumerable<IEnumerable<T>> sourceItemsPages,
	Func<IEnumerable<T>, Task<IEnumerable<U>>> func) {
	foreach (var page in sourceItemsPages) {
		var resultItems = await func(page);
		foreach (var resultItem in resultItems) {
			yield return resultItem;
		}
	}
}

async Task<IEnumerable<EnvoiQueryResult>> GetEnvoisByEnvoisIdList(IEnumerable<int> envoisIdList) {
	var commaSeparatedEnvoisIdList = string.Join(',', envoisIdList);
	var getAllEnvoisResponse =
		await httpClient.GetAsync(
			$"/v1/Envois/Envois-from-envois-id-list?comma-separated-envois-id-list={commaSeparatedEnvoisIdList}");
	getAllEnvoisResponse.EnsureSuccessStatusCode();
	var content = await getAllEnvoisResponse.Content.ReadAsStringAsync();
	return JsonSerializer.Deserialize<EnvoiQueryResult[]>(content, jsonSerializerOptions);
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

static IEnumerable<int> GetAllEnvoisIdFromDatabase(string sqlServer) {
	using var connection =
		new SqlConnection($"Server={sqlServer};Database=MAFlyDoc;Integrated Security=True");
	connection.Open();
	using var command = connection.CreateCommand();
	command.CommandText = "SELECT [EnvoiId] FROM [dbo].[Envoi]";
	var dataTable = new DataTable();
	var dataAdapter = new SqlDataAdapter(command);
	dataAdapter.Fill(dataTable);
	return dataTable.AsEnumerable().Select(r => (int)r[0]);
}