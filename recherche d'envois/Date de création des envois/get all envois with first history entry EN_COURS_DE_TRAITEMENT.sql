USE [MAFlyDoc]
GO

WITH [EnvoiWithTwoFirstHistoryEntries] ([EnvoiId], [HistoryEntryId_EN_COURS_D_ENVOI], [HistoryEntryId_EN_COURS_DE_TRAITEMENT]) AS (
    SELECT [Envoi].[EnvoiId]
    ,(SELECT [EtatEnvoiHistoryEntry].[EtatEnvoiHistoryEntryId]
    FROM [dbo].[EtatEnvoiHistoryEntry]
    WHERE [EtatEnvoiHistoryEntry].[EnvoiId] = [Envoi].[EnvoiId]
    AND [EtatEnvoiHistoryEntry].[EtatEnvoiId] = 0)
    ,(SELECT [EtatEnvoiHistoryEntry].[EtatEnvoiHistoryEntryId]
    FROM [dbo].[EtatEnvoiHistoryEntry]
    WHERE [EtatEnvoiHistoryEntry].[EnvoiId] = [Envoi].[EnvoiId]
    AND [EtatEnvoiHistoryEntry].[EtatEnvoiId] = 1)
    FROM [dbo].[Envoi])
,[EnvoiWithHistoryEntryCreation_EN_COURS_DE_TRAITEMENT] ([EnvoiId], [HistoryEntryId], [EnvoiCreationDate]) AS (
    SELECT [EnvoiWithTwoFirstHistoryEntries].[EnvoiId], [EtatEnvoiHistoryEntry].[EtatEnvoiHistoryEntryId], [EtatEnvoiHistoryEntry].[DateTime]
    FROM [EnvoiWithTwoFirstHistoryEntries]
    JOIN [dbo].[EtatEnvoiHistoryEntry]
    ON [HistoryEntryId_EN_COURS_DE_TRAITEMENT] = [EtatEnvoiHistoryEntry].[EtatEnvoiHistoryEntryId]
    WHERE [EnvoiWithTwoFirstHistoryEntries].[HistoryEntryId_EN_COURS_D_ENVOI] IS NULL)
SELECT *
FROM [EnvoiWithHistoryEntryCreation_EN_COURS_DE_TRAITEMENT]
ORDER BY [EnvoiId]
