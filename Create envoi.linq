<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi.IntegrationTest\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Json</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string webApiAddress = "http://localhost:5000/";
const string webApiVersion = "v1";
const string documentGedId = "20240129130549386056188030";

var maflyDocWebApiHttpClient =
    new HttpClient { BaseAddress = new Uri(webApiAddress) };
var createEnvoiRequestJsonBody =
    JsonNode.Parse(
        File.ReadAllText(
			path: @"C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi.IntegrationTest\create-envoi-body.json"));
createEnvoiRequestJsonBody!.Root["mainDocumentGedId"] = documentGedId;
var requestContent =
    JsonContent.Create(
        createEnvoiRequestJsonBody);
var stopwatch = Stopwatch.StartNew();
var httpResponse =
    await maflyDocWebApiHttpClient
        .PostAsync(
            $"{webApiVersion}/Envois?recipientAddressIdLocation=AddressIdProperty",
            requestContent);
try {
	httpResponse.EnsureSuccessStatusCode();
} catch {
	httpResponse.Dump();
	throw;
}	
stopwatch.Stop();
new { createEnvoiTime = stopwatch.Elapsed }.Dump();
var envoiJson =
    await maflyDocWebApiHttpClient
        .GetStringAsync(httpResponse.Headers.Location);
var jsonSerializerOptions =
	new JsonSerializerOptions {
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }		
	};
var envoi =
    JsonSerializer
        .Deserialize<EnvoiQueryResult>(envoiJson, jsonSerializerOptions)!;

var isTestSuccessful = envoi.LastEtatEnvoiHistoryEntry.Etat == EtatEnvoiEnum.EN_COURS_DE_TRAITEMENT;
new { isTestSuccessful }.Dump();

new { envoi_address = httpResponse.Headers.Location }.Dump();
