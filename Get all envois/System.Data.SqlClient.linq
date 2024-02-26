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
var commaSeparatedEnvoisIdList = string.Join(',', envoisId);

var httpClient =
	new HttpClient {
		BaseAddress = new Uri(webApiAddress)
	};
var getAllEnvoisResponse =
	await httpClient.GetAsync(
		$"/v1/Envois/Envois-from-envois-id-list?comma-separated-envois-id-list={commaSeparatedEnvoisIdList}");
getAllEnvoisResponse.EnsureSuccessStatusCode();

var jsonSerializerOptions = 
	new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
var content = await getAllEnvoisResponse.Content.ReadAsStringAsync();
JsonSerializer.Deserialize<EnvoiQueryResult[]>(content, jsonSerializerOptions)
	.Dump();
