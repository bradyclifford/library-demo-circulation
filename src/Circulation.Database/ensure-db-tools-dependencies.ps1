# We want to be explicit about which modules get loaded so that we don't load old versions we're trying to upgrade
$PSModuleAutoLoadingPreference = 'None'
$ErrorActionPreference = 'Stop'


$psGallerySource = "https://www.powershellgallery.org/api/v2"
$artifactorySource = "https://artifacts.mktp.io/artifactory/api/nuget/powershell"
$flywayUrl = "https://artifacts.mktp.io/artifactory/api/nuget/Chocolatey"
$dbToolsMinVersion = "1.1.87"
$dbToolsMaxVersion = "1.9999999"
$sqlServerMinVersion = "21.0.17099"
$sqlServerMaxVersion = "21.1.999999"


if( $PSVersionTable.PSVersion -lt "5.0" ) {
    Write-Error "Requires PowerShell 5.0 or greater"
    exit 1
}

Import-Module Microsoft.PowerShell.Utility

# make sure we're working with an up-to-date Path environment variable
. $PSScriptRoot\refreshPath.ps1

Import-Module Microsoft.PowerShell.Management

# set TLS version to 1.2 because Artifactory requires it, even just registering the PSRepository will hit the https url.
Write-Output "Setting TLS version to 1.2"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Write-Output '+---------------------'
Write-Output "| PSModulePath is $env:PSModulePath"
Write-Output "| If PSModulePath doesn't start with the userprofile directory there are going to be problems later. E.g C:\windows\system32\config\systemprofile\Documents\WindowsPowerShell\Modules"
Write-Output '+---------------------'

# bootstrap to make sure we have the minimum versions of PowerShellGet and PackageManagement we need.
if(-not (Get-Module -Name PowerShellGet -ListAvailable | Where-Object {$_.Version -ge [Version]1.6})) {
    Import-Module PackageManagement
    # This will install the latest version of PowerShellGet, which will in-turn install a compatible version of PackageManagement and the NuGet package provider
    # By installing to the CurrentUser scope, the version we install there takes precedence over any machine-scope installation because the user's profile directory comes first in PSModulePath
    PackageManagement\Install-Package -Name PowerShellGet -Force -Source PSGallery -Scope CurrentUser -AllowClobber
    # Now remove (un-import, unload) the old version to make way for importing the version that just got installed.
    Remove-Module PackageManagement
}
Import-Module PackageManagement -Force
Import-Module PowerShellGet -Force

# for troubleshooting, display what versions of what modules we now have loaded
Write-Output '+---------------------'
Write-Output "| Loaded modules follow: "
Write-Output '+---------------------'
get-module | select-object Name, Version, Path | format-table -autosize

function Install-Dependency($module, $minVersion, $maxVersion, $repoName, $repoSource)
{
    if(Get-Module -Name $module -ListAvailable | Where-Object {$_.Version -ge [Version]$minVersion -and $_.Version -le [Version]$maxVersion }) {
        Write-Output "Importing $module module"
    } else {
        $repo = Get-PSRepository -Name $repoName -ErrorAction SilentlyContinue
        if (!$repo){
            Write-Output "Registering $repoName as as PSRepository"
            Register-PSRepository -Name $repoName -SourceLocation $repoSource -InstallationPolicy Trusted
        }
        Write-Output "Installing and importing $module module"
        Install-Module -Name $module -Repository $repoName -MinimumVersion $minVersion -MaximumVersion $maxVersion -Force -AllowClobber -Scope CurrentUser
    }
    # If there's a failure to load the thing we just installed, we have serious problems.
    Import-Module $module -MinimumVersion $minVersion -MaximumVersion $maxVersion
}

Write-Output '+---------------------'
Write-Output "| Ensuring Flyway gets installed"
Write-Output '+---------------------'
try {
    Get-Command flyway | Out-Null

#    Write-Status 'Updating flyway'
#    choco upgrade eh.flyway.commandline -y --source $flywayUrl
} catch {
    if( ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
      Write-Output 'Installing flyway'
      choco install eh.flyway.commandline -y --source $flywayUrl
    }
    else {
      write-error "In order to install flyway the first time, this needs to be run from an elevated administrator process"
      exit 1
    }
}

Write-Output '+---------------------'
Write-Output "| Installing and/or Importing required PowerShell modules..."
Write-Output '+---------------------'
Install-Dependency "SqlServer" $sqlServerMinVersion $sqlServerMaxVersion "PSGallery" $psGallerySource
Install-Dependency "database-tools" $dbToolsMinVersion $dbToolsMaxVersion "Artifactory" $artifactorySource

# for troubleshooting, display what versions of what modules we now have loaded
Write-Output '+---------------------'
Write-Output "| Loaded modules follow: "
Write-Output '+---------------------'
get-module | select-object Name, Version, Path | format-table -autosize

# ... and what modules are available to be installed
Write-Output '----------------------'
Write-Output "Available modules follow: "
Write-Output '----------------------'
get-module -ListAvailable | select-object Name, Version, Path | format-table -autosize

