declare @envoi_id_origine as int = 23490
declare @envoi_id_target as int = 24508

begin transaction;

UPDATE [dbo].[Envoi]
SET [LastEtatEnvoiHistoryEntryId] = NULL
,[EtatFinalErrorMessage] = NULL
WHERE [EnvoiId] = @envoi_id_origine

delete [dbo].[EtatEnvoiHistoryEntry]
where [EtatEnvoiHistoryEntry].[EnvoiId] = @envoi_id_origine

insert into [dbo].[EtatEnvoiHistoryEntry] ([EnvoiId], [EtatEnvoiId], [DateTime])
select @envoi_id_origine, [EtatEnvoiId], [DateTime]
from [dbo].[EtatEnvoiHistoryEntry]
where [EtatEnvoiHistoryEntry].[EnvoiId] = @envoi_id_target

UPDATE [dbo].[Envoi]
SET [LastEtatEnvoiHistoryEntryId] = IDENT_CURRENT('[dbo].[EtatEnvoiHistoryEntry]')
WHERE [EnvoiId] = @envoi_id_origine

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
where [EtatEnvoiHistoryEntry].[EnvoiId] = @envoi_id_origine
order by [EtatEnvoiHistoryEntry].[DateTime] desc

rollback transaction;