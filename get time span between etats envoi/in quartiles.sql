with [envoi_history] as (
  select
    [EtatEnvoiHistoryEntry].[EnvoiId]
    ,CASE [EtatEnvoiHistoryEntry].[EtatEnvoiId]
      WHEN 00 THEN 'EN_COURS_D_ENVOI'
      WHEN 01 THEN 'EN_COURS_DE_TRAITEMENT'
      WHEN 02 THEN 'ENVOYE'
      WHEN 03 THEN 'NON_DISTRIBUE'
      WHEN 04 THEN 'REMIS_AU_DESTINATAIRE'
      WHEN 05 THEN 'TRAITEMENT_ECHOUE'
      WHEN 06 THEN 'TRAITEMENT_ANNULE'
      WHEN 07 THEN 'TRAITEMENT_REJETE'
      WHEN 08 THEN 'ENVOI_ABANDONNE'
      WHEN 09 THEN 'ABANDONNE_PAR_LA_MAF'
      WHEN 10 THEN 'AR_RECU_PAR_LA_MAF'
      WHEN 11 THEN 'PND_RECU_PAR_LA_MAF'
    END as [EtatEnvoi]
    ,[EtatEnvoiHistoryEntry].[DateTime]
  from [dbo].[EtatEnvoiHistoryEntry]),
[envoi_delai] as (
  select [etat_creation].[EnvoiId]
  ,DATEDIFF(hour, [etat_creation].[DateTime], [etat_envoi].[DateTime]) as [nb_heures_entre_creation_et_envoi]
  from [envoi_history] as [etat_creation]
  join [envoi_history] as [etat_envoi]
  on [etat_creation].[EnvoiId] = [etat_envoi].[EnvoiId]
  where [etat_creation].[EtatEnvoi] = 'EN_COURS_DE_TRAITEMENT'
  and [etat_envoi].[EtatEnvoi] = 'ENVOYE'
),
[delai_by_quartile] as (
  select NTILE(4) OVER (ORDER BY [nb_heures_entre_creation_et_envoi]) AS [quartile]
  ,[nb_heures_entre_creation_et_envoi]
  from [envoi_delai])
SELECT [quartile]
,count(*) as [nb envois in quartile]
,MAX([nb_heures_entre_creation_et_envoi]) [delai max quartile en heures]
FROM [delai_by_quartile]
GROUP BY [quartile]