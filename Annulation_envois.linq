<Query Kind="Statements">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string environment_code = "int";
const string webApiAddress = $"https://api-maflydoc-intra.{environment_code}.maf.local/";
//const string webApiAddress = "https://localhost:44377/";
const string filePathEnvoisId = @"C:\Users\deschaseauxr\Documents\MAFlyDoc\envoi_id_list.txt";
var envoiIdList = HttpUtility.UrlEncode(string.Join(separator: ',', File.ReadAllLines(filePathEnvoisId)));
var httpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };
// commenter la ligne suivante pour exécuter la commande d'annulation d'envois
Environment.Exit(0);
var response =
	await httpClient.PostAsync(
		requestUri: $"/v1/Envois/Annuler-envois-from-envois-id-list?comma-separated-envois-id-list={envoiIdList}",
		content: null);
new {
	response.IsSuccessStatusCode,
	response.StatusCode,
	response.ReasonPhrase
}.Dump();
