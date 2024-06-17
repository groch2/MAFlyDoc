<Query Kind="Expression">
  <Reference>C:\TeamProjects\tarmac\Tarmac.Presentation.Web.API\bin\Debug\net6.0\Tarmac.Core.Models.dll</Reference>
</Query>

typeof(Tarmac.Core.Models.Dto.GetDemandesDto)
	.GetProperties()
	.Select(p => {
		var nullableUnderlyingType = Nullable.GetUnderlyingType(p.PropertyType);
		var isNullable = nullableUnderlyingType != null;
		return new {
			p.Name,
			IsNullable = isNullable,
			PropertyType = (isNullable ? nullableUnderlyingType : p.PropertyType).FullName
		};
	})
	.OrderBy(item => item.Name)