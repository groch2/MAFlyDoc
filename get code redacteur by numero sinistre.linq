<Query Kind="Program">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

const string ENVIRONNEMENT_MAF = "j1d";
async Task Main() {
	var numerosSinistreList = File.ReadAllLines(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\numeros sinistre.txt");
	var codesRedacteurByNumeroSinistre =
		await Task.WhenAll(
			numerosSinistreList
				.Select(async numeroSinistre =>
					new {
						NumeroSinistre = numeroSinistre,
						CodeRedacteur = await GetCodeRedacteurByNumeroSinistre(numeroSinistre)
					}));
	codesRedacteurByNumeroSinistre.Select(item => {
		var codeRedacteur = item.CodeRedacteur.IsRight ? item.CodeRedacteur.GetRight() : item.CodeRedacteur.GetLeft().Message;
		return new {
			item.NumeroSinistre,
			CodeRedacteur = codeRedacteur,
			IsCodeRedacteurOk = item.CodeRedacteur.IsRight,
		};
	})
	.Dump();
}

HttpClient SinappsClient = new HttpClient { BaseAddress = new Uri($"https://api-sinapps-intra.{ENVIRONNEMENT_MAF}.maf.local") };
async Task<Either<ApplicationException, string>> GetCodeRedacteurByNumeroSinistre(string numeroSinistre) {
	const string _sinappsApiVersion = "v1";	
	var formattedNumeroSinistre = FormatNumeroSinistre(numeroSinistre);
	using var request =
	    BuildHttpGetRequestAcceptingJsonOnly(
	        new Uri($"api/{_sinappsApiVersion}/sinistres/{formattedNumeroSinistre}", UriKind.Relative));
	using var response = await SinappsClient.SendAsync(request);
	if (!response.IsSuccessStatusCode) {
	    return new Either<ApplicationException, string>(new ApplicationException(response.ReasonPhrase));
	}
	var json = await response.Content.ReadAsStringAsync();
	var codeRedacteur = JsonDocument.Parse(json).RootElement.GetProperty("codeRedacteur").GetString();
	return new Either<ApplicationException, string>(codeRedacteur);
}

static string FormatNumeroSinistre(string numeroSinistre) {
    var formattedNumeroSinistre = string.Join('-', SplitStringBySizes(numeroSinistre, [2, 2, 3]));
    formattedNumeroSinistre = $"{formattedNumeroSinistre[..^1]}-{formattedNumeroSinistre[^1]}";
    return formattedNumeroSinistre;
}

static HttpRequestMessage BuildHttpGetRequestAcceptingJsonOnly(Uri address) {
    var request = new HttpRequestMessage(HttpMethod.Get, address);
    request.Headers.Accept.Clear();
    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    return request;
}

static IEnumerable<string> SplitStringBySizes(string input, IEnumerable<int> sizes) {
    if (string.IsNullOrEmpty(input) || sizes == null || !sizes.Any()) {
        throw new ArgumentException("La chaîne d'entrée et la liste des tailles doivent être valides.");
    }
    var start = 0;
    foreach (var size in sizes) {
        if (start >= input.Length) {
            break;
        }
        var stop = Math.Min(start + size, input.Length);
        yield return input[start..stop];
        start = stop;
    }
    if (start < input.Length) {
        yield return input[start..];
    }
}

public class Either<TLeft, TRight> {
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
