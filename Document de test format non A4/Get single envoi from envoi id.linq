<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi.IntegrationTest\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

//const string webApiAddress = "https://api-maflydoc-intra.int.maf.local/";
const string webApiAddress = "http://localhost:5000/";
var envoiId = 31;
var httpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };
var response = await httpClient.GetAsync($"v1/envois/{envoiId}?with-etat-envoi-history=true");
response.EnsureSuccessStatusCode();
var responseContent = await response.Content.ReadAsStringAsync();
var jsonSerializerOptions = new JsonSerializerOptions {
	PropertyNameCaseInsensitive = true
};
jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
var envoi =
	JsonSerializer.Deserialize<EnvoiQueryResult>(responseContent, jsonSerializerOptions);
new {
	envoi.EnvoiId,
	envoi.Subject,
	envoi.TransportId,
	envoi.EtatsEnvoiHistoryEntriesList
}.Dump();
