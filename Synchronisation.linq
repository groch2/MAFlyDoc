<Query Kind="Statements">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string webApiAddress = "https://api-maflydoc-intra.prd.maf.local/";
var httpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };
// commenter la ligne suivante pour exécuter la commande de synchronisation de l'état des envois
//var input = Console.ReadLine();
//new { input }.Dump();
//Environment.Exit(0);
var response =
	await httpClient.PostAsync(
		requestUri: $"/v1/Envois/Synchronisation",
		content: null);
new { response.StatusCode, response.ReasonPhrase }.Dump();
response.EnsureSuccessStatusCode();
"Synchronisation lancée à: ".Dump();
DateTime.Now.Dump();