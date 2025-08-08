<#
##########################################################################
CatalystUI - Cross-Platform UI Library
Copyright (c) 2025 FireController#1847. All rights reserved.

This file is part of CatalystUI and is provided as part of an early-access release.
Unauthorized commercial use, distribution, or modification is strictly prohibited.

This software is not open source and is not publicly licensed.
For full terms, see the LICENSE and NOTICE files in the project root.
##########################################################################
#>

[CmdletBinding()]
param (
    [switch]$clear
)


# --- Disable Verbosity ---
$previousPreference = $VerbosePreference
$VerbosePreference = 'SilentlyContinue'


# --- Prepare Local Repository ---
$nugetPath = Join-Path $HOME ".catalystui/.crystal"
$nugetSource = "CatalystUI Crystal Local Repository"
if (-not (Test-Path $nugetPath)) {
    Write-Host "Creating local NuGet repository at: `e[32m$nugetPath`e[0m"
    New-Item -ItemType Directory -Path $nugetPath | Out-Null
    if ($IsWindows) {
        (Get-Item $nugetPath).Attributes += 'Hidden'
    }
} else {
    if ($clear) {
        Write-Host "Clearing contents of local NuGet repository at: `e[32m$nugetPath`e[0m"
        Get-ChildItem -Path $nugetPath -Recurse -Force |
                Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
    } else {
        Write-Host "Using existing local NuGet repository at: `e[32m$nugetPath`e[0m"
    }
}

# --- Register Local Repository ---
$existingSource = Get-PackageSource -Name $nugetSource -ErrorAction Silently
if (-not $existingSource) {
    Write-Host "Registering local NuGet source: `e[32m$nugetSource`e[0m"
    Register-PackageSource -Name $nugetSource -Location $nugetPath -ProviderName NuGet -Trusted | Out-Null
} else {
    Write-Host "Local NuGet source already registered: `e[32m$nugetSource`e[0m"
}


# --- Create NuGet Config ---
$nugetConfigPath = Join-Path (Resolve-Path "./Crystal/") "NuGet.Config"
$nugetConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="CatalystUI Local Repository" value="$(Resolve-Path $HOME/.catalystui)" />
    <add key="CatalystUI Crystal Local Repository" value="$(Resolve-Path $HOME/.catalystui/.crystal)" />
  </packageSources>
</configuration>
"@
if (-not (Test-Path -Path $nugetConfigPath)) {
    New-Item -ItemType File -Path $nugetConfigPath -Force | Out-Null
    Set-Content -Path $nugetConfigPath -Value $nugetConfigContent -Force
    Write-Host "Creating NuGet.Config at `e[32m$nugetConfigPath`e[0m"
} else {
    Write-Host "NuGet.Config already exists at `e[32m$nugetConfigPath`e[0m"
}


# --- Restore Verbosity ---
$VerbosePreference = $previousPreference