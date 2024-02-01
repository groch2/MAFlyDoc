<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi.IntegrationTest\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Json</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string sqlServer = "bdd-MAFlyDoc.int.maf.local";
const string webApiAddress = "https://api-maflydoc-intra.int.maf.local/";

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
var jsonSerializerOptions = new JsonSerializerOptions {
	PropertyNameCaseInsensitive = true,
	Converters = { new JsonStringEnumConverter() }
};
var getAllEnvois = envoisId.Select(async envoiId =>
	await httpClient.GetFromJsonAsync<EnvoiQueryResult>(
		$"v1/envois/{envoiId}", jsonSerializerOptions));
(await Task.WhenAll(getAllEnvois))
	.Select((envoi, index) => new {
		index = index + 1,
		envoi.EnvoiId,
		envoi.LastEtatEnvoiHistoryEntry,
		envoi.TransportId
	})
	.Dump();
