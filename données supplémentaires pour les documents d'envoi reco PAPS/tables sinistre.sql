use Sinistre
go
select T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME
from INFORMATION_SCHEMA.TABLES T
where T.TABLE_NAME like '%sinistre%'
and T.TABLE_SCHEMA not in ('Ref', 'Reprise')