<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
</Query>

const string ENVIRONMENT_CODE = "int";
var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.EnableSensitiveDataLogging()
		.UseSqlServer(
			$"Server=bdd-MAFlyDoc.{ENVIRONMENT_CODE}.maf.local;Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(3))
	    .Options;
var context = new EnvoiCourrierDbContext(dbContextOptions);
var envoiInitialisationDateTime =
	DateTimeOffset.Parse(
		"01/01/2025 10:00:00 +01:00",
		CultureInfo.GetCultureInfo("fr-FR"), 
		DateTimeStyles.AssumeLocal);
var envoiConfirmationDateTime = envoiInitialisationDateTime.AddMinutes(1);
//new { envoiInitialisationDateTime, envoiConfirmationDateTime }.Dump();
//Environment.Exit(0);
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
		    MainDocumentGedId = "20250121091615772173743005",
		    MailPostageId = MailPostage.ENVOI_SIMPLE,
		    NbRetriesLeft = 0,
			Subject = "7D74686C3BB8434FB786269DF00B3D91",
		};
		var envoiInitialisationHistoryEntry =
			new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
				EtatEnvoi = EtatEnvoiEnum.EN_COURS_DE_TRAITEMENT, 
				DateTime = envoiConfirmationDateTime,
			};
		var envoiConfirmationHistoryEntry =
			new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
				EtatEnvoi = EtatEnvoiEnum.EN_COURS_D_ENVOI, 
				DateTime = envoiInitialisationDateTime,
			};
		envoi.EtatsEnvoiHistory =
			new List<MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry> {
				envoiInitialisationHistoryEntry,
				envoiConfirmationHistoryEntry,
			};
		return new { envoi, lastEtatEnvoi = envoiConfirmationHistoryEntry };
	})
	.ToList()
	.ForEach(item => {
		var envoi = item.envoi;
		var envoiEntity = context.Add(envoi).Entity;
		context.SaveChanges();
		envoi.LastEtatEnvoiHistoryEntry = item.lastEtatEnvoi;
		context.SaveChanges();
		new { envoi.EnvoiId }.Dump();
	});
