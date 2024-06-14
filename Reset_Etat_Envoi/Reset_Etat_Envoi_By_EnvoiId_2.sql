UPDATE [dbo].[Envoi]
SET [LastEtatEnvoiHistoryEntryId] = NULL
	,[EtatFinalErrorMessage] = NULL
WHERE [EnvoiId] = @EnvoiId

DELETE FROM [dbo].[EtatEnvoiHistoryEntry]
WHERE [EnvoiId] = @EnvoiId

INSERT INTO [dbo].[EtatEnvoiHistoryEntry]
([EnvoiId]
,[EtatEnvoiId]
,[DateTime])
VALUES
(@EnvoiId
,@EtatEnvoiId
,@DateTimeEtatEnvoi)

UPDATE [dbo].[Envoi]
SET [LastEtatEnvoiHistoryEntryId] = IDENT_CURRENT('[dbo].[EtatEnvoiHistoryEntry]')
WHERE [EnvoiId] = @EnvoiId
