USE [MAFlyDoc]
GO

DELETE [Attachement]
FROM [dbo].[Attachement]
JOIN [dbo].[Envoi]
ON [Attachement].[EnvoiId] = [Envoi].[EnvoiId]
WHERE [Envoi].[TransportId] is null

DELETE FROM [dbo].[Envoi]
WHERE [TransportId] is null
GO

