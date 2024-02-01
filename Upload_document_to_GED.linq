<Query Kind="Statements">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Net.Http.Json</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

const string filePath = @"C:\Users\deschaseauxr\Documents\MAFlyDoc\test.pdf";
var fileName = Path.GetFileName(filePath);

await using var stream = File.OpenRead(filePath);
using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/v1/upload", UriKind.Relative));
using var content = new MultipartFormDataContent { { new StreamContent(stream), "file", fileName } };
request.Content = content;

var client = new HttpClient { BaseAddress = new Uri("https://api-ged-intra.int.maf.local/") };
var httpResponse = await client.SendAsync(request);
var responseContent = await httpResponse.Content.ReadAsStringAsync();
var uploadId = JsonNode.Parse(responseContent)["guidFile"].GetValue<string>();

var documentUpload = new {
  fileId = uploadId,
  libelle = fileName,
  deposePar = "ROD",
  dateDocument = DateTime.Now.ToUniversalTime(),
  fichierNom = fileName,
  fichierTaille = new FileInfo(filePath).Length,
  categoriesFamille = "DOCUMENTS CONTRAT",
  categoriesCote = "AUTRES",
  categoriesTypeDocument = "DIVERS",
  canalId = 1,
};
var documentUploadJson = JsonContent.Create(documentUpload);
var uploadResponse = 
	await client.PostAsync(new Uri("/v1/finalizeUpload", UriKind.Relative), documentUploadJson);
var uploadResponseContent = await uploadResponse.Content.ReadAsStringAsync();
var documentId = JsonNode.Parse(uploadResponseContent)["documentId"].GetValue<string>();
new { documentId }.Dump();