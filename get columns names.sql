select COLUMN_NAME
from INFORMATION_SCHEMA.COLUMNS
where TABLE_CATALOG = 'MAFlyDoc'
and TABLE_SCHEMA = 'dbo'
and TABLE_NAME = 'DocumentsArTelecharges'
order by COLUMN_NAME