<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

var dbContextOptions =
	new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
		.EnableSensitiveDataLogging()
		.UseSqlServer(
			"Server=localhost;Database=MAFlyDoc;Trusted_Connection=True;",
			providerOptions => providerOptions.CommandTimeout(3))
	    .Options;
var startDate = new DateTimeOffset(new DateTime(year: 2024, month: 03, day: 02, hour: 0, minute: 0, second: 0, kind: DateTimeKind.Utc));
var endDate = new DateTimeOffset(new DateTime(year: 2024, month: 03, day: 02, hour: 0, minute: 0, second: 0, kind: DateTimeKind.Utc));
//new { startDate, endDate }.Dump();
//Environment.Exit(0);
var context = new EnvoiCourrierDbContext(dbContextOptions);
var envois =
	context
		.Envois
        .Select(
            envoi =>
                envoi
                    .EtatsEnvoiHistory
                    .FirstOrDefault(
                        envoiHistoryEntry =>
                            envoiHistoryEntry.EtatEnvoi == EtatEnvoiEnum.EN_COURS_D_ENVOI ||
							envoiHistoryEntry.EtatEnvoi == EtatEnvoiEnum.EN_COURS_DE_TRAITEMENT))
		.Where(item => item.DateTime.Date >= startDate.Date && item.DateTime.Date <= endDate.Date);
envois
	.ToList()
	.ForEach(item => item.Dump());
