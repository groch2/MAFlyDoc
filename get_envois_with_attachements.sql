select
    [EnvoiId],
    count([AttachementGedId]) as [nb_attachements]
from [dbo].[Attachement]
group by [EnvoiId]
having count([AttachementGedId]) > 1
