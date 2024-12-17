<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(3))
	    .Options;
using var context = new EnvoiCourrierDbContext(dbContextOptions);
var envois =
	context
		.Envois
		.Include(envoi => envoi.DocumentsArTelecharges)
		.Where(envoi => envoi.DocumentsArTelecharges != null && envoi.DocumentsArTelecharges.AccuseReceptionGedId == null);
foreach(var envoi in envois) {
	envoi.DocumentsArTelecharges.AccuseReceptionGedId = Guid.NewGuid().ToString("N").ToUpperInvariant();
}
await context.SaveChangesAsync();
"terminé".Dump();