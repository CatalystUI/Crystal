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

# CatalystUI Configuration Script
# Author: FireController#1847
# Date: 2025-08-06
# Version: 1.0.0

# --- PowerShell 7+ Required ---
if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Host "PowerShell 7+ is required. Attempting to relaunch..."

    # Try to find pwsh in PATH
    $pwsh = Get-Command pwsh -ErrorAction SilentlyContinue

    # If not found in PATH, try known locations
    if (-not $pwsh) {
        $knownPaths = @(
            "$env:ProgramFiles\PowerShell\7\pwsh.exe",
            "$env:ProgramFiles(x86)\PowerShell\7\pwsh.exe",
            "$env:LOCALAPPDATA\Microsoft\PowerShell\7\pwsh.exe"
        )
        $pwsh = $knownPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
    } else {
        $pwsh = $pwsh.Source
    }
    if ($pwsh) {
        Write-Host "Relaunching script in PowerShell 7..."
        Start-Process -FilePath $pwsh -ArgumentList "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "`"$($MyInvocation.MyCommand.Definition)`""
        $failed = $false
    } else {
        Write-Host "PowerShell 7 not found in PATH or common locations."
        Write-Host "Download it here: https://aka.ms/powershell"
        $failed = $true
    }

    Write-Host "`nFor a better script experience, install PowerShell 7+ and ensure your IDE is configured to use it."
    Write-Host "Download PowerShell 7+: https://aka.ms/powershell"
    Write-Host "Configure Visual Studio to use PowerShell 7+: https://stackoverflow.com/a/76045797/6472449`n"
    Write-Host "Configure Visual Studio Code to use PowerShell 7+: https://code.visualstudio.com/docs/terminal/basics#_terminal-shells"
    Write-Host "Configure JetBrains-based IDEs to use PowerShell 7+: https://www.jetbrains.com/help/idea/settings-tools-terminal.html#application-settings (ensure to use pwsh.exe not powershell.exe)"

    if ($failed) {
        exit 1
    } else {
        exit 0
    }
}
Write-Host "Using PowerShell version: $($PSVersionTable.PSVersion)"


# --- Global Variables ---
$scripts = Resolve-Path "$PSScriptRoot/.scripts"


# --- Argument Handling ---
$command = "help"
$passableArgs = @()
if ($args.Count -gt 0) {
    $command = $args[0]
    if ($args.Count -gt 1) {
        $passableArgs = $args[1..($args.Count-1)]
    }
}

# --- Command Handling ---
if ($command.ToLowerInvariant() -eq "setup") {
    $cmd = Join-Path $scripts "setup.ps1"
    if ($VerbosePreference -eq 'Continue') {
        & $cmd @passableArgs -Verbose
    } else {
        & $cmd @passableArgs
    }
    return
}
if ($command.ToLowerInvariant() -eq "local-publish") {
    $cmd = Join-Path $scripts "local-publish.ps1"
    if ($VerbosePreference -eq 'Continue') {
        & $cmd @passableArgs -Verbose
    } else {
        & $cmd @passableArgs
    }
    return
}
if ($command.ToLowerInvariant() -eq "dump") {
    if ($passableArgs.Count -gt 0) {
        $type = $passableArgs[0].ToLowerInvariant()
        if ($type.ToLowerInvariant() -eq "commits") {
            $cmd = Join-Path $scripts "dump" "dump-commits.ps1"
            if ($VerbosePreference -eq 'Continue') {
                & $cmd @passableArgs -Verbose
            } else {
                & $cmd @passableArgs
            }
            return
        }
        if ($type.ToLowerInvariant() -eq "structure") {
            $cmd = Join-Path $scripts "dump" "dump-structure.ps1"
            if ($VerbosePreference -eq 'Continue') {
                & $cmd @passableArgs -Verbose
            } else {
                & $cmd @passableArgs
            }
            return
        }
        if ($type.ToLowerInvariant() -eq "symbols") {
            $cmd = Join-Path $scripts "dump" "dump-symbols.ps1"
            if ($VerbosePreference -eq 'Continue') {
                & $cmd @passableArgs -Verbose
            } else {
                & $cmd @passableArgs
            }
            return
        }
    }
    
    # --- Sub-Help Command ---
    Write-Host
    Write-Host "`e[1;34mCatalystUI Configuration Script`e[0m"
    Write-Host
    Write-Host "Usage: `e[33m./configure dump`e[0m `e[90m[type]`e[0m"
    Write-Host "Available Commands:"
    Write-Host
    Write-Host "    `e[33mcommits`e[0m         Dumps all commits to a text file"
    Write-Host "    `e[33mstructure`e[0m       Dumps the solution structure to a text file"
    Write-Host "    `e[33msymbols`e[0m         Dumps all symbols to a text file"
    Write-Host
    return
}


# --- Help Command ---
Write-Host
Write-Host "`e[1;34mCatalystUI Configuration Script`e[0m"
Write-Host
Write-Host "Usage: `e[33m./configure`e[0m `e[90m[command] [options]`e[0m"
Write-Host "Available Commands:"
Write-Host
Write-Host "    `e[33msetup`e[0m                           Perform initial environment setup"
Write-Host "    `e[33mlocal-publish`e[0m `e[90m[project name]`e[0m    Publish a project to the local repository"
Write-Host "    `e[33mdump`e[0m `e[90m[type]`e[0m                     Dump project information"
Write-Host
