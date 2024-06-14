BEGIN TRANSACTION

DELETE [dbo].[Attachement]
GO

UPDATE [dbo].[Envoi]
SET [LastEtatEnvoiHistoryEntryId] = NULL
GO

DELETE [dbo].[EtatEnvoiHistoryEntry]
GO

DELETE [dbo].[Envoi]
GO

DELETE [dbo].[DocumentsArTelecharges]
GO

DELETE [dbo].[Recipient]
GO

DELETE [dbo].[Sender]
GO

COMMIT TRANSACTION