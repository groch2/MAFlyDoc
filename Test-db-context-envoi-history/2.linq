<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.EnableSensitiveDataLogging()
		.UseSqlServer(
			"Server=localhost;Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(60))
	    .Options;
var context = new EnvoiCourrierDbContext(dbContextOptions);
var envoi = new Envoi
{
    TransportId = default,
    Etat = EtatEnvoiEnum.NON_INITIALISE,
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
var cultureFr = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
var etatEnvoiHistoryEntry_1 =
	new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
		EtatEnvoi = EtatEnvoiEnum.NON_INITIALISE, 
		DateTime = DateTime.Parse("09/07/2007", cultureFr)
	};
envoi.EtatsEnvoiHistory =
	new List<MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry> {
		etatEnvoiHistoryEntry_1
	};
var envoiEntity = context.Add(envoi).Entity;
context.SaveChanges();
envoi.LastEtatEnvoiHistoryEntry = etatEnvoiHistoryEntry_1;
context.SaveChanges();
PrintEnvoi();
var etatEnvoiHistoryEntry_2 =
	new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
		EtatEnvoi = EtatEnvoiEnum.INITIALISE, 
		DateTime = etatEnvoiHistoryEntry_1.DateTime.AddDays(1)
	};
envoi.EtatsEnvoiHistory.Add(etatEnvoiHistoryEntry_2);
envoi.LastEtatEnvoiHistoryEntry = etatEnvoiHistoryEntry_2;
context.SaveChanges();
PrintEnvoi();

void PrintEnvoi() {
	new {
		envoiEntity.EnvoiId,
		envoiEntity.EtatsEnvoiHistory,
		envoiEntity.LastEtatEnvoiHistoryEntry
	}.Dump();
}