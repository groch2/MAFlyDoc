$dacpac = "C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.Database\bin\Debug\MAFlyDoc.Database.dacpac"
# bdd-maflydoc.int.maf.local
$databaseServer = "localhost"
$databaseName = "MAFlyDoc"
# $databaseServer = "bdd-maflydoc.int.maf.local"
$connectionString = "Data Source=$databaseServer;Initial Catalog=$databaseName;Integrated Security=true;TrustServerCertificate=True"
Write-Host $connectionString
SqlPackage /Action:Publish /SourceFile:$dacpac /TargetConnectionString:$connectionString /p:BlockOnPossibleDataLoss='False' /p:DropObjectsNotInSource='True'
