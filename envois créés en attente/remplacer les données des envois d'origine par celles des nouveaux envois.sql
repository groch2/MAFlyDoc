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
(39440, 39820),
(39440, 39824),
(39441, 39818),
(39441, 39821),
(39444, 39819),
(39444, 39830),
(39446, 39822),
(39447, 39828),
(39449, 39829),
(39450, 39825),
(39451, 39826),
(39452, 39892),
(39453, 39890),
(39454, 39894),
(39455, 39827),
(39456, 39895),
(39457, 39893),
(39458, 39891),
(39464, 39858),
(39465, 39921),
(39466, 39836),
(39468, 39885),
(39470, 39849),
(39471, 39907),
(39472, 39855),
(39473, 39851),
(39474, 39841),
(39475, 39835),
(39477, 39870),
(39480, 39847),
(39481, 39834),
(39482, 39842),
(39483, 39823),
(39484, 39943),
(39485, 39883),
(39486, 39844),
(39488, 39837),
(39489, 39860),
(39490, 39976),
(39491, 39871),
(39492, 39850),
(39493, 39838),
(39494, 39971),
(39496, 39910),
(39502, 39869),
(39505, 39904),
(39506, 39879),
(39507, 39983),
(39510, 39845),
(39512, 39953),
(39513, 39957),
(39514, 39901),
(39517, 39902),
(39518, 39944),
(39519, 39877),
(39520, 39889),
(39521, 39833),
(39522, 39912),
(39523, 39878),
(39524, 39899),
(39525, 39951),
(39526, 39915),
(39527, 39867),
(39528, 39848),
(39529, 39917),
(39530, 40005),
(39531, 40003),
(39532, 39906),
(39533, 39884),
(39534, 39856),
(39535, 39832),
(39536, 39981),
(39537, 39840),
(39539, 39955),
(39540, 39873),
(39541, 39931),
(39542, 39876),
(39543, 39984),
(39544, 39998),
(39545, 39939),
(39548, 39946),
(39550, 39857),
(39551, 39909),
(39552, 39882),
(39555, 39934),
(39556, 39993),
(39558, 39988),
(39559, 39919),
(39560, 39947),
(39561, 39991),
(39562, 39948),
(39563, 39978),
(39564, 39887),
(39565, 39979),
(39566, 39987),
(39567, 39914),
(39568, 39861),
(39569, 39831),
(39570, 39897),
(39571, 39859),
(39572, 39922),
(39573, 39854),
(39575, 39843),
(39578, 39872),
(39579, 39940),
(39580, 39866),
(39581, 39853),
(39582, 39956),
(39583, 39997),
(39584, 39839),
(39585, 39985),
(39586, 39898),
(39587, 39932),
(39588, 39959),
(39589, 39930),
(39590, 39929),
(39591, 39927),
(39592, 39852),
(39593, 39964),
(39594, 39905),
(39595, 39926),
(39596, 39913),
(39597, 39937),
(39598, 39880),
(39599, 39874),
(39601, 39875),
(39602, 39949),
(39604, 39933),
(39605, 39935),
(39606, 39962),
(39607, 40004),
(39608, 39864),
(39611, 39958),
(39612, 39881),
(39613, 39936),
(39614, 39961),
(39615, 39868),
(39617, 39967),
(39619, 39865),
(39620, 39923),
(39621, 39863),
(39622, 39846),
(39623, 39945),
(39624, 39920),
(39625, 39888),
(39626, 39952),
(39629, 39975),
(39631, 39960),
(39632, 39941),
(39633, 39986),
(39634, 39862),
(39636, 39918),
(39637, 39942),
(39638, 39950),
(39639, 39954),
(39640, 39908),
(39641, 39980),
(39642, 39965),
(39643, 39970),
(39644, 40006),
(39646, 39972),
(39648, 39928),
(39649, 39903),
(39650, 39989),
(39651, 39886),
(39652, 40002),
(39653, 39974),
(39654, 39982),
(39655, 39999),
(39656, 39973),
(39657, 39900),
(39658, 39969),
(39659, 39911),
(39660, 39896),
(39661, 39977),
(39662, 39966),
(39663, 39994),
(39664, 39916),
(39665, 39938),
(39666, 40000),
(39667, 39968),
(39668, 40001),
(39669, 39924),
(39670, 39995),
(39671, 39925),
(39672, 39996),
(39673, 39992),
(39674, 39990),
(39675, 39963)

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

  insert into [dbo].[EtatEnvoiHistoryEntry] ([EnvoiId], [EtatEnvoiId], [DateTime])
  select @Envoi_id_origin, [EtatEnvoiId], [DateTime]
  from [dbo].[EtatEnvoiHistoryEntry]
  where [EtatEnvoiHistoryEntry].[EnvoiId] = @Envoi_id_target

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