use MAFlyDoc
go

select [Envoi].[EnvoiId],
format([EnvoiInitialisation].[DateTime], 'dd/MM/yyyy HH:mm') as [EnvoiDateInitialisation],
  case [LastEtatEnvoi].[EtatEnvoiId]
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
format([LastEtatEnvoi].[DateTime], 'dd/MM/yyyy HH:mm') as [LastEtatEnvoiDate],
[DocumentsArTelecharges].[PreuveDeDepotGedId],
[DocumentsArTelecharges].[AccuseReceptionGedId_ESKER]

from [dbo].[Envoi]
join [dbo].[DocumentsArTelecharges]
on [Envoi].[DocumentsArTelechargesId] = [DocumentsArTelecharges].[DocumentsArTelechargesId]

join [dbo].[EtatEnvoiHistoryEntry] as [EnvoiInitialisation]
on [Envoi].[EnvoiId] = [EnvoiInitialisation].[EnvoiId]
and [EnvoiInitialisation].[EtatEnvoiId] = 0

join [dbo].[EtatEnvoiHistoryEntry] as [LastEtatEnvoi]
on [Envoi].[LastEtatEnvoiHistoryEntryId] = [LastEtatEnvoi].[EtatEnvoiHistoryEntryId]

where [Envoi].[MailPostageId] = 2
and (
  [DocumentsArTelecharges].[PreuveDeDepotGedId] is not NULL OR
  [DocumentsArTelecharges].[AccuseReceptionGedId_ESKER] is not NULL)

order by [EnvoiInitialisation].[DateTime] desc