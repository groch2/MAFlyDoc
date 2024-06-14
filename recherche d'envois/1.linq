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
var startDate = ConvertDateTimeToString(new DateTime(year: 2024, month: 03, day: 13));
var endDate = ConvertDateTimeToString(new DateTime(year: 2024, month: 04, day: 29));
new { startDate, endDate }.Dump();
//Environment.Exit(0);
var queryPath =
	$"/v1/Envois/Recherche-envois?MailPostages=ENVOI_PRIORITAIRE&MailPostages=ENVOI_AR&EtatsEnvoiActuel=EN_COURS_D_ENVOI&EtatsEnvoiActuel=ENVOYE&DateCreationEnvoi.From={startDate}&DateCreationEnvoi.To={endDate}";
var query =
	new HttpRequestMessage(
		method: HttpMethod.Get,
		requestUri: new Uri(uriString: queryPath, uriKind: UriKind.Relative));
var response = await httpClient.SendAsync(query);
new { responseStatusCode = response.StatusCode }.Dump();

var jsonSerializerOptions = 
	new JsonSerializerOptions {
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};

static string ConvertDateTimeToString(DateTime date) =>
	date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
