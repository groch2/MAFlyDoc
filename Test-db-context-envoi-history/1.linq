<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer("Server=localhost;Database=MAFlyDoc;Trusted_Connection=True;", providerOptions => providerOptions.CommandTimeout(60))
	    .Options;
var context = new EnvoiCourrierDbContext(dbContextOptions);
var envoi = new Envoi();
envoi.MainDocumentGedId = "toto";
envoi.Recipient = new Recipient { RecipientId = 1 };
envoi.Sender = new Sender { SenderId = 1 };
envoi.EtatsEnvoiHistory = new List<EtatEnvoiHistoryEntry> { new EtatEnvoiHistoryEntry { EtatEnvoiId = 3, DateTime = DateTime.Now } };
context.Attach(envoi);
context.SaveChanges().Dump();
new { envoi.EnvoiId }.Dump();
context.Dispose();