<Query Kind="Statements">
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <IncludeLinqToSql>true</IncludeLinqToSql>
</Query>

var commandText = File.ReadAllText(@"C:\Users\deschaseauxr\Documents\MAFlyDoc\get comptes MAF.sql");
using var connection = new SqlConnection("Server=INTPARBDD31;Database=MAF_NSI;Integrated Security=True");
connection.Open();
using var command = connection.CreateCommand();
command.CommandText = commandText;
command.Parameters.AddWithValue("@rowNumberStart", 20040);
command.Parameters.AddWithValue("@rowNumberEnd", 21000);
var dataTable = new DataTable();
var dataAdapter = new SqlDataAdapter(command);
dataAdapter.Fill(dataTable);
dataTable.Dump();
dataTable.Rows[0]["CompteID"].Dump();