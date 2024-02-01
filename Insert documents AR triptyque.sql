USE [GEDMAF]
GO

DECLARE @NewFamilleDocumentId AS INT = (SELECT MAX([FamilleDocumentId]) + 1 FROM [Ref].[FamilleDocument])
DECLARE @NewCoteDocumentId AS INT = (SELECT MAX([CoteDocumentId]) + 1 FROM [Ref].[CoteDocument])
DECLARE @NewTypeDocumentId AS INT = (SELECT MAX([TypeDocumentId]) + 1 FROM [Ref].[TypeDocument])

BEGIN TRANSACTION

INSERT INTO [Ref].[FamilleDocument]
           ([FamilleDocumentId]
           ,[Code]
           ,[Libelle]
           ,[IsActif])
     VALUES
           (@NewFamilleDocumentId
           ,'DOCUMENTS PAPS'
           ,'Documents PAPS'
           ,1)

INSERT INTO [Ref].[CoteDocument]
           ([CoteDocumentId]
           ,[Code]
           ,[Libelle]
           ,[FamilleDocumentId]
           ,[CodeCouleur]
           ,[IsActif]
           ,[Ordre])
     VALUES
           (@NewCoteDocumentId
           ,'GESTION'
           ,'Gestion'
           ,@NewFamilleDocumentId
           ,NULL
           ,1
           ,NULL)

INSERT INTO [Ref].[TypeDocument]
           ([TypeDocumentId]
           ,[Code]
           ,[Libelle]
           ,[CoteDocumentId]
           ,[IsActif]
           ,[CodeTypeDoc]
           ,[VisibilitePapsExtra]
           ,[IsTimeline])
     VALUES
           (@NewTypeDocumentId
           ,'AR POSTE'
           ,'AR posté'
           ,@NewCoteDocumentId
           ,1
           ,NULL
           ,NULL
           ,NULL)

COMMIT TRANSACTION
GO

