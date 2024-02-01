<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer("Server=(local);Database=MAFlyDoc;Trusted_Connection=True;", providerOptions => providerOptions.CommandTimeout(60))
	    .Options;
var context = new EnvoiCourrierDbContext(dbContextOptions);
context.Envois
	.Include(envoi => envoi.AttachementsList)
	.Include(envoi => envoi.Recipient)
	.Include(envoi => envoi.Sender)
	.Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
	.Include(envoi => envoi.EtatsEnvoiHistory.OrderByDescending(etatEnvoiHistoryEntry => etatEnvoiHistoryEntry.DateTime))
	.Include(envoi => envoi.DocumentsArTelecharges)
	//.First(envoi => envoi.EnvoiId == 1)
	.First()
	.Dump();
context.Dispose();