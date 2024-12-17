-- DÉBUT Variables à modifier en fonction du test à réaliser
DECLARE @EnvoiId AS INT = 1
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
ENVOI_ABANDONNE         8
ABANDONNE_PAR_LA_MAF    9
AR_RECU_PAR_LA_MAF     10
PND_RECU_PAR_LA_MAF	   11
*/
DECLARE @EtatEnvoi AS INT = 2
-- DECLARE @DateTimeEtatEnvoi AS DATETIMEOFFSET(7) = (SELECT CAST('2021-06-23T17:30:00+02:00' AS DATETIMEOFFSET))
-- FIN Variables à modifier en fonction du test à réaliser

BEGIN TRANSACTION Reset_Etat_Envoi_History WITH MARK N'Reset envoi etat history';
UPDATE [dbo].[Envoi]
SET [LastEtatEnvoiHistoryEntryId] = NULL,
	[EtatFinalErrorMessage] = NULL

DELETE FROM [dbo].[EtatEnvoiHistoryEntry]

INSERT INTO [dbo].[EtatEnvoiHistoryEntry]
SELECT 
[EnvoiId]
,2
,'2024-10-28'
FROM [dbo].[Envoi]

UPDATE [dbo].[Envoi]
SET [LastEtatEnvoiHistoryEntryId] = [EtatEnvoiHistoryEntry].EtatEnvoiHistoryEntryId
from [dbo].[EtatEnvoiHistoryEntry]
join [dbo].[Envoi]
on [EtatEnvoiHistoryEntry].EnvoiId = [Envoi].EnvoiId

COMMIT TRANSACTION Reset_Etat_Envoi_History;
GO
