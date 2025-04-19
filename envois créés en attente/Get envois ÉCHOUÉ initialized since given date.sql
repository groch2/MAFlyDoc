declare @date_minimal_initialisation_envoi as datetimeoffset = parse('2025-03-25T16:00Z' as datetimeoffset);
with [CTE_envois] as(
  SELECT [Envoi].[EnvoiId],
  case [MailPostageId]
    when 0 then 'PRIORITAIRE'
    when 1 then 'SIMPLE'
    when 2 then 'RECOMMANDÉ'
  end as [Affranchissement],
  CASE [etatEnvoiInitial].[EtatEnvoiId]
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
  END as [etat initial],
  [etatEnvoiInitial].[DateTime] as [date initialisation],
  CASE [LastEtatEnvoi].[EtatEnvoiId]
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
  END as [dernier etat],
  [LastEtatEnvoi].[DateTime] as [date dernier etat],
  trim(iif([EtatFinalErrorMessage] is null, '', [EtatFinalErrorMessage])) as [EtatFinalErrorMessage]
  FROM [dbo].[Envoi]
  LEFT JOIN [dbo].[EtatEnvoiHistoryEntry] AS [LastEtatEnvoi]
    ON [Envoi].[LastEtatEnvoiHistoryEntryId] = [LastEtatEnvoi].[EtatEnvoiHistoryEntryId]
  join [dbo].[EtatEnvoiHistoryEntry] as [etatEnvoiInitial]
    on [Envoi].[EnvoiId] = [etatEnvoiInitial].[EnvoiId]
    and [etatEnvoiInitial].[EtatEnvoiId] = 0
  WHERE [Envoi].[TransportId] IS NOT NULL
  AND [LastEtatEnvoi].[EtatEnvoiHistoryEntryId] IS NOT NULL
  AND [etatEnvoiInitial].[DateTime] >= @date_minimal_initialisation_envoi)
select
  [CTE_envois].[EnvoiId],
  [Envoi].[MainDocumentGedId],
  [Attachement].[AttachementGedId],
  [CTE_envois].[Affranchissement],
  [CTE_envois].[etat initial],
  FORMAT([CTE_envois].[date initialisation], 'ddd dd/MM/yyyy HH:mm:ss zz\h', 'fr-FR') as [date initialisation],
  [CTE_envois].[dernier etat],
  FORMAT([CTE_envois].[date dernier etat], 'ddd dd/MM/yyyy HH:mm:ss zz\h', 'fr-FR') as [date dernier etat],
  [CTE_envois].[EtatFinalErrorMessage]
from [CTE_envois]
join [dbo].[Envoi]
  on [CTE_envois].[EnvoiId] = [Envoi].[EnvoiId]
left join [dbo].[Attachement]
  on [Envoi].[EnvoiId] = [Attachement].[EnvoiId]
where trim([CTE_envois].[dernier etat]) = 'TRAITEMENT_ECHOUE'
  and trim([CTE_envois].[EtatFinalErrorMessage]) = 'The validity date of this message has expired.'
order by [CTE_envois].[date initialisation] desc
