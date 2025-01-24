<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(1))
	    .Options;
var envoiCourrierDbContext = new EnvoiCourrierDbContext(dbContextOptions);
envoiCourrierDbContext.Envois
    .Include(envoi => envoi.AttachementsList)
    .Include(envoi => envoi.EtatsEnvoiHistory)
    .Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
	.ToList()
	.ForEach(DeleteEnvoiFromDatabase);
envoiCourrierDbContext.SaveChanges();

var envoiIdList =
	File
		.ReadAllLines(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\Empêcher le plantage des traitements de fond sur un envoi\envois initialization data.txt")
		.Select(line => {
			var data = line.Split(',');
			var transportId = data[0];
			var envoiConfirmationDateTime =
				DateTimeOffset.ParseExact(input: data[1], format: "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
			var envoiInitialisationDateTime = envoiConfirmationDateTime.AddMinutes(-1);
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
			    MailPostageId = MailPostage.ENVOI_AR,
			    NbRetriesLeft = 0,
			};
			var envoiInitialisationHistoryEntry =
				new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
					EtatEnvoi = EtatEnvoiEnum.EN_COURS_D_ENVOI, 
					DateTime = envoiInitialisationDateTime,
				};		
			var envoiConfirmationHistoryEntry =
				new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
					EtatEnvoi = EtatEnvoiEnum.EN_COURS_DE_TRAITEMENT, 
					DateTime = envoiConfirmationDateTime,
				};
			envoi.EtatsEnvoiHistory =
				new List<MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry> {
					envoiInitialisationHistoryEntry,
					envoiConfirmationHistoryEntry,
				};
			return new { envoi, lastEtatEnvoi = envoiConfirmationHistoryEntry };
		})
		.Select(item => {
			var envoi = item.envoi;
			var envoiEntity = envoiCourrierDbContext.Add(envoi).Entity;
			envoiCourrierDbContext.SaveChanges();
			envoi.LastEtatEnvoiHistoryEntry = item.lastEtatEnvoi;
			envoiCourrierDbContext.SaveChanges();
			return envoi.EnvoiId;
		});
envoiIdList.Dump();

void DeleteEnvoiFromDatabase(MAFlyDoc.WebApi.Database.Model.Envoi envoiToDelete) {
	if (envoiToDelete.LastEtatEnvoiHistoryEntry != null) {
	    envoiCourrierDbContext
	        .EtatEnvoiHistoryEntries
	        .Remove(envoiToDelete.LastEtatEnvoiHistoryEntry);
		envoiCourrierDbContext.SaveChanges();
	}
	if (envoiToDelete.EtatsEnvoiHistory != null) {
	    var envoiEtatHistory =
	        envoiToDelete
	            .EtatsEnvoiHistory
	            .Except(new[] { envoiToDelete.LastEtatEnvoiHistoryEntry });
	    envoiCourrierDbContext
	        .EtatEnvoiHistoryEntries
	        .RemoveRange(
	            envoiEtatHistory!);
		envoiCourrierDbContext.SaveChanges();
	}
	envoiCourrierDbContext.Envois.Remove(envoiToDelete);
	envoiCourrierDbContext.SaveChanges();
}