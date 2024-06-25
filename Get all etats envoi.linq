<Query Kind="Expression">
  <Reference>C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.WebApi\bin\Debug\net6.0\MAFlyDoc.WebApi.dll</Reference>
  <Namespace>MAFlyDoc.WebApi.Model</Namespace>
</Query>

Enum
	.GetValues(typeof(EtatEnvoiEnum))
	.Cast<EtatEnvoiEnum>()
	.Select(item => new { id = (int)item, item })