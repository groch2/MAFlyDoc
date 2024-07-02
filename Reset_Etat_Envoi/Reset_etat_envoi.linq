<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(3))
	    .Options;
usingvar context = new EnvoiCourrierDbContext(dbContextOptions);
var envoi =
	context.Envois
		.Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
		.Include(envoi => envoi.EtatsEnvoiHistory)
		.First(envoi => envoi.EnvoiId == 1);
envoi.
context.Dispose();