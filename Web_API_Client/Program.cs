var httpClient = new HttpClient();
var maflyDocClient =
    new MyApplication.MAFlyDocClient("https://api-maflydoc-intra.int.maf.local/", httpClient);
var envoiId = await maflyDocClient.EnvoiAvecAdresseDuDestinataireEnClairAsync(
    new MAFlyDocClient.EnvoiAvecAdresseDuDestinataireEnClairCommand
    {
        Subject = "",
        Sender = new MAFlyDocClient.SenderEnvoiCommand
        {
            PersonFirstName = "John",
            PersonLastName = "Smith",
            CompanyName = "MAF",
            UserId = "john.smith@maf.fr"
        },
        AdresseDuDestinataireLignes = new[] {
                "monsieur X",
                "1 rue Bidule",
                "75001 Paris",
                "France"
            },
        MainDocumentGedId = "20240417104202845140274012",
        AttachementsGedIdList = Array.Empty<string>(),
        MailPostage = MAFlyDocClient.MailPostage.ENVOI_SIMPLE,
        Impression = null
    });
//var envoi =
//    await maflyDocClient
//        .EnvoisFromEnvoisIdListAsync(
//            comma_separated_envois_id_list: "68,75,74",
//            with_etat_envoi_history: false);
//Console.WriteLine(JsonConvert.SerializeObject(envoi, Formatting.Indented));
Console.WriteLine(new { envoiId });
Console.ReadKey(true);