USE [MAFlyDoc]
GO

DECLARE @MainDocumentGedId AS VARCHAR(50) = '20230828175135721281802470'
/*
EN_COURS_D_ENVOI		0
EN_COURS_DE_TRAITEMENT	1
ENVOYE					2
NON_DISTRIBUE			3
REMIS_AU_DESTINATAIRE	4
TRAITEMENT_ECHOUE		5
TRAITEMENT_ANNULE		6
TRAITEMENT_REJETE		7
*/
DECLARE @EtatEnvoi AS INT = 0
DECLARE @DateTimeEtatEnvoi AS DATETIMEOFFSET(7) = (SELECT CAST('2021-06-23T17:30:00+02:00' AS DATETIMEOFFSET))

BEGIN TRANSACTION Reset_Etat_Envoi_History WITH MARK N'Reset envoi etat history';  
DECLARE @EnvoiId AS INT = (
	SELECT [EnvoiId]
	FROM [dbo].[Envoi]
	WHERE [MainDocumentGedId] = @MainDocumentGedId)

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
,@EtatEnvoi
,@DateTimeEtatEnvoi)

UPDATE [dbo].[Envoi]
SET [LastEtatEnvoiHistoryEntryId] = IDENT_CURRENT('[dbo].[EtatEnvoiHistoryEntry]')
WHERE [EnvoiId] = @EnvoiId

COMMIT TRANSACTION Reset_Etat_Envoi_History;
GO
