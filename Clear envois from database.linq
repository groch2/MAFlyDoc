<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server=bdd-MAFlyDoc.int.maf.local;Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(10))
	    .Options;
var envoiCourrierDbContext = new EnvoiCourrierDbContext(dbContextOptions);
envoiCourrierDbContext.Envois
    .Include(envoi => envoi.AttachementsList)
    .Include(envoi => envoi.EtatsEnvoiHistory)
    .Include(envoi => envoi.LastEtatEnvoiHistoryEntry)
	.ToList()
	.ForEach(DeleteEnvoiFromDatabase);
envoiCourrierDbContext.SaveChanges();

void DeleteEnvoiFromDatabase(MAFlyDoc.WebApi.Database.Model.Envoi envoiToDelete) {
	if (envoiToDelete.LastEtatEnvoiHistoryEntry != null)
	{
	    envoiCourrierDbContext
	        .EtatEnvoiHistoryEntries
	        .Remove(envoiToDelete.LastEtatEnvoiHistoryEntry);
		envoiCourrierDbContext.SaveChanges();
	}
	if (envoiToDelete.EtatsEnvoiHistory != null)
	{
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
	envoiCourrierDbContext.Envois.Remove(envoiToDelete);
	envoiCourrierDbContext.SaveChanges();
}