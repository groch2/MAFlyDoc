<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net8.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer(
			"Server=bdd-maflydoc.int.maf.local;Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(60))
	    .Options;
var envoiCourrierDbContext = new EnvoiCourrierDbContext(dbContextOptions);
var transportsIdList =
	File
		.ReadAllLines(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\TransportsIdList.txt")
		.Select(long.Parse)
		.ToHashSet();
envoiCourrierDbContext.Envois
    //.Include(envoi => envoi.DocumentsArTelecharges)
	.Where(envoi => transportsIdList.Contains(envoi.TransportId.Value))
	.Select(envoi => new { envoi.EnvoiId, envoi.TransportId })
	.Dump();
