select
  CASE [EtatEnvoiHistoryEntry].[EtatEnvoiId]
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
  END as [etat]
  ,FORMAT([EtatEnvoiHistoryEntry].[DateTime], 'ddd dd/MM/yyyy HH:mm:ss zz\h', 'fr-FR') as [date]
from [dbo].[EtatEnvoiHistoryEntry]
where [EtatEnvoiHistoryEntry].[EnvoiId] = 23490
order by [EtatEnvoiHistoryEntry].[DateTime] desc
