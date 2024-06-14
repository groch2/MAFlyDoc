<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.EnableSensitiveDataLogging()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(60))
	    .Options;
var context = new EnvoiCourrierDbContext(dbContextOptions);
var envoi = new Envoi
{
    TransportId = 0,
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
var envoiCreationDateTime =
	DateTimeOffset.Parse(
		"02/03/2020 +02:00",
		CultureInfo.GetCultureInfo("fr-FR"),
		DateTimeStyles.AssumeLocal);
await CreateEnvoi(envoi, envoiCreationDateTime, context);

var envoiUpdateDateTime =
	DateTimeOffset.Parse(
		"04/05/2020 +02:00",
		CultureInfo.GetCultureInfo("fr-FR"),
		DateTimeStyles.AssumeLocal);
await UpdateEnvoiEtat(envoi, envoiUpdateDateTime, context);

new { envoi.EnvoiId }.Dump();

static async Task CreateEnvoi(
	Envoi envoi,
	DateTimeOffset creationDate,
	EnvoiCourrierDbContext context) {
	var etatEnvoiHistoryEntry =
		new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
			EtatEnvoi = EtatEnvoiEnum.EN_COURS_D_ENVOI,
			DateTime = creationDate
		};
	envoi.EtatsEnvoiHistory =
		new List<MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry> {
			etatEnvoiHistoryEntry
		};
	var envoiEntity = context.Add(envoi).Entity;
	await context.SaveChangesAsync();
	envoi.LastEtatEnvoiHistoryEntry = etatEnvoiHistoryEntry;
	await context.SaveChangesAsync();
}

static async Task UpdateEnvoiEtat(
	Envoi envoi,
	DateTimeOffset envoiUpdateDate,
	EnvoiCourrierDbContext context) {
	var etatEnvoiHistoryEntry =
		new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
			EtatEnvoi = EtatEnvoiEnum.EN_COURS_DE_TRAITEMENT,
			DateTime = envoiUpdateDate
		};
	envoi.LastEtatEnvoiHistoryEntry = etatEnvoiHistoryEntry;
	envoi.EtatsEnvoiHistory.Add(etatEnvoiHistoryEntry);
	context.Update(envoi);
	await context.SaveChangesAsync();
}