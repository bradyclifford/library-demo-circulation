param(
    [string]$hostName
)

# make sure we're working with an up-to-date Path environment variable
. $PSScriptRoot\refreshPath.ps1

$dmConfig = Import-LocalizedData -BaseDirectory $PSScriptRoot -FileName ".dmconfig.psd1"

$dbName = $dmConfig.db.name
$serverInstance = $dmConfig.db.hostName

if($hostName)
{
  $serverInstance = $hostName
}
# assume a non-incremental DB build so that it will actually create the database.
$incremental = $false

# but, if the database exists, we'll do an incremental build, otherwise we won't.
if (SqlServer\Get-SqlDatabase -serverinstance $serverInstance | where-object name -eq $dbName) { $incremental = $true } 


$BuildParams = @{
  ProjectRoot = $PSScriptRoot
  Incremental = $incremental
}

if($hostName)
{
  $BuildParams.add("HostName", $hostName)
}

Invoke-DatabaseBuild @BuildParams
