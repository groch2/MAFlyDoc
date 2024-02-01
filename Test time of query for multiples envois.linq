<Query Kind="Statements">
  <Namespace>System.Net.Http</Namespace>
</Query>

var http_client = new HttpClient();
var watch = System.Diagnostics.Stopwatch.StartNew();
var http_response = await http_client.GetAsync("http://localhost:5000/Envois?recipient-compte-id=70200020");
http_response.IsSuccessStatusCode.Dump();
watch.Stop();
watch.Elapsed.Dump();