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
var envoi =
	context
		.Envois
		.Include(envoi => envoi.EtatsEnvoiHistory)
		.Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
		.First(envoi => envoi.EnvoiId == 408);
PrintEnvoi();
var cultureFr = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
var etatEnvoiHistoryEntry =
	new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
		EtatEnvoi = EtatEnvoiEnum.POSTE, 
		DateTime = envoi.LastEtatEnvoiHistoryEntry.DateTime.AddDays(1)
	};
envoi.EtatsEnvoiHistory.Add(etatEnvoiHistoryEntry);
envoi.LastEtatEnvoiHistoryEntry = etatEnvoiHistoryEntry;
context.SaveChanges();
PrintEnvoi();

void PrintEnvoi() {
	new {
		envoi.EnvoiId,
		envoi.EtatsEnvoiHistory,
		envoi.LastEtatEnvoiHistoryEntry
	}.Dump();
}