<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(3))
	    .Options;
using var context = new EnvoiCourrierDbContext(dbContextOptions);
var envoi =
    new MAFlyDoc.WebApi.Database.Model.Envoi {
        TransportId = 954622401926341001,
        Sender = new MAFlyDoc.WebApi.Database.Model.Sender {
            PersonFirstName = "Test",
            PersonLastName = "Test",
            CompanyName = "Test",
            SenderId = 0,
            UserId = "Test",
        },
        Recipient = new MAFlyDoc.WebApi.Database.Model.Recipient {
            AdresseAfnor = "Test",
            AdresseId = 0,
            CompteId = 0,
            PersonneId = 0,
        },
        MainDocumentGedId = "20240708135151448765050288",
        MailPostageId = MailPostage.ENVOI_AR,
        NbRetriesLeft = 0,
		DocumentsArTelecharges = new()
    };
var etatEnvoiEnCoursDeTraitement =
    new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
        DateTime = DateTimeOffset.UtcNow,
        EtatEnvoi = EtatEnvoiEnum.EN_COURS_DE_TRAITEMENT
    };
envoi.EtatsEnvoiHistory = new[] { etatEnvoiEnCoursDeTraitement };
var envoiEntity = (await context.Envois.AddAsync(envoi)).Entity;
await context.SaveChangesAsync();
envoi.LastEtatEnvoiHistoryEntry = etatEnvoiEnCoursDeTraitement;
await context.SaveChangesAsync();
var envoiId = envoiEntity.EnvoiId;
new { envoiId }.Dump();
