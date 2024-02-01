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
var envoi =
	_dbContext.Envois
		.Include(envoi => envoi.DocumentsArTelecharges)
		.First(envoi => envoi.EnvoiId == 5);
envoi.DocumentsArTelecharges = new DocumentsArTelecharges();
var test = await _dbContext.SaveChangesAsync();
test.Dump();
