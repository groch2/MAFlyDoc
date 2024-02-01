DECLARE @RefTypeDoc TABLE
	(
		[TypeDocumentId]      [INT]           NOT NULL,
		[Code]                [NVARCHAR](500) NOT NULL,
		[Libelle]             [NVARCHAR](500) NOT NULL,
		[CoteDocumentId]      [INT]           NOT NULL,
		[IsActif]             [BIT]           NOT NULL,
		[CodeTypeDoc]         [VARCHAR](5)    NULL,
		[VisibilitePapsExtra] [CHAR](1)       NULL,
		[IsTimeline]          [BIT]           NULL
	);
INSERT INTO @RefTypeDoc
VALUES
(18,N'AR POSTE',N'A.R. Poste',17,1,N'GE056',NULL,NULL),
(19,N'AR POSTE',N'AR poste',51,1,N'PAAPO',NULL,NULL),
(430,N'AR POSTE',N'Ar Poste',73,1,N'PAAPO',NULL,NULL),
(556,N'AR POSTE',N'A.R. Poste',91,1,N'RECAR',NULL,NULL);

MERGE [Ref].[TypeDocument] td
USING
	(
		SELECT
			[TypeDocumentId],
			[Code],
			[Libelle],
			[CoteDocumentId],
			[IsActif],
			[CodeTypeDoc],
			[VisibilitePapsExtra],
			[IsTimeline]
		FROM
			@RefTypeDoc
	) new
ON td.TypeDocumentId = new.TypeDocumentId
WHEN MATCHED
	THEN UPDATE SET
			 [Code] = new.[Code],
			 [Libelle] = new.[Libelle],
			 [CoteDocumentId] = new.[CoteDocumentId],
			 [IsActif] = new.[IsActif],
			 [CodeTypeDoc] = new.[CodeTypeDoc],
			 [VisibilitePapsExtra] = new.[VisibilitePapsExtra],
			 [IsTimeline] = new.[IsTimeline]
WHEN NOT MATCHED
	THEN INSERT
		(
			[TypeDocumentId],
			[Code],
			[Libelle],
			[CoteDocumentId],
			[IsActif],
			[CodeTypeDoc],
			[VisibilitePapsExtra],
			[IsTimeline]
		)
	VALUES
		(
			new.[TypeDocumentId], new.[Code], new.[Libelle], new.[CoteDocumentId], new.[IsActif],
			new.[CodeTypeDoc], new.[VisibilitePapsExtra], new.[IsTimeline]
		);
