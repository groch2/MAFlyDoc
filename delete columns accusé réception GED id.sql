ALTER TABLE [dbo].[DocumentsArTelecharges]
ADD [AccuseReceptionGedId] VARCHAR(50) NULL
GO

UPDATE [dbo].[DocumentsArTelecharges]
SET [AccuseReceptionGedId] = [AccuseReceptionGedId_ESKER]
GO

alter table [dbo].[DocumentsArTelecharges]
drop column AccuseReceptionGedId_MAF
GO

alter table [dbo].[DocumentsArTelecharges]
drop column AccuseReceptionGedId_ESKER
GO
