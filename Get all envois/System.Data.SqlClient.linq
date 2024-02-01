<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi.IntegrationTest\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

var settings = JsonDocument.Parse(File.ReadAllText(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\Get all envois\settings.json")).RootElement;
var sqlServer = settings.GetProperty("sqlServer").GetString();
var webApiAddress = settings.GetProperty("webApiAddress").GetString();

using var connection = new SqlConnection($"Server={sqlServer};Database=MAFlyDoc;Integrated Security=True");
connection.Open();
using var command = connection.CreateCommand();
command.CommandText = "SELECT [EnvoiId] FROM [dbo].[Envoi]";
var dataTable = new DataTable();
var dataAdapter = new SqlDataAdapter(command);
dataAdapter.Fill(dataTable);
var envoisId = dataTable.AsEnumerable().Select(r => (int)r[0]);

var httpClient =
	new HttpClient {
		BaseAddress = new Uri(webApiAddress)
	};
var getAllEnvois = envoisId.Select(async envoiId => {
	var response = await httpClient.GetAsync($"v1/envois/{envoiId}");
	response.EnsureSuccessStatusCode();
	return await response.Content.ReadAsStringAsync();
});
await Task.WhenAll(getAllEnvois);
var jsonSerializerOptions = new JsonSerializerOptions {
	PropertyNameCaseInsensitive = true
};
jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
getAllEnvois.Where(response => response != null)
	.Select(response =>
		JsonSerializer.Deserialize<EnvoiQueryResult>(response.Result, jsonSerializerOptions))
	.Select((envoi, index) => new {
		index = index + 1,
		envoi.EnvoiId,
		envoi.LastEtatEnvoiHistoryEntry,
		envoi.TransportId
	})
	.Dump();
