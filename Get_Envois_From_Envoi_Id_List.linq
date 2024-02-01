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

const string webApiAddress = "https://api-maflydoc-intra.prd.maf.local/";
var envoisId =
	File
		.ReadAllText(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\envoi_id_list.txt")
		.Split(',')
		.Select(int.Parse);
var httpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };
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
getAllEnvois
	.Select(response =>
		JsonSerializer.Deserialize<EnvoiQueryResult>(response.Result, jsonSerializerOptions))
	.Select((envoi, index) => new {
		index = index + 1,
		envoi.EnvoiId,
		envoi.LastEtatEnvoiHistoryEntry,
		envoi.TransportId
	})
	.Dump();
