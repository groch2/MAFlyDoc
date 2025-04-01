<Query Kind="Program">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

const string environment = "int";
async Task Main() {
	var raw = File.ReadAllText(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\envois créés en attente\envois à refaire.json");
	var envois_list =
		JsonDocument.Parse(raw).RootElement.EnumerateArray()
			.Select(item => JsonSerializer.Deserialize<Envoi>(item.GetRawText()))
			.Select(envoi =>
		        new {
					envoiId = envoi.EnvoiId,
		            subject = envoi.Subject,
		            sender = new {
		                personFirstName = envoi.PersonFirstName,
		                personLastName = envoi.PersonLastName,
		                companyName = envoi.CompanyName,
		                userId = envoi.UserId
		            },
					recipient = new {
						compteId = envoi.CompteId,
						personneId = envoi.PersonneId,
						adresseId = envoi.AdresseId
					},					
					AdresseDuDestinataireLignes = envoi.AdresseAfnor.Split("\n"),
		            mainDocumentGedId = envoi.MainDocumentGedId,
		            attachementsGedIdList = (object)null,
		            mailPostage = envoi.MailPostageId switch { 0 => "ENVOI_PRIORITAIRE", 1 => "ENVOI_SIMPLE", 2 => "ENVOI_AR", _ => throw new Exception("affranchissement inconnu") },
					pageAdresse = envoi.PageAdresse switch { "0" => "SANS", "1" => "AVEC", _ => throw new Exception("paramètre de page d'adresse inconnu") },
					impression = envoi.Impression switch { 0 => "RECTO_VERSO", 1 => "RECTO", _ => throw new Exception("paramètre d'impression inconnu") },
					referenceAR = envoi.ReferenceAR,
		        });
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
					envoi_result.result.GetLeft()) });
	var résultat_sérialisé = JsonSerializer.Serialize(résultat).Dump();
	File.WriteAllText(contents: résultat_sérialisé, path: @"C:\Users\deschaseauxr\Documents\MAFlyDoc\envois créés en attente\résultat_nouveaux_envois_2.json");
	résultat.Dump();
	//envois_list.Where(envoi => envoi.recipient.adresseId != null).Dump();
}

const string webApiVersion = "v1";
const string webApiAddress = $"https://api-maflydoc-intra.{environment}.maf.local/";
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

record Envoi(
	int EnvoiId,
	string Application,
	int Impression,
	int MailPostageId,
	string MainDocumentGedId,
	string PageAdresse,
	string ReferenceAR,
	string Subject,
	string CompanyName,
	string PersonFirstName,
	string PersonLastName,
	string UserId,
	string AdresseAfnor,
	int? AdresseId,
	int? CompteId,
	int? PersonneId);

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
