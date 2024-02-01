<Query Kind="Statements">
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

using var connection = new SqlConnection("Data Source = localhost; Initial Catalog = MAFlyDoc; Integrated Security = True");
connection.Open();
using var command = connection.CreateCommand();
command.CommandText = @"SELECT [EnvoiId] FROM [dbo].[Envoi];";
var dataTable = new DataTable();
new SqlDataAdapter(command).Fill(dataTable);
var envoiIdList = Enumerable.Range(0, dataTable.Rows.Count).Select(i => (int)dataTable.Rows[i][0]);
var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
var envoiPrototype = new { TransportId = "" }.GetType();
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var envois = await Task.WhenAll(envoiIdList.Select(async envoiId => {
	var response = await client.GetAsync($"envoi/{envoiId}");
	if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
		return new { envoiId, transportId = (string)null };
	}
	var content = await response.Content.ReadAsStringAsync();
	dynamic envoi = JsonSerializer.Deserialize(content, envoiPrototype, options);
	return new { envoiId, transportId = (string)envoi.TransportId };
}));
envois.Select(envoi => envoi.transportId).Where(tid => tid != null).OrderBy(long.Parse).Dump();
