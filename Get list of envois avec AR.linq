<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\Microsoft.EntityFrameworkCore.SqlServer.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.UseSqlServer("Server = (local); Database = MAFlyDoc; Trusted_Connection = True;", providerOptions => providerOptions.CommandTimeout(60))
	    .Options;
using var _dbContext = new EnvoiCourrierDbContext(dbContextOptions);
var envoisAvecAr =
	_dbContext.Envois
		.Include(envoi => envoi.DocumentsArTelecharges)
		.Where(envoi => envoi.DocumentsArTelecharges != null);
envoisAvecAr.Dump();
