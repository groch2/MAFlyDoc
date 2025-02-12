use MAFlyDoc
go

with [Envoi] ([EnvoiId], [MailPostageId], [DateInitialisation], [LastEtatEnvoi], [LastEtatEnvoiDate], [TransportId], [DocumentsArTelechargesId], [EtatFinalErrorMessage]) as (
  select [Envoi].[EnvoiId],
  [Envoi].[MailPostageId],
  [EnvoiInitialisation].[DateTime],
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
    end as [LastEtatEnvoi],
  [LastEtatEnvoi].[DateTime],
  [TransportId],
  [DocumentsArTelechargesId],
  [EtatFinalErrorMessage]

  from [dbo].[Envoi]

  join [dbo].[EtatEnvoiHistoryEntry] as [EnvoiInitialisation]
  on [Envoi].[EnvoiId] = [EnvoiInitialisation].[EnvoiId]
  and [EnvoiInitialisation].[EtatEnvoiId] = 0

  join [dbo].[EtatEnvoiHistoryEntry] as [LastEtatEnvoi]
  on [Envoi].[LastEtatEnvoiHistoryEntryId] = [LastEtatEnvoi].[EtatEnvoiHistoryEntryId]
)
select TOP 10
  [EnvoiId]
  ,[MailPostageId]
  ,format([DateInitialisation], 'dd/MM/yyyy HH:mm') as [DateInitialisation]
  ,[LastEtatEnvoi]
  ,format([LastEtatEnvoiDate], 'dd/MM/yyyy HH:mm') as [LastEtatEnvoiDate]
  ,[TransportId]
  ,[DocumentsArTelecharges].[PliNonDistribueGedId]
  ,[Envoi].[EtatFinalErrorMessage]
from [Envoi]
join [dbo].[DocumentsArTelecharges]
on [Envoi].[DocumentsArTelechargesId] = [DocumentsArTelecharges].[DocumentsArTelechargesId]
where [MailPostageId] = 2
and [LastEtatEnvoi] = 'NON_DISTRIBUE'
order by [Envoi].[DateInitialisation] desc