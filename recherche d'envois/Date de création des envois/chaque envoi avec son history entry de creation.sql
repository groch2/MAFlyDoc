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
,[EtatEnvoiHistoryEntry].[EtatEnvoiId]
,CASE [EtatEnvoiHistoryEntry].[EtatEnvoiId]
    WHEN 0 THEN 'EN_COURS_D_ENVOI'
    WHEN 1 THEN 'EN_COURS_DE_TRAITEMENT'
    WHEN 2 THEN 'ENVOYE'
    WHEN 3 THEN 'NON_DISTRIBUE'
    WHEN 4 THEN 'REMIS_AU_DESTINATAIRE'
    WHEN 5 THEN 'TRAITEMENT_ECHOUE'
    WHEN 6 THEN 'TRAITEMENT_ANNULE'
    WHEN 7 THEN 'TRAITEMENT_REJETE'
    WHEN 8 THEN 'ENVOI_ABANDONNE'
    WHEN 9 THEN 'ABANDONNE_PAR_LA_MAF'
END
FROM [EnvoiWithHistoryEntryIdCreation]
JOIN [dbo].[EtatEnvoiHistoryEntry]
ON [EnvoiWithHistoryEntryIdCreation].[HistoryEntryIdCreation] = [EtatEnvoiHistoryEntry].[EtatEnvoiHistoryEntryId]