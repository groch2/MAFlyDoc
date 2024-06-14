$databaseServer = "(local)"
# bdd-maflydoc.int.maf.local
$database = "MAFlyDoc"

$command = Get-Content -Path "C:\Users\deschaseauxr\Documents\MAFlyDoc\Drop database.sql" -Raw
Invoke-Sqlcmd -ServerInstance $databaseServer -Database $database -Query $command
Write-Host "the database has been dropped"

$dacpac = "C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.Database\bin\Debug\MAFlyDoc.Database.dacpac"
$connectionString = "Data Source=$databaseServer;Initial Catalog=$database;Integrated Security=true;TrustServerCertificate=True"
&"SqlPackage.exe" /Action:Publish /SourceFile:$dacpac /TargetConnectionString:$connectionString /p:BlockOnPossibleDataLoss='False' /p:DropObjectsNotInSource='True'