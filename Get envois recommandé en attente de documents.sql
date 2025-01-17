SELECT [e].[EnvoiId], [e].[AccuseDeReceptionNumeriseParEsker], [e].[Application], [e].[DocumentsArTelechargesId], [e].[EtatFinalErrorMessage], [e].[Impression], [e].[LastEtatEnvoiHistoryEntryId], [e].[MailPostageId], [e].[MainDocumentGedId], [e].[NbRetriesLeft], [e].[PageAdresse], [e].[RecipientId], [e].[SenderId], [e].[Subject], [e].[SubmissionId], [e].[TransportId], [d].[DocumentsArTelechargesId], [d].[AccuseReceptionGedId_ESKER], [d].[AccuseReceptionGedId_MAF], [d].[PliNonDistribueGedId], [d].[PreuveDeDepotGedId]
FROM [dbo].[Envoi] AS [e]
LEFT JOIN [dbo].[EtatEnvoiHistoryEntry] AS [e0] ON [e].[LastEtatEnvoiHistoryEntryId] = [e0].[EtatEnvoiHistoryEntryId]
LEFT JOIN [dbo].[DocumentsArTelecharges] AS [d] ON [e].[DocumentsArTelechargesId] = [d].[DocumentsArTelechargesId]
WHERE (([e].[MailPostageId] = 2) AND [e0].[EtatEnvoiId] IN (1, 4)) AND (([d].[PreuveDeDepotGedId] IS NULL) OR ([d].[AccuseReceptionGedId_ESKER] IS NULL))
ORDER BY [e].[EnvoiId]