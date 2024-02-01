USE [Sinistre]
GO

UPDATE [Sinapps].[OperationComptableCourrier]
--ENVOI_PRIORITAIRE
SET [EnvoiId] = 2045
WHERE [OperationComptableCourrierId] = 423533

UPDATE [Sinapps].[OperationComptableCourrier]
--ENVOI_SIMPLE
SET [EnvoiId] = 2046
WHERE [OperationComptableCourrierId] = 423534

UPDATE [Sinapps].[OperationComptableCourrier]
--ENVOI_AR
SET [EnvoiId] = 2044
WHERE [OperationComptableCourrierId] = 423532
GO


