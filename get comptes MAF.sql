WITH PERSONNE_P ([RowNumber], [CompteID], [TypeCompteID], [PersonneID], [Nom], [Prenom]) AS (
	SELECT ROW_NUMBER() OVER(ORDER BY P.[PersonneID] ASC)
		  ,C.[CompteID]
		  ,C.[TypeCompteID]
		  ,P.[PersonneID]
		  ,P.Nom
		  ,P.Prenom
	FROM [Personnes].[Compte] C
	JOIN [Personnes].[Personne] P
	ON C.PersonneID = P.PersonneID
	WHERE P.TypePersonneId = 1
	AND Nom is not null
	AND Nom <> ''
	AND Prenom is not null
	AND Prenom <> '')
SELECT [CompteID], [PersonneID], [Nom], [Prenom], [Espace].[Description] as [EspaceCompte]
FROM PERSONNE_P
JOIN [RefMaf].[TypeCompte]
ON PERSONNE_P.[TypeCompteID] = [TypeCompte].[TypeCompteId]
JOIN [RefMaf].[Espace]
ON [TypeCompte].EspaceId = [Espace].EspaceId
WHERE [RowNumber] BETWEEN @rowNumberStart AND @rowNumberEnd
