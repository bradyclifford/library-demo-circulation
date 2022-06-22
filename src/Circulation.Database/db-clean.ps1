param(
    [string]$hostName
)

# make sure we're working with an up-to-date Path environment variable
. $PSScriptRoot\refreshPath.ps1

$dmConfig = Import-LocalizedData -BaseDirectory $PSScriptRoot -FileName ".dmconfig.psd1"

$dbName = $dmConfig.db.name

if (!$hostName) {
    $hostName = $dmConfig.db.hostName
}

database-tools\Remove-Db -instanceName $hostName -dbName $dbName
