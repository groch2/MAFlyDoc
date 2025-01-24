use MAFlyDoc
go
select
  [Envoi].[EnvoiId],
  case [Envoi].[MailPostageId]
    when 1 then 'ENVOI_PRIORITAIRE'
    when 2 then 'ENVOI_SIMPLE'
    when 3 then 'ENVOI_AR'
  end as [MailPostage],
  case [EtatEnvoiHistoryEntry].[EtatEnvoiId]
    when 0 then 'EN_COURS_D_ENVOI'
    when 1 then 'EN_COURS_DE_TRAITEMENT'
    when 2 then 'ENVOYE'
    when 3 then 'NON_DISTRIBUE'
    when 4 then 'REMIS_AU_DESTINATAIRE'
    when 5 then 'TRAITEMENT_ECHOUE'
    when 6 then 'TRAITEMENT_ANNULE'
    when 7 then 'TRAITEMENT_REJETE'
    when 8 then 'ENVOI_ABANDONNE'
    when 9 then 'ABANDONNE_PAR_LA_MAF'
    when 10 then 'AR_RECU_PAR_LA_MAF'
    when 11 then 'PND_RECU_PAR_LA_MAF'
  end as [EtatEnvoi],
  format([EtatEnvoiHistoryEntry].[DateTime], 'dd/MM/yyyy HH:mm') as [HistoryEntryDate]
from [dbo].[Envoi]
join [dbo].[EtatEnvoiHistoryEntry]
on [Envoi].[EnvoiId] = [EtatEnvoiHistoryEntry].[EnvoiId]
order by [Envoi].[EnvoiId], [EtatEnvoiHistoryEntry].[DateTime] desc