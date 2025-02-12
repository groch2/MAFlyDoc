<Query Kind="Statements">
  <Namespace>System.Globalization</Namespace>
</Query>

var frCulture = CultureInfo.GetCultureInfo("fr-FR");
// 15 avr. 2022, 11:42
File
	.ReadAllLines(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\transport id d'envois en recommandé.txt")
	.Select(line => {
		var data = line.Split(';');
		var transportId = data[0];
		var initializationDateTime =
			DateTimeOffset.Parse(
				input: data[1],
				formatProvider: frCulture);
		return new {
			transportId,
			initializationDateTime,
		};
	})
	.Dump();
