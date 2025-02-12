use [MAFlyDoc]
go
update [DocumentsArTelecharges]
set [PliNonDistribueGedId] = null
from [dbo].[Envoi]
join [dbo].[DocumentsArTelecharges] as [DocumentsArTelecharges]
on [Envoi].[DocumentsArTelechargesId] = [DocumentsArTelecharges].[DocumentsArTelechargesId]
where [Envoi].[EnvoiId] = 127