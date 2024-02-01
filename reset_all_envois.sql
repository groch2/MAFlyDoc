USE [MAFlyDoc]
GO

SELECT [EnvoiId]
      ,[TransportId]
      ,[Subject]
      ,[DateTimeEnvoi]
      ,[Etat]
      ,[DateCourrierPoste]
      ,[DateCourrierRetourneNonDelivre]
FROM [dbo].[Envoi]

UPDATE [dbo].[Envoi]
SET  [Etat] = 1
	,[DateCourrierPoste] = null
	,[DateCourrierRetourneNonDelivre] = null
FROM [dbo].[Envoi]

GO
