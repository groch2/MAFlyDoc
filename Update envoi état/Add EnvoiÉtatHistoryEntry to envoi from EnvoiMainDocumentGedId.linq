<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore.Diagnostics</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>System.Globalization</Namespace>
</Query>

const string envoiMainDocumentGedId = "20250212163301757520084885";
const EtatEnvoiEnum newEnvoiEtat = EtatEnvoiEnum.EN_COURS_D_ENVOI;
// JJ/MM/AAAA HH:MM
const string envoiEtatDateTime = "10/09/2022 10:00";

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
        .Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
		.First(envoi => envoi.MainDocumentGedId == envoiMainDocumentGedId);
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
