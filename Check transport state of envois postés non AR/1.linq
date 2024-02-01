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
			providerOptions => providerOptions.CommandTimeout(3))
	    .Options;
var context = new EnvoiCourrierDbContext(dbContextOptions);

var envoiPosteLate = GetEnvoiPosteLate();
var envoiPosteLateId = SaveNewEnvoiAndGetId(envoiPosteLate);
new { envoiPosteLateId }.Dump();

var envoiPosteRecent = GetEnvoiPosteRecent();
var envoiPosteRecentId = SaveNewEnvoiAndGetId(envoiPosteRecent);
new { envoiPosteRecentId }.Dump();

int SaveNewEnvoiAndGetId(Envoi envoi) {
	var envoiEntity = context.Add(envoi).Entity;
	context.SaveChanges();
	envoi.TransportId = envoi.EnvoiId;
	envoi.LastEtatEnvoiHistoryEntry = envoi.EtatsEnvoiHistory.First();
	context.SaveChanges();
	return envoiEntity.EnvoiId;
}

Envoi GetEnvoiPosteLate() {
	var envoi = GetDummyEnvoi();
	var etatEnvoiHistoryEntry =
		new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
			EtatEnvoi = EtatEnvoiEnum.POSTE, 
			DateTime =
				DateTimeOffset.Parse(
					"12/08/2023 +02:00",
					CultureInfo.GetCultureInfo("fr-FR"), 
					DateTimeStyles.AssumeLocal)
		};
	envoi.EtatsEnvoiHistory =
		new List<MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry> {
			etatEnvoiHistoryEntry
		};
	return envoi;
}

Envoi GetEnvoiPosteRecent() {
	var envoi = GetDummyEnvoi();
	var etatEnvoiHistoryEntry =
		new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
			EtatEnvoi = EtatEnvoiEnum.POSTE, 
			DateTime =
				DateTimeOffset.Parse(
					"13/08/2023 +02:00",
					CultureInfo.GetCultureInfo("fr-FR"), 
					DateTimeStyles.AssumeLocal)			
		};
	envoi.EtatsEnvoiHistory =
		new List<MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry> {
			etatEnvoiHistoryEntry
		};
	return envoi;
}

Envoi GetDummyEnvoi() {
	return new Envoi {
	    TransportId = null,
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
	    MainDocumentGedId = "Test",
	    MailPostageId = 0,
	    NbRetriesLeft = 0,
	};
}