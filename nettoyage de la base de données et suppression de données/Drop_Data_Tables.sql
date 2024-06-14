USE [MAFlyDoc]
GO

ALTER TABLE [dbo].[EtatEnvoiHistoryEntry] DROP CONSTRAINT [FK_EtatEnvoiHistoryEntry_Envoi]
DROP TABLE [dbo].[Attachement]
DROP TABLE [dbo].[Envoi]
DROP TABLE [dbo].[EtatEnvoiHistoryEntry]
DROP TABLE [dbo].[Sender]
DROP TABLE [dbo].[DocumentsARTelecharges]
DROP TABLE [dbo].[Recipient]
