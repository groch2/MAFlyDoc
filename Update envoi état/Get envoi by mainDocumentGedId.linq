<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Globalization</Namespace>
</Query>

// bdd-maflydoc.int.mal.local
var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server = (local); Database = MAFlyDoc; Trusted_Connection = True;",
			providerOptions => providerOptions.CommandTimeout(3))
	    .Options;
var envoiCourrierDbContext = new EnvoiCourrierDbContext(dbContextOptions);
var envoi =
	envoiCourrierDbContext
		.Envois
	    .Include(envoi => envoi.EtatsEnvoiHistory)
	    .Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
		.First(envoi => envoi.MainDocumentGedId == "20230828175135721281802470");
//envoi.Dump();
//Environment.Exit(0);
var cultureInfoFormatter = CultureInfo.GetCultureInfo("fr-FR");
new {
	envoi.EnvoiId,
	envoi.MainDocumentGedId,
	LastEtatEnvoiHistoryEntry = new {
		envoi
			.LastEtatEnvoiHistoryEntry
			.EtatEnvoiHistoryEntryId,
		envoi
			.LastEtatEnvoiHistoryEntry
			.EtatEnvoi,
		DateTime =
			formatDateTime(
				envoi
					.LastEtatEnvoiHistoryEntry
					.DateTime)
	},
	EtatsEnvoiHistory =
		envoi
			.EtatsEnvoiHistory
			.OrderByDescending(
				etatEnvoiHistory => etatEnvoiHistory.DateTime)
			.Select(
				etatEnvoiHistory => new {
					etatEnvoiHistory.EtatEnvoi,
					DateTime = formatDateTime(etatEnvoiHistory.DateTime)
				})	
}.Dump();
//Environment.Exit(0);
new {
	EtatEnvoi =
		Enum
			.GetNames(typeof(EtatEnvoiEnum))
			.OrderBy(
				item => item,
				StringComparer.InvariantCultureIgnoreCase)
			.ToArray()
}.Dump();

string formatDateTime(DateTimeOffset dateTime) => dateTime.ToString("G", cultureInfoFormatter);