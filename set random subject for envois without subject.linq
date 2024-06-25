<Query Kind="Program">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main() {
	var dbContextOptions =
		new DbContextOptionsBuilder<EnvoiCourrierDbContext>()
			.UseSqlServer(
				"Server=(local);Database=MAFlyDoc;Trusted_Connection=True;",
				providerOptions => providerOptions.CommandTimeout(3))
		    .Options;
	using var context = new EnvoiCourrierDbContext(dbContextOptions);
	var envoisWithoutSubject = context.Envois.Where(envoi => string.IsNullOrEmpty(envoi.Subject));
	foreach (var envoi in envoisWithoutSubject) {
		envoi.Subject = GetRandomWord(10);
	}
	await context.SaveChangesAsync();
}

static Random dice = new Random();
static string GetRandomWord(int length) =>
	new string(new int[length].Select(_ => (char)(dice.Next(26) + (int)'A')).ToArray());

