<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi.IntegrationTest\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi.IntegrationTest\bin\Debug\net6.0\Newtonsoft.Json.dll</Reference>
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

var settings = JsonDocument.Parse(File.ReadAllText(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\Get all envois\settings.json")).RootElement;
var sqlServer = settings.GetProperty("sqlServer").GetString();
var webApiAddress = settings.GetProperty("webApiAddress").GetString();

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.EnableSensitiveDataLogging()
		.UseSqlServer(
			$"Server={sqlServer};Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(3))
	    .Options;
using var context = new EnvoiCourrierDbContext(dbContextOptions);

var httpClient =
	new HttpClient {
		BaseAddress = new Uri(webApiAddress)
	};
var getAllEnvois =
	context
		.Set<MAFlyDoc.WebApi.Database.Model.Envoi>()
		.Select(envoi => envoi.EnvoiId)
		.ToArray()
		.Select(async envoiId =>
			await httpClient.GetStringAsync($"v1/envois/{envoiId}"));
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
