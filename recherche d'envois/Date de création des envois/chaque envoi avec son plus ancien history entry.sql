USE [MAFlyDoc]
GO

WITH [EnvoiWithCreation] ([EnvoiId], [EnvoiCreationHistoryEntryId]) AS (
    SELECT [EnvoiId]
    ,(SELECT TOP 1 [EtatEnvoiHistoryEntryId]
    FROM [dbo].[EtatEnvoiHistoryEntry]
    WHERE [EtatEnvoiHistoryEntry].[EnvoiId] = [Envoi].[EnvoiId]
    ORDER BY [DateTime] ASC) AS [EnvoiCreationHistoryEntryId]
    FROM [dbo].[Envoi])
SELECT [EnvoiWithCreation].[EnvoiId]
,[EtatEnvoiHistoryEntry].[DateTime] AS [DateCreationEnvoi]
,[EtatEnvoiHistoryEntry].[EtatEnvoiId] AS [EtatDeCreationEnvoi]
FROM [dbo].[EtatEnvoiHistoryEntry]
JOIN [EnvoiWithCreation]
ON [EtatEnvoiHistoryEntry].[EtatEnvoiHistoryEntryId] = [EnvoiWithCreation].[EnvoiCreationHistoryEntryId]
