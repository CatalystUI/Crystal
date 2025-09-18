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

# CatalystUI Configuration Module
# Name: CatalystUI.NuGet.psm1
# Author: FireController#1847
# Date: 2025-08-24
# Version: 1.1.1

$config = Import-PowerShellDataFile "$PSScriptRoot/../Config.psd1"

function Get-NuGetRepository {
    <#
    .SYNOPSIS
        Gets the path to the local NuGet repository.
    
    .DESCRIPTION
        Returns the full path to the local NuGet repository as specified in the configuration file.
        
    .OUTPUTS
        [string]
        The full path to the local NuGet repository.
    
    .EXAMPLE
        PS C:\> Get-NuGetRepository
        C:\Users\Username\.catalystui
    #>
    [CmdletBinding()]
    param()

    return Join-Path $HOME $config.NuGetDot
}

function Get-CatalystNuGetRepository {
    <#
    .SYNOPSIS
        Gets the path to the CatalystUI local NuGet repository.
    
    .DESCRIPTION
        Returns the full path to the CatalystUI local NuGet repository as specified in the configuration file.
        
    .OUTPUTS
        [string]
        The full path to the CatalystUI local NuGet repository.
    
    .EXAMPLE
        PS C:\> Get-CatalystNuGetRepository
        C:\Users\Username\.catalystui
    #>
    [CmdletBinding()]
    param()

    return Join-Path $HOME $config.CatalystNuGetDot
}

function Get-NuGetCache {
    <#
    .SYNOPSIS
        Gets the path to the NuGet package cache.
    
    .DESCRIPTION
        Returns the full path to the NuGet package cache directory.
        
    .OUTPUTS
        [string]
        The full path to the NuGet package cache directory.
        
    .EXAMPLE
        PS C:\> Get-NuGetCache
        C:\Users\Username\.nuget\packages
    #>
    [CmdletBinding()]
    param()

    return Join-Path $HOME ".nuget/packages"
}

