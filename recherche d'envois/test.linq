<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi.IntegrationTest\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string webApiAddress = "http://localhost:5000";
var httpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };
var startDate = ConvertDateTimeToString(new DateTime(year: 2017, month: 11, day: 07));
var endDate = ConvertDateTimeToString(new DateTime(year: 2017, month: 11, day: 08));
//Environment.Exit(0);
var etatsEnvoiQuery = new Func<string>(() => {
	var etatsEnvoi = new [] {
		EtatEnvoiEnum.EN_COURS_D_ENVOI,
		EtatEnvoiEnum.EN_COURS_DE_TRAITEMENT
	};
	return string.Join('&', etatsEnvoi.Select(etatEnvoi => $"EtatEnvoiActuel={etatEnvoi}"));
})();
var mailPostageQuery = new Func<string>(() => {
	var mailPostages = new [] {
		MailPostage.ENVOI_PRIORITAIRE
	};
	return string.Join('&', mailPostages.Select(etatEnvoi => $"MailPostage={etatEnvoi}"));
})();
//Environment.Exit(0);
var queryPath =
	$"/v1/Envois/Rechercher-envois?{etatsEnvoiQuery}&{mailPostageQuery}&DateCreationEnvoi.From={startDate}&DateCreationEnvoi.To={endDate}&with-etat-envoi-history=true";
var query =
	new HttpRequestMessage(
		method: HttpMethod.Get,
		requestUri: new Uri(uriString: queryPath, uriKind: UriKind.Relative));
var response = await httpClient.SendAsync(query);
response.EnsureSuccessStatusCode();
//new { responseStatusCode = response.StatusCode }.Dump();
var responseContent = await response.Content.ReadAsStringAsync();
//responseContent.Dump();
JsonNode
	.Parse(responseContent)
	.AsArray()
	.Select(envoi =>
		new {
			envoiId = envoi["envoiId"].GetValue<int>(),
			subject = envoi["subject"]?.GetValue<string>(),
			mailPostage = envoi["mailPostage"].GetValue<string>(),
			transportId = envoi["transportId"].GetValue<string>(),
			lastEtatEnvoiHistoryEntry = envoi["lastEtatEnvoiHistoryEntry"].AsObject(),
			creation = envoi["etatsEnvoiHistoryEntriesList"].AsArray().Last()
		})
	.Dump();
//JsonDocument
//	.Parse(responseContent)
//	.RootElement
//	.EnumerateArray()
//	.Select(envoi => new {
//		EnvoiId = envoi.GetProperty("envoiId").GetInt32(),
//		Subject = envoi.GetProperty("subject").GetString()
//	})
//	.Dump();

static string ConvertDateTimeToString(DateTime date) =>
	date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
