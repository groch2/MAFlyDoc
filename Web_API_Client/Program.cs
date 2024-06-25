using MAFlyDoc;
using System.Text.Json;

var httpClient = new HttpClient { BaseAddress = new Uri("https://api-maflydoc-intra.hom.maf.local/") };
var maflyDocClient =
    new MAFlyDocApiClient(httpClient);
var httpResponse =
    await maflyDocClient
        .EnvoisFromEnvoisIdListAsync(
            comma_separated_envois_id_list: "2026,2028,2029",
            with_etat_envoi_history: false);
var envois = httpResponse.Result;
Console.WriteLine(
    JsonSerializer.Serialize(
        envois, new JsonSerializerOptions { WriteIndented = true }));
Console.ReadKey(true);