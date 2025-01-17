<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer("Server=(local);Database=MAFlyDoc;Trusted_Connection=True;", providerOptions => providerOptions.CommandTimeout(60))
	    .Options;
var context = new EnvoiCourrierDbContext(dbContextOptions);
var envoiIdList = "1,2,3".Split(',').Select(int.Parse).ToHashSet();
var envoisToDelete =
	context
		.Envois
		.Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
		.Include(envoi => envoi.EtatsEnvoiHistory)
		.Where(envoi => envoiIdList.Contains(envoi.EnvoiId))
		.ToList();
envoisToDelete
	.ForEach(envoi => envoi.LastEtatEnvoiHistoryEntry = null);
context.SaveChanges();
envoisToDelete
	.ForEach(envoi => 
		context.EtatEnvoiHistoryEntries.RemoveRange(envoi.EtatsEnvoiHistory));
context.Envois.RemoveRange(envoisToDelete);
context.SaveChanges();