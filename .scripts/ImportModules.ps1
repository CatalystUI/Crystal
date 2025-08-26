[CmdletBinding()]
param()

Get-ChildItem "${PSScriptRoot}/Modules" -Recurse -Filter *.psm1 | ForEach-Object {
    $modulePath = $_.FullName
    Import-Module -Name $modulePath -Force
}