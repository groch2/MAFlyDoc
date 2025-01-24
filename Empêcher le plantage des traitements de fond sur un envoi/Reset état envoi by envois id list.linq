<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

var envoisIdList =
	File
		.ReadAllLines(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\Empêcher le plantage des traitements de fond sur un envoi\envois id list.txt")
		.Select(int.Parse)
		.ToHashSet();
//envoisIdList.Dump();
//Environment.Exit(0);
var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(1))
	    .Options;
var context = new EnvoiCourrierDbContext(dbContextOptions);
var envois =
	context.Envois
		.Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
		.Include(envoi => envoi.EtatsEnvoiHistory)
		.Where(envoi => envoisIdList.Contains(envoi.EnvoiId))
		.ToArray();
var envoiInitialisationDateTime =
	DateTimeOffset.Parse(
		"01/01/2025 10:00:00 +01:00",
		CultureInfo.GetCultureInfo("fr-FR"), 
		DateTimeStyles.AssumeLocal);
var envoiConfirmationDateTime = envoiInitialisationDateTime.AddMinutes(1);
envois
	.Select(envoi => {
		envoi.LastEtatEnvoiHistoryEntry = null;
		envoi.EtatsEnvoiHistory.Clear();
		context.SaveChanges();

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
		envoi.LastEtatEnvoiHistoryEntry = envoiConfirmationHistoryEntry;
		return context.SaveChanges();
	});
"terminé".Dump();