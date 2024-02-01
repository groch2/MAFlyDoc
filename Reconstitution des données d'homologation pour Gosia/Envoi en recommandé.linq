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
			"Server=bdd-maflydoc.hom.maf.local;Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(3))
	    .Options;
var context = new EnvoiCourrierDbContext(dbContextOptions);
var envoiInitialisationDateTime =
	DateTimeOffset.Parse(
		"01/01/2022 +02:00",
		CultureInfo.GetCultureInfo("fr-FR"), 
		DateTimeStyles.AssumeLocal);
var recipient = context.Set<Recipient>().First(r => r.RecipientId == 1246);
var sender = context.Set<Sender>().First(s => s.SenderId == 1246);
const string transportId = "1115397389744915723";
var envoi = new Envoi
{
    TransportId = long.Parse(transportId),
    Sender = sender,
    Recipient = recipient,
    MainDocumentGedId = "20231107172409476481573281",
    MailPostageId = MailPostage.ENVOI_AR,
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

var envoiEntity = context.Add(envoi).Entity;
context.SaveChanges();
envoi.LastEtatEnvoiHistoryEntry = envoi.EtatsEnvoiHistory.First();
context.SaveChanges();
envoiEntity.EnvoiId.Dump();