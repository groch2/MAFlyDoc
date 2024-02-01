<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\Newtonsoft.Json.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string sqlServer = "bdd-MAFlyDoc.hom.maf.local";
const string webApiAddress = "https://api-maflydoc-intra.hom.maf.local/v1/envois/";

var mainDocumentGedIdList =
	"20231107172409476481573281,20231107224452023632540186,20231107225034105340744566"
		.Split(',')
		.Aggregate(new StringBuilder("("),
		(state, item) => state.Append($"'{item}',"),
		state => state.Remove(state.Length - 1, 1).Append(")"));
//mainDocumentGedIdList.Dump();
//Environment.Exit(0);

using var connection = new SqlConnection($"Server={sqlServer};Database=MAFlyDoc;Integrated Security=True");
connection.Open();
using var command = connection.CreateCommand();
command.CommandText = $"SELECT [EnvoiId] FROM [dbo].[Envoi] WHERE [MainDocumentGedId] IN {mainDocumentGedIdList}";
//command.CommandText.Dump();
using var dataTable = new DataTable();
using var dataAdapter = new SqlDataAdapter(command);
dataAdapter.Fill(dataTable);
var envoisIdList = dataTable.AsEnumerable().Select(r => (int)r[0]);
//envoisIdList.Dump();

var httpClient =
	new HttpClient {
		BaseAddress = new Uri(webApiAddress)
	};
var getAllEnvois = envoisIdList.Select(async envoiId => {
	var response = await httpClient.GetAsync($"{envoiId}");
	//response.Dump();
	response.EnsureSuccessStatusCode();
	return await response.Content.ReadAsStringAsync();
});
await Task.WhenAll(getAllEnvois);
getAllEnvois
	.Select(response => {
		//response.Result.Dump();
		var envoi = Newtonsoft.Json.JsonConvert.DeserializeObject<EnvoiQueryResult>(response.Result);
		return new {
			envoi.EnvoiId,
			envoi.LastEtatEnvoiHistoryEntry,
			envoi.TransportId
		};
	}).Dump();
