USE [MAFlyDoc]
GO

WITH [EnvoiWithTwoFirstHistoryEntries] ([EnvoiId], [HistoryEntryId_1], [HistoryEntryId_2]) AS (
    SELECT [Envoi1].[EnvoiId]
    ,(SELECT [EtatEnvoiHistoryEntry].[EtatEnvoiHistoryEntryId]
    FROM [dbo].[EtatEnvoiHistoryEntry]
    WHERE [EtatEnvoiHistoryEntry].[EnvoiId] = [Envoi1].[EnvoiId]
    AND [EtatEnvoiHistoryEntry].[EtatEnvoiId] = 0) AS [EnvoiHistoryEntry_1]
    ,(SELECT [EtatEnvoiHistoryEntry].[EtatEnvoiHistoryEntryId]
    FROM [dbo].[EtatEnvoiHistoryEntry]
    WHERE [EtatEnvoiHistoryEntry].[EnvoiId] = [Envoi1].[EnvoiId]
    AND [EtatEnvoiHistoryEntry].[EtatEnvoiId] = 1) AS [EnvoiHistoryEntry_2]
    FROM [dbo].[Envoi] AS [Envoi1])
,[EnvoiWithHistoryEntryIdCreation] ([EnvoiId], [HistoryEntryIdCreation]) AS (
    SELECT [EnvoiId]
    ,COALESCE([HistoryEntryId_1], [HistoryEntryId_2])
    FROM [EnvoiWithTwoFirstHistoryEntries])
SELECT [EnvoiWithHistoryEntryIdCreation].[EnvoiId]
,[EtatEnvoiHistoryEntry].[DateTime] AS [DateTimeCreationEnvoi]
FROM [EnvoiWithHistoryEntryIdCreation]
JOIN [dbo].[EtatEnvoiHistoryEntry]
ON [EnvoiWithHistoryEntryIdCreation].[HistoryEntryIdCreation] = [EtatEnvoiHistoryEntry].[EtatEnvoiHistoryEntryId]