function Initialize-NuGetRepository {
    <#
    .SYNOPSIS
        Ensures the local NuGet repository is set up and configured.
        
    .DESCRIPTION
        Ensures the local NuGet repository directory exists, registers it as a package source,
        and creates a NuGet.Config file in the specified solution directory if it doesn't already exist.
        
        Returns $true if the operation completes successfully, otherwise $false.
        
    .PARAMETER solution
        The path to the solution directory where the NuGet.Config file should be created.
        
    .INPUTS
        [string] solution
        The path to the solution directory.
        
    .OUTPUTS
        [bool]
        $true if the operation completes successfully, otherwise $false.
        
    .EXAMPLE
        PS C:\> Initialize-NuGetRepository -solution "C:\Path\To\Solution"
        Creating local NuGet repository at: `e[32mC:\Users\Username\.catalystui`e[0m
        Registering local NuGet source: `e[32mCatalystUI Local Repository`e[0m
        Creating NuGet.Config at `e[32mC:\Path\To\Solution\NuGet.Config`e[0m
        True
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string] $solution
    )

    # Disable verbosity (NuGet seems to output a lot of info)
    $previousPreference = $VerbosePreference
    $VerbosePreference = 'SilentlyContinue'

    try {
        # Ensure NuGet repository directory exists
        $nugetPath = Get-NuGetRepository
        $nugetSource = $config.NuGetSource
        if (-not (Test-Path $nugetPath)) {
            Write-Host "Creating local NuGet repository at: `e[32m$nugetPath`e[0m"
            New-Item -ItemType Directory -Path $nugetPath | Out-Null
            if ($IsWindows) {
                (Get-Item $nugetPath).Attributes += 'Hidden'
            }
        } else {
            Write-Host "Using existing local NuGet repository at: `e[32m$nugetPath`e[0m"
        }

        # Ensure NuGet source is registered
        $existingSource = Get-PackageSource -Name $nugetSource -ErrorAction SilentlyContinue
        if (-not $existingSource) {
            Write-Host "Registering local NuGet source: `e[32m$nugetSource`e[0m"
            Register-PackageSource -Name $nugetSource -Location $nugetPath -ProviderName NuGet -Trusted | Out-Null
        } else {
            Write-Host "Local NuGet source already registered: `e[32m$nugetSource`e[0m"
        }

        # Create the NuGet.Config file if it doesn't exist
        $catalystNuGetPath = Get-CatalystNuGetRepository
        $catalystNuGetSource = $config.CatalystNuGetSource
        $nugetConfigPath = Join-Path $solution "NuGet.Config"
        $nugetConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="$($catalystNuGetSource)" value="$($catalystNuGetPath)" />
    <add key="$($nugetSource)" value="$($nuGetPath)" />
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
    } catch {
        return $false
    } finally {
        # Restore verbosity
        $VerbosePreference = $previousPreference
    }

    # Return success
    return $true
}

function Clear-NuGetRepository {
    <#
    .SYNOPSIS
        Clears all contents of the local NuGet repository, except for hidden files.
        
    .DESCRIPTION
        Deletes all contents of the local NuGet repository directory, except for hidden files.
        
        Returns $true if the operation completes successfully, otherwise $false.
        
    .OUTPUTS
        [bool]
        $true if the operation completes successfully, otherwise $false.
    
    .EXAMPLE
        PS C:\> Clear-NuGetRepository
        Clearing contents of local NuGet repository at: `e[32mC:\Users\Username\.catalystui`e[0m
        True
    #>
    [CmdletBinding()]
    param()

    $nugetPath = Get-NuGetRepository
    if (Test-Path $nugetPath) {
        Write-Host "Clearing contents of local NuGet repository at: `e[32m$nugetPath`e[0m"
        Get-ChildItem -Path $nugetPath -Recurse -Force |
                Where-Object {
                    # Always exclude dotfiles/folders
                    if ($IsWindows) {
                        -not $_.Name.StartsWith('.') -and -not ($_.Attributes -band [IO.FileAttributes]::Hidden)
                    } else {
                        -not $_.Name.StartsWith('.')
                    }
                } |
                Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
    } else {
        Write-Host "Local NuGet repository does not exist at: `e[32m$nugetPath`e[0m"
        return $false
    }
    return $true
}

function Remove-NuGetRepository {
    <#
    .SYNOPSIS
        Clears the local NuGet repository directory, and then removes the NuGet source and config file.
        
    .DESCRIPTION
        Deletes the entire local NuGet repository directory, removes the registered NuGet source,
        and deletes the NuGet.Config file in the solution directory if it exists.
        
        Returns $true if the operation completes successfully, otherwise $false.
        
    .OUTPUTS
        [bool]
        $true if the operation completes successfully, otherwise $false.
    
    .EXAMPLE
        PS C:\> Remove-NuGetRepository
        Removing local NuGet repository at: `e[32mC:\Users\Username\.catalystui`e[0m
        True
    #>
    [CmdletBinding()]
    param()

    $nugetPath = Get-NuGetRepository
    if (Test-Path $nugetPath) {
        Write-Host "Removing local NuGet repository at: `e[32m$nugetPath`e[0m"

        # Clear the repository first
        if (-not (Clear-NuGetRepository)) {
            Write-Warning "Failed to clear NuGet repository contents. Aborting removal."
            return $false
        }

        # Remove the package source
        $nugetSource = $config.NuGetSource
        $existingSource = Get-PackageSource -Name $nugetSource -ErrorAction SilentlyContinue
        if ($existingSource) {
            Write-Host "Unregistering local NuGet source: `e[32m$nugetSource`e[0m"
            Unregister-PackageSource -Name $nugetSource -ErrorAction SilentlyContinue
        }

        # Delete the configuration file
        $solution = Get-LocationSolution
        $nugetConfigPath = Join-Path $solution "NuGet.Config"
        if (Test-Path -Path $nugetConfigPath) {
            Write-Host "Removing NuGet.Config at `e[32m$nugetConfigPath`e[0m"
            Remove-Item -Path $nugetConfigPath -Force -ErrorAction SilentlyContinue
        }
    } else {
        Write-Host "Local NuGet repository does not exist at: `e[32m$nugetPath`e[0m"
        return $false
    }
    return $true
}

Export-ModuleMember -Function Get-CatalystNuGetRepository, Get-NuGetRepository, Get-NuGetCache, Initialize-NuGetRepository, Clear-NuGetRepository, Remove-NuGetRepository