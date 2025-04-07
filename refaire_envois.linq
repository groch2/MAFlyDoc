<Query Kind="Program">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

const string ENVIRONMENT_CODE = "int";
async Task Main() {
	var envoisIdList =
		File.ReadAllLines(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\envois créés en attente\envois_id_d_origine.txt")
			.Select(int.Parse)
			.ToArray();
	var envois_data_list = GetEnvois(envoisIdList);
	var envois_list =
		envois_data_list
			.Select(envoi =>
		        new {
					envoiId = envoi.EnvoiId,
		            subject = envoi.Subject,
		            sender = new {
		                personFirstName = envoi.Sender.PersonFirstName,
		                personLastName = envoi.Sender.PersonLastName,
		                companyName = envoi.Sender.CompanyName,
		                userId = envoi.Sender.UserId
		            },
					recipient = new {
						compteId = envoi.Recipient.CompteId,
						personneId = envoi.Recipient.PersonneId,
						adresseId = envoi.Recipient.AdresseId
					},
					AdresseDuDestinataireLignes = envoi.Recipient.AdresseAfnor.Split("\n"),
		            mainDocumentGedId = envoi.MainDocumentGedId,
		            attachementsGedIdList = (object)null,
		            mailPostage =
						envoi.MailPostageId switch {
							0 => MailPostage.ENVOI_PRIORITAIRE,
							1 => MailPostage.ENVOI_SIMPLE,
							2 => MailPostage.ENVOI_AR,
							_ => throw new Exception("affranchissement inconnu")
						},
					pageAdresse = envoi.PageAdresse.ToString(),
					impression = envoi.Impression.ToString(),
					referenceAR = envoi.ReferenceAR,
		        });
	envois_list.Dump();
	return;
	var creations_envois_commands =
		await Task.WhenAll(
			envois_list.Select(async envoi => {
				try {
					var new_envoi_id =
						envoi.recipient.compteId == null && envoi.recipient.adresseId == null ?
						await CreateEnvoiFromPlainAddressText(envoi) :
						await CreateEnvoiFromRecipientAddressId(envoi);
					return new { original_envoi_id = envoi.envoiId, result = new Either<Exception, int>(new_envoi_id) };
				} catch (Exception exception) {
					return new { original_envoi_id = envoi.envoiId, result = new Either<Exception, int>(exception) };
				}
			}));
	"envois terminés".Dump();
	var résultat =
		creations_envois_commands
			.Select(envoi_result =>
				new { success = envoi_result.result.IsRight, envoi_result.original_envoi_id, result = (object)(envoi_result.result.IsRight ?
					envoi_result.result.GetRight() :
					envoi_result.result.GetLeft().Message) });
	var résultat_sérialisé = JsonSerializer.Serialize(résultat).Dump();
	const string envoisRefaitsDirectory = @"C:\Users\deschaseauxr\Documents\MAFlyDoc\envois_refaits";
	var fileName = $"{Directory.GetFiles(envoisRefaitsDirectory).Count() + 1}.json";
	File.WriteAllText(contents: résultat_sérialisé, path: Path.Combine(envoisRefaitsDirectory, fileName));
	résultat.Dump();
	envois_list.Where(envoi => envoi.recipient.adresseId != null).Dump();
}

IEnumerable<EnvoiFromDatabase> GetEnvois(IEnumerable<int> envoisIdList) {
	var sql_query = File.ReadAllText(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\envois créés en attente\get_envois_SQL_QUERY_TEMPLATE.txt");
	sql_query = sql_query.Replace("__ENVOIS_ID_LIST__", string.Join(',', envoisIdList));
	using var connection =
		new SqlConnection($"Server=bdd-MAFlyDoc.{ENVIRONMENT_CODE}.maf.local;Database=MAFlyDoc;Integrated Security=True");
	connection.Open();
	using var command = connection.CreateCommand();
	command.CommandText = sql_query;
    using var reader = command.ExecuteReader();
	var attachementsByEnvoiId = new Dictionary<int, List<string>>();
    while (reader.Read()) {
        var envoiId = reader.GetInt32(0);
        var attachementGedId = reader.GetString(1);
		if (!attachementsByEnvoiId.ContainsKey(envoiId)) {
			attachementsByEnvoiId[envoiId] = new List<string>{ attachementGedId };
		} else {
			attachementsByEnvoiId[envoiId].Add(attachementGedId);
		}
	}
	reader.NextResult();
    while (reader.Read()) {
		var envoiId = reader.GetInt32(0);
        yield return new EnvoiFromDatabase(
            EnvoiId: envoiId,
            Application: reader.IsDBNull(1) ? null : reader.GetString(1),
            Impression: reader.IsDBNull(2) ? Impression.RECTO_VERSO : (Impression)reader.GetInt32(2),
            MailPostageId: reader.GetInt32(3),
            MainDocumentGedId: reader.GetString(4),
            PageAdresse: reader.IsDBNull(5) ? PageAdresse.SANS : (reader.GetBoolean(5) ? PageAdresse.AVEC : PageAdresse.SANS),
            ReferenceAR: reader.IsDBNull(6) ? null : reader.GetString(6),
            Subject: reader.IsDBNull(7) ? null : reader.GetString(7),
            Sender: new Sender(
                CompanyName: reader.IsDBNull(8) ? null : reader.GetString(8),
                PersonFirstName: reader.IsDBNull(9) ? null : reader.GetString(9),
                PersonLastName: reader.IsDBNull(10) ? null : reader.GetString(10),
                UserId: reader.IsDBNull(12) ? null : reader.GetString(12)
            ),
            Recipient: new Recipient(
                AdresseAfnor: reader.GetString(13),
                AdresseId: reader.IsDBNull(14) ? null : reader.GetInt32(14),
                CompteId: reader.IsDBNull(15) ? null : reader.GetInt32(15),
                PersonneId: reader.IsDBNull(16) ? null : reader.GetInt32(16)
            ),
			AttachementsGedIdList: attachementsByEnvoiId.GetValueOrDefault(envoiId)?.ToArray()
        );
    }
}

record EnvoiFromDatabase(
	int EnvoiId,
	string Application,
	Impression Impression,
	int MailPostageId,
	string MainDocumentGedId,
	PageAdresse PageAdresse,
	string ReferenceAR,
	string Subject,
	Sender Sender,
	Recipient Recipient,
	string[] AttachementsGedIdList);

record Recipient(
	string AdresseAfnor,
	int? AdresseId,
	int? CompteId,
	int? PersonneId);

record Sender(
	string CompanyName,
	string PersonFirstName,
	string PersonLastName,
	string UserId);

record EnvoiAttachement(int envoiId, string attachementGedId);

const string webApiVersion = "v1";
const string webApiAddress = $"https://api-maflydoc-intra.{ENVIRONMENT_CODE}.maf.local/";
static HttpClient maflyDocWebApiHttpClient = new HttpClient { BaseAddress = new Uri(webApiAddress) };

static async Task<int> CreateEnvoiFromPlainAddressText(dynamic envoi) {
	var createEnvoiRequestJsonBody = System.Text.Json.JsonSerializer.Serialize(envoi);
	var requestContent =
	    new StringContent(
	        $"{createEnvoiRequestJsonBody}",
	        Encoding.UTF8,
	        "application/json");
	var httpResponse =
	    await maflyDocWebApiHttpClient
	        .PostAsync(
	            $"{webApiVersion}/Envois/Envoi-avec-adresse-du-destinataire-en-clair",
	            requestContent);
	try {
		httpResponse.EnsureSuccessStatusCode();
	} catch {
		var responseContent = await httpResponse.Content.ReadAsStringAsync();
		responseContent.Dump();
		httpResponse.Dump();
		throw;
	}
	var envoiId = int.Parse(httpResponse.Headers.Location!.Segments[^1]);
	return envoiId;
}

static async Task<int> CreateEnvoiFromRecipientAddressId(dynamic envoi) {
	var createEnvoiRequestJsonBody =
	    System.Text.Json.JsonSerializer.Serialize(envoi);
	var requestContent =
	    new StringContent(
	        $"{createEnvoiRequestJsonBody}",
	        Encoding.UTF8,
	        "application/json");
	var recipientAddressIdLocation = new Func<string>(() => {
		if (envoi.recipient.adresseId != null) {
			return "AddressIdProperty";
		} else {
			return "AddressIdFromCompteId";
		}
	})();
	var httpResponse =
	    await maflyDocWebApiHttpClient
	        .PostAsync(
	            $"{webApiVersion}/Envois?recipientAddressIdLocation={recipientAddressIdLocation}",
	            requestContent);
	try {
		httpResponse.EnsureSuccessStatusCode();
	} catch {
		var responseContent = await httpResponse.Content.ReadAsStringAsync();
		responseContent.Dump();
		httpResponse.Dump();
		throw;
	}
	var envoiId = int.Parse(httpResponse.Headers.Location!.Segments[^1]);
	return envoiId;
}

class Either<TLeft, TRight> {
    private readonly object _value;

    public Either(TLeft value) {
        _value = value;
        IsLeft = true;
    }

    public Either(TRight value) {
        _value = value;
    }

    public bool IsLeft { get; }
    public TLeft GetLeft() {
        if (IsRight) {
            throw new Exception($"the original value is correct: {_value}");
        }
        return (TLeft)_value;
    }

    public bool IsRight => !IsLeft;
    public TRight GetRight() {
        if (IsLeft) {
            throw new Exception($"incorrect value: {_value}");
        }
        return (TRight)_value;
    }
}
