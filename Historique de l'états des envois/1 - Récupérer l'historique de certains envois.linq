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
	var envoisId = "1,2,4,5".Split(',');
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
					envoi.etatActuelDeLEnvoi,
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
	EtatEnvoiHistoryEntry etatActuelDeLEnvoi,
	string mailPostage,
	string transportId,
	EtatEnvoiHistoryEntry[] historiqueDesEtats);

public record EtatEnvoiHistoryEntry {
    public string Etat { get; init; }
    public DateTimeOffset DateTime { get; init; }
}

static DateOnly? GetDateOnly(DateTime? date) =>
	date.HasValue ? DateOnly.FromDateTime(date.Value): (DateOnly?)null;
