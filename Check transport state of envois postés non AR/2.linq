<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.EnableSensitiveDataLogging()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(3))
	    .Options;
using var context = new EnvoiCourrierDbContext(dbContextOptions);

var minDateTimeOffset =
	new DateTimeOffset(DateTimeOffset.Now.AddMonths(-1).AddDays(-0).Date);
new { minDateTimeOffset }.Dump();
context
	.Envois
	.Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
	.Where(envoi =>
	    envoi.LastEtatEnvoiHistoryEntry.EtatEnvoi == EtatEnvoiEnum.INITIALISE ||
	    envoi.LastEtatEnvoiHistoryEntry.EtatEnvoi == EtatEnvoiEnum.POSTE &&
	        (envoi.MailPostageId == MailPostage.RECOMMANDE ||
	            envoi.LastEtatEnvoiHistoryEntry.DateTime >= minDateTimeOffset))
	.Select(envoi =>
		new {
			envoi.EnvoiId,
			LastEtatEnvoiHistoryEntry = new {
				envoi.LastEtatEnvoiHistoryEntry.EtatEnvoi, 
				envoi.LastEtatEnvoiHistoryEntry.DateTime 
			}
		})
	.Dump();
