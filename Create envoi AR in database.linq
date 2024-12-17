<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
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
        TransportId = null,
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
        MainDocumentGedId = "20241029110226501112818622",
        MailPostageId = MailPostage.ENVOI_AR,
        NbRetriesLeft = 0,
		AccuseDeReceptionNumeriseParEsker = true,
		DocumentsArTelecharges = new DocumentsArTelecharges(),
		PreuveDeDepotReference = "3993F35C938744FF865C057A2036FE2C",
		Subject = "4981071E76D34E74833BEF4A8E9F1206"
    };
var etatEnvoiEnCoursDeTraitement =
    new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
        DateTime = DateTimeOffset.UtcNow,
        EtatEnvoi = EtatEnvoiEnum.EN_COURS_D_ENVOI
    };
envoi.EtatsEnvoiHistory = new[] { etatEnvoiEnCoursDeTraitement };
var envoiEntity = (await context.Envois.AddAsync(envoi)).Entity;
await context.SaveChangesAsync();
envoi.LastEtatEnvoiHistoryEntry = etatEnvoiEnCoursDeTraitement;
await context.SaveChangesAsync();
var envoiId = envoiEntity.EnvoiId;
new { envoiId }.Dump();
