DECLARE @__courrierPosteMinDateTimeOffset_0 datetimeoffset = DATEADD(month, -1, GETDATE());

SELECT [e].[EnvoiId], 
CASE [e0].EtatEnvoiId
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
  WHEN 10 THEN 'AR_RECU_PAR_LA_MAF'
  WHEN 11 THEN 'PND_RECU_PAR_LA_MAF'
END as 'ETAT_ENVOI',
[e0].[DateTime]
FROM [dbo].[Envoi] AS [e]
LEFT JOIN [dbo].[EtatEnvoiHistoryEntry] AS [e0] ON [e].[LastEtatEnvoiHistoryEntryId] = [e0].[EtatEnvoiHistoryEntryId]
WHERE (([e].[TransportId] IS NOT NULL) AND ([e0].[EtatEnvoiHistoryEntryId] IS NOT NULL)) AND (([e0].[EtatEnvoiId] = 1) OR (([e0].[EtatEnvoiId] = 2) AND (([e].[MailPostageId] = 2) OR ([e0].[DateTime] >= @__courrierPosteMinDateTimeOffset_0))))
