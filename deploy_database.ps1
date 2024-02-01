$dacpac = "C:\TeamProjects\MAFlyDoc\MAFlyDoc\MAFlyDoc.Database\bin\Debug\MAFlyDoc.Database.dacpac"
# bdd-maflydoc.int.maf.local
$databaseServer = "localhost"
# $databaseServer = "bdd-maflydoc.int.maf.local"
$connectionString = "Data Source=$databaseServer;Initial Catalog=MAFlyDoc;Integrated Security=true"
Write-Host $connectionString
SqlPackage /Action:Publish /SourceFile:$dacpac /TargetConnectionString:"Data Source=$($databaseServer);Initial Catalog=MAFlyDoc;Integrated Security=true" /p:BlockOnPossibleDataLoss='False' /p:DropObjectsNotInSource='True'