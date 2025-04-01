USE [MAFlyDoc]

DECLARE @TargetEnvoiId AS INT = 1

UPDATE T2
SET [AccuseReceptionGedId_ESKER] = null
  ,[PreuveDeDepotGedId] = null
  ,[PliNonDistribueGedId] = null
FROM [dbo].[Envoi] as T1
JOIN [dbo].[DocumentsArTelecharges] AS T2
ON T1.DocumentsArTelechargesId = T2.DocumentsArTelechargesId
WHERE T1.[EnvoiId] = @TargetEnvoiId

UPDATE [dbo].[Envoi]
SET [AccuseDeReceptionNumeriseParEsker] = 1
WHERE [EnvoiId] = @TargetEnvoiId
