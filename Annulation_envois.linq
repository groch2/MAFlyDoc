<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi.IntegrationTest\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string webApiAddress = "https://api-maflydoc-intra.prd.maf.local/";
var envoiIdList = File.ReadAllText(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\envoi_id_list.txt");
var encodedEnvoiIdList = HttpUtility.UrlEncode(envoiIdList);
var httpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };
// commenter la ligne suivante pour exécuter la commande d'annulation d'envois
Environment.Exit(0);
var response =
	await httpClient.PostAsync(
		requestUri: $"/v1/Envois/Annuler-envois-from-envois-id-list?comma-separated-envois-id-list={encodedEnvoiIdList}",
		content: null);
response.Dump();
