use [MAFlyDoc];
SELECT [Envoi].[EnvoiId]
      ,[TransportId]
      ,[MailPostageId]
      ,[Subject]
      ,[EtatEnvoi].[EtatEnvoiId]
      ,[AccuseDeReceptionNumeriseParEsker]
      ,[DocumentsArTelecharges].*
FROM [dbo].[Envoi]
join [dbo].[EtatEnvoiHistoryEntry] AS [EtatEnvoi]
on [Envoi].[LastEtatEnvoiHistoryEntryId] = [EtatEnvoi].[EtatEnvoiHistoryEntryId]
join [dbo].[DocumentsArTelecharges]
on [Envoi].[DocumentsArTelechargesId] = [DocumentsArTelecharges].[DocumentsArTelechargesId]