using MAFlyDoc;

var httpClient = new HttpClient { BaseAddress = new Uri("https://api-maflydoc-intra.int.maf.local/") };
var maflyDocClient =
    new MAFlyDocApiClient(httpClient);
var response = await maflyDocClient.EnvoiAvecAdresseDuDestinataireEnClairAsync(
    new EnvoiAvecAdresseDuDestinataireEnClairCommand
    (
        @subject: "",
        @sender: new SenderEnvoiCommand
        (
            @personFirstName: "John",
            @personLastName: "Smith",
            @companyName: "MAF",
            @userId: "john.smith@maf.fr"
        ),
        @adresseDuDestinataireLignes: new[] {
                "monsieur X",
                "1 rue Bidule",
                "75001 Paris",
                "France"
            },
        @mainDocumentGedId: "20240417104202845140274012",
        @attachementsGedIdList: Array.Empty<string>(),
        @mailPostage: MailPostage.ENVOI_SIMPLE,
        @impression: null,
        application: null
    ));
var location =
    response.Headers.First(
        header => string.Equals(header.Key, "location", StringComparison.OrdinalIgnoreCase))
    .Value.First();
//var envoi =
//    await maflyDocClient
//        .EnvoisFromEnvoisIdListAsync(
//            comma_separated_envois_id_list: "68,75,74",
//            with_etat_envoi_history: false);
//Console.WriteLine(JsonConvert.SerializeObject(envoi, Formatting.Indented));
Console.WriteLine(new { location });
Console.ReadKey(true);