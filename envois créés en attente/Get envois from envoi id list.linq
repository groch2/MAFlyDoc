<Query Kind="Program">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string ENVIRONMENT_CODE = "j1d";
async Task Main() {
	var envois_id_list =
		JsonDocument.Parse(
			File.ReadAllText(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\envois créés en attente\résultat_nouveaux_envois.json"))
				.RootElement.EnumerateArray()
					.Select(resultat_envoi =>
						new {
							original_envoi_id = resultat_envoi.GetProperty("original_envoi_id").GetInt32(),
							new_envoi_id = resultat_envoi.GetProperty("result").GetInt32()
						})
					.ToArray();
	//$"nombre d'envois: {envois_id_list.Length}".Dump();
	var envoisOriginauxByEnvoiId =
		await SelectManyAsync(
			GetItemsByPages(envois_id_list, pageSize: 400)
				.Select(chunk => chunk.Select(envoi => envoi.original_envoi_id)),
			GetEnvoisByEnvoisIdList)
				.Select(
					(envoi, index) => new {
						//index = index + 1,
						envoi.EnvoiId,					
						date_creation = envoi.EtatsEnvoiHistoryEntriesList.Last().DateTime,
						//envoi.LastEtatEnvoiHistoryEntry,
						//envoi.TransportId,
						//envoi.MailPostage,
						//envoi.Subject
					})		
				.ToDictionaryAsync(envoi => envoi.EnvoiId);
	
	var envois_list_1 =
		await SelectManyAsync(
			GetItemsByPages(envois_id_list, pageSize: 400)
				.Select(chunk => chunk.Select(envoi => envoi.new_envoi_id)),
			GetEnvoisByEnvoisIdList)
				.Select(
					envoi => new {
						envoi.EnvoiId,					
						envoi.EtatsEnvoiHistoryEntriesList,
						envoi.LastEtatEnvoiHistoryEntry,
						//envoi.TransportId,
						envoi.MailPostage,
						envoi.Subject
					})
				.ToArrayAsync();
	var envois_list_2 =
		envois_list_1
			//.Where(envoi => envoi.EtatsEnvoiHistoryEntriesList.First().Etat == EtatEnvoiEnum.ENVOYE)
			.Select(envoi =>
				new {
					original_envoi_id = envois_id_list.First(_envoi => _envoi.new_envoi_id == envoi.EnvoiId).original_envoi_id,
					new_envoi_id = envoi.EnvoiId,
					dernier_état = new { envoi.LastEtatEnvoiHistoryEntry.Etat, Date = FormatDateTime(envoi.LastEtatEnvoiHistoryEntry.DateTime) },
					//historique = envoi.EtatsEnvoiHistoryEntriesList,
					date_envoi = FormatDateTime(envoi.EtatsEnvoiHistoryEntriesList.Last().DateTime),			
					affranchissement = envoi.MailPostage,
					sujet = envoi.Subject,
				})
			.OrderBy(envoi => envoisOriginauxByEnvoiId[envoi.original_envoi_id].date_creation)
			.Select((envoi, index) =>
				new {
					index = index + 1,
					envoi.original_envoi_id,
					envoi.new_envoi_id,
					envoi.dernier_état,
					origine_date_envoi = FormatDateTime(envoisOriginauxByEnvoiId[envoi.original_envoi_id].date_creation),
					new_date_envoi = envoi.date_envoi,
					envoi.sujet,
					envoi.affranchissement,
				})
		.GroupBy(envoi => envoi.original_envoi_id)
		.SelectMany( group =>
			group.Select( item =>
				new {
					item.index,
					item.original_envoi_id,
					item.new_envoi_id,
					item.dernier_état,
					item.origine_date_envoi,
					item.new_date_envoi,
					item.sujet,
					item.affranchissement,
					doublon = group.Count() > 1 ? "OUI" : "NON"
				}));
	envois_list_2.Dump();
	
	string FormatDateTime(DateTimeOffset dateTime) => dateTime.ToString(format: "ddd dd/MM/yyyy HH:mm:ss zzzz", dateFormatter);
}

static readonly JsonElement settings = JsonDocument.Parse(File.ReadAllText(@$"C:\Users\deschaseauxr\Documents\MAFlyDoc\Get all envois\settings_{ENVIRONMENT_CODE}.json")).RootElement;
static readonly string webApiAddress = settings.GetProperty("webApiAddress").GetString();
static readonly HttpClient maflydocHttpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };
static readonly JsonSerializerOptions jsonSerializerOptions = 
	new JsonSerializerOptions {
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};
static readonly DateTimeFormatInfo dateFormatter = CultureInfo.GetCultureInfo("FR-fr").DateTimeFormat;

static async Task<IEnumerable<EnvoiQueryResult>> GetEnvoisByEnvoisIdList(IEnumerable<int> envoisIdList) {
	var commaSeparatedEnvoisIdList = string.Join(',', envoisIdList);
	var getAllEnvoisResponse =
		await maflydocHttpClient.GetAsync(
			$"/v1/Envois/Envois-from-envois-id-list?comma-separated-envois-id-list={commaSeparatedEnvoisIdList}&with-etat-envoi-history=true");
	getAllEnvoisResponse.EnsureSuccessStatusCode();
	var content = await getAllEnvoisResponse.Content.ReadAsStringAsync();
	return JsonSerializer.Deserialize<EnvoiQueryResult[]>(content, jsonSerializerOptions);
}

static async IAsyncEnumerable<U> SelectManyAsync<T, U>(
	IEnumerable<IEnumerable<T>> sourceItemsPages,
	Func<IEnumerable<T>, Task<IEnumerable<U>>> func) {
	foreach (var page in sourceItemsPages) {
		var resultItems = await func(page);
		foreach (var resultItem in resultItems) {
			yield return resultItem;
		}
	}
}

static IEnumerable<IEnumerable<T>> GetItemsByPages<T>(IEnumerable<T> itemsSource, int pageSize) {
	var nbItemsTotal = itemsSource.Count();
	var skip = 0;
	while (skip + pageSize <= nbItemsTotal) {
		yield return itemsSource.Skip(skip).Take(pageSize);
		skip += pageSize;
	}
	if (skip < nbItemsTotal) {
		yield return itemsSource.Skip(skip).Take(pageSize);
	}
}
