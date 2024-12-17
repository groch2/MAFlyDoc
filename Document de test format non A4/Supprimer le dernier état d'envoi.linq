<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

using DatabaseEtatEnvoiHistoryEntry = MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry;
const int envoiId = 3;
var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(3))
	    .Options;
using var context = new EnvoiCourrierDbContext(dbContextOptions);
var envoi =
	context.Envois
		.Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
		.Include(envoi => envoi.EtatsEnvoiHistory)
		.First(envoi => envoi.EnvoiId == envoiId);

var chronologicalEtatEnvoiHistory =
	GetChronologicallySortedEnvoiHistory(envoi.EtatsEnvoiHistory);
var beforeLastEtatEnvoiHistory =
	chronologicalEtatEnvoiHistory[^2];
var lastEtatEnvoiHistory =
	chronologicalEtatEnvoiHistory[^1];

envoi.EtatsEnvoiHistory.Remove(lastEtatEnvoiHistory);
envoi.LastEtatEnvoiHistoryEntry = beforeLastEtatEnvoiHistory;

chronologicalEtatEnvoiHistory =
	GetChronologicallySortedEnvoiHistory(envoi.EtatsEnvoiHistory);
chronologicalEtatEnvoiHistory.Dump();
envoi.LastEtatEnvoiHistoryEntry.Dump();

await context.SaveChangesAsync();

DatabaseEtatEnvoiHistoryEntry[] GetChronologicallySortedEnvoiHistory(
	ICollection<DatabaseEtatEnvoiHistoryEntry> envoiHistory) =>
		envoiHistory.OrderBy(envoi => envoi.DateTime).ToArray();
