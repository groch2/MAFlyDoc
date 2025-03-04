USE [MAFlyDoc]
GO

-- DÉBUT Variables à modifier en fonction du test à réaliser
DECLARE @MainDocumentGedId AS VARCHAR(50) = '20250212163301757520084885'
/*
État des envois :
  EN_COURS_D_ENVOI        0
  EN_COURS_DE_TRAITEMENT  1
  ENVOYE                  2
  NON_DISTRIBUE           3
  REMIS_AU_DESTINATAIRE   4
  TRAITEMENT_ECHOUE       5
  TRAITEMENT_ANNULE       6
  TRAITEMENT_REJETE       7
*/
DECLARE @EtatEnvoi AS INT = 2
DECLARE @DateTimeEtatEnvoi AS DATETIMEOFFSET(7) = (SELECT CAST('2025-02-12T16:40:00+02:00' AS DATETIMEOFFSET))
-- DECLARE @DateTimeEtatEnvoi AS DATETIMEOFFSET(7) = ( SYSDATETIMEOFFSET())
-- FIN Variables à modifier en fonction du test à réaliser

BEGIN TRANSACTION Reset_Etat_Envoi_History WITH MARK N'Reset envoi etat history';
  DECLARE @EnvoiId AS INT = (
  	SELECT [EnvoiId]
  	FROM [dbo].[Envoi]
  	WHERE [MainDocumentGedId] = @MainDocumentGedId)
	
	--SELECT @EnvoiId AS ENVOI_ID
	IF @EnvoiId IS NULL
    PRINT N'aucun envoi n''a été trouvé avec cet identifiant de document GED'
	ELSE
    PRINT 'ENVOI_ID: ' + CAST(@EnvoiId AS NVARCHAR(30))
  
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
