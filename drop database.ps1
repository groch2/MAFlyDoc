$SQLServer = "(local)"
$Database = "MAFlyDoc"
$Command = Get-Content -Path "C:\Users\deschaseauxr\Documents\MAFlyDoc\Drop database.sql"  -Raw
Invoke-Sqlcmd -ServerInstance $SQLServer -Database $Database -Query $Command
