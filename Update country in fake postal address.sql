USE [MAF_NSI]
GO

DECLARE @AdresseId AS INT = 574092

DECLARE @FormatAFNOR AS NVARCHAR(240) = (SELECT [FormatAfnor] FROM [Personnes].[Adresse] WHERE [AdresseID] = @AdresseId)
--FAUSSE ADRESSE 0  FAUSSE ADRESSE 10000 FAUSSE ADRESSE Afghanistan 
SET @FormatAFNOR = REPLACE(@FormatAFNOR , 'Afghanistan' , 'France')

UPDATE [Personnes].[Adresse]
SET [FormatAfnor] = @FormatAFNOR,
[PaysID] = 60
WHERE [AdresseID] = @AdresseId
GO
