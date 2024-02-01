USE [MAFlyDoc]
GO

SELECT MAX([EnvoiId]) AS [Max Envoi Id] FROM [dbo].[Envoi]
GO

SELECT IDENT_CURRENT( '[dbo].[Envoi]' ) AS [Next Envoi Id]
GO

SELECT MAX([EtatEnvoiHistoryEntryId]) AS [Max Etat Envoi History Entry Id] FROM [dbo].[EtatEnvoiHistoryEntry]
GO

SELECT IDENT_CURRENT( '[dbo].[EtatEnvoiHistoryEntry]' ) AS [Next Etat Envoi History Entry Id]
GO
