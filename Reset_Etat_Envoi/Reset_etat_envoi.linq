<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

const int envoiId = 1;
var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(1))
	    .Options;
using var context = new EnvoiCourrierDbContext(dbContextOptions);
var envoi =
	context.Envois
		.Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
		.Include(envoi => envoi.EtatsEnvoiHistory)
		.First(envoi => envoi.EnvoiId == envoiId);
envoi.LastEtatEnvoiHistoryEntry = null;
envoi.EtatsEnvoiHistory.Clear();
await context.SaveChangesAsync();
var etatEnvoi =
	new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
		EtatEnvoi = EtatEnvoiEnum.ENVOYE,
		DateTime = DateTimeOffset.Parse("2024-07-17")
	};
envoi.LastEtatEnvoiHistoryEntry = etatEnvoi;
envoi.EtatsEnvoiHistory.Add(etatEnvoi);
await context.SaveChangesAsync();
"terminé".Dump();