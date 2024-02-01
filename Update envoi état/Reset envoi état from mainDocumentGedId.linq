<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Reference Relative="..\..\microsoft.extensions.logging.abstractions.8.0.0-rc.2.23479.6\lib\net7.0\Microsoft.Extensions.Logging.Abstractions.dll">&lt;MyDocuments&gt;\microsoft.extensions.logging.abstractions.8.0.0-rc.2.23479.6\lib\net7.0\Microsoft.Extensions.Logging.Abstractions.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore.Diagnostics</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>System.Globalization</Namespace>
</Query>

const string envoiMainDocumentGedId = "20230828175135721281802470";
const EtatEnvoiEnum newEnvoiEtat = EtatEnvoiEnum.ENVOYE;
// JJ/MM/AAAA HH:MM
const string envoiEtatDateTime = "23/06/2021 17:30";

var cultureInfoFormatter = CultureInfo.GetCultureInfo("fr-FR");
var envoiEtatDate =
	DateTime
		.Parse(
			envoiEtatDateTime,
			cultureInfoFormatter);

// bdd-maflydoc.int.mal.local
var useSqlServer =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(3));
useSqlServer
	.LogTo(
		logMessage => {
			Console.WriteLine(logMessage);
			Console.WriteLine();
		},
		new[] {
			DbLoggerCategory.Database.Command.Name
		},
		LogLevel.Information,
		DbContextLoggerOptions.None)
	.EnableSensitiveDataLogging();;
var envoiCourrierDbContext = new EnvoiCourrierDbContext(useSqlServer.Options);
var envoiToUpdate =
	envoiCourrierDbContext.Envois
        .Include(envoi => envoi.EtatsEnvoiHistory)
		.First(envoi => envoi.MainDocumentGedId == envoiMainDocumentGedId);
if (envoiToUpdate.EtatsEnvoiHistory.Any()) {
    envoiCourrierDbContext
        .EtatEnvoiHistoryEntries
        .RemoveRange(
            envoiToUpdate
            .EtatsEnvoiHistory);
	envoiCourrierDbContext.SaveChanges();
}
var etatEnvoiHistoryEntry =
    new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
        DateTime = envoiEtatDate,
        EtatEnvoi = newEnvoiEtat
    };
envoiToUpdate.EtatsEnvoiHistory.Add(etatEnvoiHistoryEntry);
envoiToUpdate.LastEtatEnvoiHistoryEntry = etatEnvoiHistoryEntry;
envoiCourrierDbContext.SaveChanges();
new {
	envoiToUpdate.EnvoiId,
	envoiToUpdate.MainDocumentGedId,
	LastEtatEnvoiHistoryEntry = new {
		envoiToUpdate
			.LastEtatEnvoiHistoryEntry
			.EtatEnvoi,
		DateTime =
			envoiToUpdate
				.LastEtatEnvoiHistoryEntry
				.DateTime
				.ToString("G", cultureInfoFormatter)
	}
}.Dump();
