<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi.IntegrationTest\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi.IntegrationTest\bin\Debug\net6.0\Newtonsoft.Json.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string webApiAddress = "http://localhost:5000/";
const string webApiVersion = "v1";
const string documentGedId = "20240417104202845140274012";
var maflyDocWebApiHttpClient =
    new HttpClient {
        BaseAddress = new Uri(webApiAddress)
    };
var envoiSubject = Guid.NewGuid().ToString("N").ToUpperInvariant();
var createEnvoiRequestJsonBody =
    System.Text.Json.JsonSerializer.Serialize(
        new {
            subject = envoiSubject,
            sender = new {
                personFirstName = "John",
                personLastName = "Smith",
                companyName = "MAF",
                userId = "john.smith@maf.fr"
            },
			adresseAuFormatAfnorLignes = new [] {
				"monsieur X",
				"1 rue Bidule",
				"Machin-Ville",
				"Pays Imaginaire"
			},
            mainDocumentGedId = documentGedId,
            attachementsGedIdList = Array.Empty<string>(),
            mailPostage = "ENVOI_SIMPLE"
        });
var requestContent =
    new StringContent(
        $"{createEnvoiRequestJsonBody}",
        Encoding.UTF8,
        "application/json");
var httpResponse =
    await maflyDocWebApiHttpClient
        .PostAsync(
            $"{webApiVersion}/Envois/Envoi-avec-adresse-en-clair",
            requestContent);
try {
	httpResponse.EnsureSuccessStatusCode();
} catch {
	var responseContent = await httpResponse.Content.ReadAsStringAsync();
	responseContent.Dump();
	httpResponse.Dump();
	throw;
}	
var envoiId =
    httpResponse.Headers.Location!.Segments[^1];
var envoiJson =
    await maflyDocWebApiHttpClient
        .GetStringAsync($"{webApiVersion}/Envois/{envoiId}");
var envoi =
    JsonConvert
        .DeserializeObject<EnvoiQueryResult>(envoiJson)!;
new { envoiId, envoi.TransportId, envoiSubject }.Dump();
var isTestSuccessful = envoi.LastEtatEnvoiHistoryEntry.Etat == EtatEnvoiEnum.EN_COURS_DE_TRAITEMENT;
new { isTestSuccessful }.Dump();
