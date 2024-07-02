USE [MAFlyDoc]
GO

-- DÉBUT Variables à modifier en fonction du test à réaliser
DECLARE @EnvoiId AS INT = 1
/*
État des envois :
EN_COURS_D_ENVOI       00
EN_COURS_DE_TRAITEMENT 01
ENVOYE                 02
NON_DISTRIBUE          03
REMIS_AU_DESTINATAIRE  04
TRAITEMENT_ECHOUE      05
TRAITEMENT_ANNULE      06
TRAITEMENT_REJETE      07
ENVOI_ABANDONNE        08
ABANDONNE_PAR_LA_MAF	 09
AR_RECU_PAR_LA_MAF     10
PND_RECU_PAR_LA_MAF	   11
*/
DECLARE @EtatEnvoi AS INT = 0
DECLARE @DateTimeEtatEnvoi AS DATETIMEOFFSET(7) = SYSDATETIMEOFFSET()
-- FIN Variables à modifier en fonction du test à réaliser

BEGIN TRANSACTION Reset_Etat_Envoi_History WITH MARK N'Reset envoi etat history';
UPDATE [dbo].[Envoi]
SET [LastEtatEnvoiHistoryEntryId] = NULL,
	[EtatFinalErrorMessage] = NULL
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
