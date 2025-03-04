<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string environment = "int";
const string webApiVersion = "v1";
const string webApiAddress = $"https://localhost:44377/";
//const string webApiAddress = $"https://api-maflydoc-intra.{environment}.maf.local/";
var maflyDocWebApiHttpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };
const string documentGedId = "20241029175129537362218878";
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
			AdresseDuDestinataireLignes = new [] {
				"Roch Deschaseaux",
				"MAF Assurances - DOSI",
				"189 boulevard Malesherbes",
				"75856 PARIS CEDEX 17",
				"France"
			},
            mainDocumentGedId = documentGedId,
            attachementsGedIdList = (object)null,
            mailPostage = "ENVOI_AR",
			//pageAdresse = PageAdresse.AVEC,
			pageAdresse = (object)null,
			impression = (object)null,
        });
var requestContent =
    new StringContent(
        $"{createEnvoiRequestJsonBody}",
        Encoding.UTF8,
        "application/json");
var httpResponse =
    await maflyDocWebApiHttpClient
        .PostAsync(
            $"{webApiVersion}/Envois/Envoi-avec-adresse-du-destinataire-en-clair",
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
