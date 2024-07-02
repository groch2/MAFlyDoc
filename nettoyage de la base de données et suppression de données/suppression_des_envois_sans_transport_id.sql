USE [MAFlyDoc]
GO

DELETE [Attachement]
FROM [dbo].[Attachement]
  JOIN [dbo].[Envoi]
  ON [Attachement].[EnvoiId] = [Envoi].[EnvoiId]
WHERE [Envoi].[TransportId] is null
GO

UPDATE [dbo].[Envoi]
SET [LastEtatEnvoiHistoryEntryId] = NULL
WHERE [TransportId] is null
GO

DELETE [EtatEnvoiHistoryEntry]
FROM [dbo].[EtatEnvoiHistoryEntry]
  JOIN [dbo].[Envoi]
  ON [EtatEnvoiHistoryEntry].[EnvoiId] = [Envoi].[EnvoiId]
WHERE [Envoi].[TransportId] is null
GO

DELETE FROM [dbo].[Envoi]
WHERE [TransportId] is null
GO
