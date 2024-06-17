<Query Kind="Statements">
  <Namespace>System.Text.Json</Namespace>
</Query>

var raw_text = File.ReadAllText(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\statistiques de duré de synchro.json");
JsonDocument
	.Parse(raw_text)
	.RootElement
	.GetProperty("rawResponse")
	.GetProperty("hits")
	.GetProperty("hits")
	.EnumerateArray()
	.Select(item => {
		var fields = item.GetProperty("fields");
		Func<string, string> getItemFromFields =
			(string fieldName) => GetFirstItemOfJsonElement(fields, fieldName);
		var timestamp = DateTime.Parse(getItemFromFields("@timestamp"));
		var message = getItemFromFields("message");
		var nombre_d_envois_synchronisés =
			Regex.Match(input: message, pattern: @"""nombre d'envois synchronisés"": (\b\d+\b)$")
				.Groups.Cast<Group>().ToArray()[1].Value;
		return new {
			timestamp = timestamp,
			durée = getItemFromFields("fields.taskDuration"),
			nombre_d_envois_synchronisés
		};
	})
	.OrderBy(item => item.timestamp)
	.Dump();

string GetFirstItemOfJsonElement(JsonElement jsonElement, string propertyName) {
	JsonElement property;
	try {
		property =
			jsonElement
				.GetProperty(propertyName);
	} catch (KeyNotFoundException) {
		return string.Empty;
	}
	return 
		property
			.EnumerateArray()
			.First()
			.GetString();
}