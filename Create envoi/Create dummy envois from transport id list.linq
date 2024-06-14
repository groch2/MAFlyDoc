<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.EnableSensitiveDataLogging()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(60))
	    .Options;
var context = new EnvoiCourrierDbContext(dbContextOptions);
var envoiInitialisationDateTime =
	DateTimeOffset.Parse(
		"01/01/2022 +02:00",
		CultureInfo.GetCultureInfo("fr-FR"), 
		DateTimeStyles.AssumeLocal);
File
	.ReadAllLines(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\TransportsIdList.txt")
	.Select(transportId => {
		var envoi = new Envoi
		{
		    TransportId = long.Parse(transportId),
		    Sender = new Sender
		    {
		        PersonFirstName = "Test",
		        PersonLastName = "Test",
		        CompanyName = "Test",
		        SenderId = 0,
		        UserId = "Test",
		    },
		    Recipient = new Recipient
		    {
		        AdresseAfnor = "Test",
		        AdresseId = 0,
		        CompteId = 0,
		        PersonneId = 0,
		    },
		    MainDocumentGedId = "20240606130915430131474537",
		    MailPostageId = 0,
		    NbRetriesLeft = 0,
		};
		var etatEnvoiHistoryEntry =
			new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
				EtatEnvoi = EtatEnvoiEnum.EN_COURS_DE_TRAITEMENT, 
				DateTime = envoiInitialisationDateTime
			};
		envoi.EtatsEnvoiHistory =
			new List<MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry> {
				etatEnvoiHistoryEntry
			};
		return envoi;
	})
	.ToList()
	.ForEach(envoi => {
		var envoiEntity = context.Add(envoi).Entity;
		context.SaveChanges();
		envoi.LastEtatEnvoiHistoryEntry = envoi.EtatsEnvoiHistory.First();
		context.SaveChanges();
	});
