USE [MAFlyDoc]
GO

DECLARE @EnvoiId AS INT = 2025

UPDATE [dbo].[Envoi]
SET [LastEtatEnvoiHistoryEntryId] = null
WHERE [EnvoiId] = @EnvoiId

DELETE FROM [dbo].[EtatEnvoiHistoryEntry]
WHERE EnvoiId = @EnvoiId

DELETE FROM [dbo].[Envoi]
WHERE EnvoiId = @EnvoiId
GO
