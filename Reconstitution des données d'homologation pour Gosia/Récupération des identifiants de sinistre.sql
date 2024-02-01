USE [Sinistre]
GO

SELECT [Sinistre].[NumeroSinistre]
      ,[Sinistre].[SinistreId]
	  ,[OperationComptableCourrier].[DocGedId]
	  ,[OperationComptableCourrier].[EnvoiId]
	  ,[OperationComptableCourrier].[OperationComptableCourrierId]
FROM [Sinapps].[Sinistre]
JOIN [Sinapps].[OperationComptableCourrier]
ON [Sinistre].[SinistreId] = [OperationComptableCourrier].[SinistreId]
where NumeroSinistre in ('MA-23-443-884653-D', 'MA-23-220-3315436-X', 'MA-23-220-3315437-J')