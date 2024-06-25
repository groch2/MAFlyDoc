USE [MAFlyDoc]
GO

BEGIN TRANSACTION

;WITH [EnvoiWithTwoFirstHistoryEntries] ([EnvoiId], [HistoryEntryId_EN_COURS_D_ENVOI], [HistoryEntryId_EN_COURS_DE_TRAITEMENT]) AS (
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
INSERT INTO [dbo].[EtatEnvoiHistoryEntry] ([EnvoiId], [EtatEnvoiId], [DateTime])
SELECT [EnvoiId], 0, DATEADD(second, -1, [EnvoiCreationDate])
FROM [EnvoiWithHistoryEntryCreation_EN_COURS_DE_TRAITEMENT]

COMMIT TRANSACTION
