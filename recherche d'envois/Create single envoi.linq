<Query Kind="Program">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main() {
	var context = CreateEnvoiDbContext("Server=(local);Database=MAFlyDoc;Trusted_Connection=True;");
	var envoi = new Envoi {
	    TransportId = 0,
	    Sender = new Sender {
	        PersonFirstName = "Test",
	        PersonLastName = "Test",
	        CompanyName = "Test",
	        SenderId = 0,
	        UserId = "Test",
	    },
	    Recipient = new Recipient {
	        AdresseAfnor = "Test",
	        AdresseId = 0,
	        CompteId = 0,
	        PersonneId = 0,
	    },
	    MainDocumentGedId = "20240606130915430131474537",
	    MailPostageId = 0,
	    NbRetriesLeft = 0,
		Subject = GetRandomWord(10)
	};
	await CreateEnvoi(envoi, ParseDateTimeOffset("07/11/2017"), context);

	await UpdateEnvoiEtat(
		envoi: envoi,
		envoiEtat: EtatEnvoiEnum.EN_COURS_DE_TRAITEMENT,
		envoiUpdatedEtatDate: ParseDateTimeOffset("20/03/2019"),
		context: context);

	new {
		envoi.EnvoiId,
		envoi.Subject
	}.Dump();
}

static async Task CreateEnvoi(
	Envoi envoi,
	DateTimeOffset creationDate,
	EnvoiCourrierDbContext context) {
	var etatEnvoiHistoryEntry =
		new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
			EtatEnvoi = EtatEnvoiEnum.EN_COURS_D_ENVOI,
			DateTime = creationDate
		};
	envoi.EtatsEnvoiHistory =
		new List<MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry> {
			etatEnvoiHistoryEntry
		};
	var envoiEntity = context.Add(envoi).Entity;
	await context.SaveChangesAsync();
	envoi.LastEtatEnvoiHistoryEntry = etatEnvoiHistoryEntry;
	await context.SaveChangesAsync();
}

static async Task UpdateEnvoiEtat(
	Envoi envoi,
	EtatEnvoiEnum envoiEtat,
	DateTimeOffset envoiUpdatedEtatDate,
	EnvoiCourrierDbContext context) {
	var etatEnvoiHistoryEntry =
		new MAFlyDoc.WebApi.Database.Model.EtatEnvoiHistoryEntry {
			EtatEnvoi = envoiEtat,
			DateTime = envoiUpdatedEtatDate
		};
	envoi.LastEtatEnvoiHistoryEntry = etatEnvoiHistoryEntry;
	envoi.EtatsEnvoiHistory.Add(etatEnvoiHistoryEntry);
	context.Update(envoi);
	await context.SaveChangesAsync();
}

static DateTimeOffset ParseDateTimeOffset(string input) =>
	DateTimeOffset.ParseExact(
		input: input,
		format: "dd/MM/yyyy",
		formatProvider: CultureInfo.GetCultureInfo("fr-FR"),
		styles: DateTimeStyles.AssumeLocal);

static Random dice = new();
static string GetRandomWord(int length) =>
	new string(new int[length].Select(_ => (char)(dice.Next(26) + (int)'A')).ToArray());

static EnvoiCourrierDbContext CreateEnvoiDbContext(string connectionString) {
	var dbContextOptions =
		new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
			.EnableSensitiveDataLogging()
			.UseSqlServer(
				connectionString,
				providerOptions => providerOptions.CommandTimeout(3))
		    .Options;
	return new EnvoiCourrierDbContext(dbContextOptions);
}
