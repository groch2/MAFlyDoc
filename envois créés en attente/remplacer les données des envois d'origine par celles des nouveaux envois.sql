USE MAFlyDoc;

SET NOCOUNT ON

DROP TABLE IF EXISTS #envoi_id_origin_target

CREATE TABLE #envoi_id_origin_target (
    Envoi_id_origin INT NOT NULL,
    Envoi_id_target INT NOT NULL,
);
ALTER TABLE #envoi_id_origin_target
ADD CONSTRAINT composite_PK 
PRIMARY KEY (Envoi_id_origin, Envoi_id_target)

insert into #envoi_id_origin_target values
(39750, 41133),
(39759, 41134)

BEGIN TRANSACTION;

DECLARE @Envoi_id_origin int;
DECLARE @Envoi_id_target int;

SELECT TOP(1)
  @Envoi_id_origin = Envoi_id_origin,
  @Envoi_id_target = Envoi_id_target
FROM #envoi_id_origin_target

WHILE @@ROWCOUNT <> 0
BEGIN
  -- remplacer les données d'historique de envoi_origin par celles de envoi_target
  -- DÉBUT
  UPDATE [dbo].[Envoi]
  SET [LastEtatEnvoiHistoryEntryId] = NULL
  ,[EtatFinalErrorMessage] = NULL
  WHERE [EnvoiId] = @Envoi_id_origin

  delete [dbo].[EtatEnvoiHistoryEntry]
  where [EtatEnvoiHistoryEntry].[EnvoiId] = @Envoi_id_origin
  and [EtatEnvoiHistoryEntry].[EtatEnvoiId] not in (0, 1)

  insert into [dbo].[EtatEnvoiHistoryEntry] ([EnvoiId], [EtatEnvoiId], [DateTime])
  select @Envoi_id_origin, [EtatEnvoiId], [DateTime]
  from [dbo].[EtatEnvoiHistoryEntry]
  where [EtatEnvoiHistoryEntry].[EnvoiId] = @Envoi_id_target
  and [EtatEnvoiHistoryEntry].[EtatEnvoiId] not in (select [EtatEnvoiId] from [dbo].[EtatEnvoiHistoryEntry] where [EnvoiId] = @Envoi_id_origin)

  UPDATE [dbo].[Envoi]
  SET [LastEtatEnvoiHistoryEntryId] = IDENT_CURRENT('[dbo].[EtatEnvoiHistoryEntry]')
  WHERE [EnvoiId] = @Envoi_id_origin

  DELETE FROM #envoi_id_origin_target
  WHERE Envoi_id_origin = @Envoi_id_origin
  and Envoi_id_target = @Envoi_id_target
  -- FIN

  -- remplacer le transportId de envoi_origin par celui de envoi_target
  -- DÉBUT
  UPDATE [dbo].[Envoi]
  SET [TransportId] = (
    select [TransportId]
    from [dbo].[Envoi]
    where [EnvoiId] = @Envoi_id_target)
  WHERE [EnvoiId] = @Envoi_id_origin

  DELETE FROM #envoi_id_origin_target
  WHERE Envoi_id_origin = @Envoi_id_origin
  and Envoi_id_target = @Envoi_id_target
  -- FIN

  SELECT TOP(1)
    @Envoi_id_origin = Envoi_id_origin,
    @Envoi_id_target = Envoi_id_target
  FROM #envoi_id_origin_target
END

DROP TABLE #envoi_id_origin_target;

COMMIT TRANSACTION;