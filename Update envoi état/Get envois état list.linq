<Query Kind="Statements">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Database</Namespace>
  <Namespace>MAFlyDoc.WebApi.Database.Model</Namespace>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Globalization</Namespace>
</Query>

new {
	EtatEnvoi =
		Enum.GetNames(typeof(EtatEnvoiEnum))
			.Select(etatEnvoi => new { name = etatEnvoi, value = (int)Enum.Parse<EtatEnvoiEnum>(etatEnvoi) })
}.Dump();
