<Query Kind="Statements">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
</Query>

var client = new HttpClient();
var jsonDocument = JsonNode.Parse(await client.GetStringAsync("http://localhost:5000/swagger/v1/swagger.json"));
var jsonNode = (JsonObject)jsonDocument["components"]["schemas"]["DocumentsArTelechargesGedIdQueryResult"];
jsonNode.Add("nullable", true);
jsonNode.Dump();