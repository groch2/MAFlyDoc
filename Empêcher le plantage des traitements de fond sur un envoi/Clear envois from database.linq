<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(1))
	    .Options;
var envoiCourrierDbContext = new EnvoiCourrierDbContext(dbContextOptions);
envoiCourrierDbContext.Envois
    .Include(envoi => envoi.AttachementsList)
    .Include(envoi => envoi.EtatsEnvoiHistory)
    .Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
	.Include(envoi => envoi.DocumentsArTelecharges)
	.ToList()
	.ForEach(DeleteEnvoiFromDatabase);
envoiCourrierDbContext.SaveChanges();
"terminé".Dump();

void DeleteEnvoiFromDatabase(MAFlyDoc.WebApi.Database.Model.Envoi envoiToDelete) {
	if (envoiToDelete.LastEtatEnvoiHistoryEntry != null) {
	    envoiCourrierDbContext
	        .EtatEnvoiHistoryEntries
	        .Remove(envoiToDelete.LastEtatEnvoiHistoryEntry);
		envoiCourrierDbContext.SaveChanges();
	}
	if (envoiToDelete.EtatsEnvoiHistory != null) {
	    var envoiEtatHistory =
	        envoiToDelete
	            .EtatsEnvoiHistory
	            .Except(new[] { envoiToDelete.LastEtatEnvoiHistoryEntry });
	    envoiCourrierDbContext
	        .EtatEnvoiHistoryEntries
	        .RemoveRange(
	            envoiEtatHistory!);
		envoiCourrierDbContext.SaveChanges();
	}
	if (envoiToDelete.DocumentsArTelecharges != null) {
		envoiToDelete.DocumentsArTelecharges = null;
	}
	envoiCourrierDbContext.Envois.Remove(envoiToDelete);
	envoiCourrierDbContext.SaveChanges();
}