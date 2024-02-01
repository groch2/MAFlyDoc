<Query Kind="Program">
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

async Task Main()
{
	using var connection = new SqlConnection("Server=(local);Database=MAFlyDoc;Integrated Security=True");
	connection.Open();
	using var command = connection.CreateCommand();
	command.CommandText = "SELECT [EnvoiId] FROM [dbo].[Envoi]";
	var dataTable = new DataTable();
	var dataAdapter = new SqlDataAdapter(command);
	dataAdapter.Fill(dataTable);
	var envoisId = dataTable.AsEnumerable().Select(r => (int)r[0]);

	var httpClient =
		new HttpClient {
			BaseAddress = new Uri("http://localhost:5000/")
		};
	var getAllEnvois = envoisId.Select(async envoiId => {
		var response = await httpClient.GetAsync($"/envois/{envoiId}");
		return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
	});
	await Task.WhenAll(getAllEnvois);
	var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
	var allEnvoisInitialisés =
		getAllEnvois
			.Where(response => response.Result != null)
			.Select(response => {
				var envoi = JsonSerializer.Deserialize<Envoi>(response.Result, options);
				var etatsEnvoiHistorique =
					JsonDocument
						.Parse(response.Result)
						.RootElement
						.GetProperty("historiqueDesEtats")
						.EnumerateArray()
						.Select(json => JsonSerializer.Deserialize<EtatEnvoiHistoryEntry>(json, options))
						.ToArray();
				return new {
					envoi.envoiId,
					envoi.mailPostage,
					envoi.status,
					dateCourrierPoste = GetDateOnly(envoi.dateCourrierPoste),
					dateCourrierRetourneNonDelivre = GetDateOnly(envoi.dateCourrierRetourneNonDelivre),
					envoi.transportId,
					etatsEnvoiHistorique
				};	
			});
			//.Where(envoi => envoi.status switch {
			//	"INITIALISE" or
			//	"NON_INITIALISE" => false,
			//	_ => true,
			//});
	allEnvoisInitialisés.Dump();
}

record Envoi(
	int envoiId,
	string status,
	string mailPostage,
	string transportId,
	DateTime? dateCourrierPoste,
	DateTime? dateCourrierRetourneNonDelivre,
	EtatEnvoiHistoryEntry[] historiqueDesEtats);

public record EtatEnvoiHistoryEntry {
    public string Etat { get; init; }
    public DateTimeOffset DateTime { get; init; }
}

static DateOnly? GetDateOnly(DateTime? date) =>
	date.HasValue ? DateOnly.FromDateTime(date.Value): (DateOnly?)null;
