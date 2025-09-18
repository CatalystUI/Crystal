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

$scripts = Resolve-Path "$PSScriptRoot/.scripts"

# Argument handling
$command = "help"
$passableArgs = @()
if ($args.Count -gt 0) {
    $command = $args[0]
    if ($args.Count -gt 1) {
        $passableArgs = $args[1..($args.Count-1)]
    }
}

if ($command.ToLowerInvariant() -eq "setup") {
    $cmd = Join-Path $scripts "Setup.ps1"
    & $cmd @passableArgs
    return
}
if ($command.ToLowerInvariant() -eq "clean") {
    $cmd = Join-Path $scripts "Clean.ps1"
    & $cmd @passableArgs
    return
}
if ($command.ToLowerInvariant() -eq "signatures") {
    $cmd = Join-Path $scripts "MakeSignatures.ps1"
    & $cmd @passableArgs
    return
}
if ($command.ToLowerInvariant() -eq "local-publish") {
    $cmd = Join-Path $scripts "LocalPublish.ps1"
    & $cmd @passableArgs
    return
}
if ($command.ToLowerInvariant() -eq "dump") {
    if ($passableArgs.Count -gt 0) {
        $type = $passableArgs[0].ToLowerInvariant()
        if ($type.ToLowerInvariant() -eq "commits") {
            $cmd = Join-Path $scripts "Dump" "DumpCommits.ps1"
            & $cmd @passableArgs
            return
        }
        if ($type.ToLowerInvariant() -eq "structure") {
            $cmd = Join-Path $scripts "Dump" "DumpStructure.ps1"
            & $cmd @passableArgs
            return
        }
        if ($type.ToLowerInvariant() -eq "symbols") {
            $cmd = Join-Path $scripts "Dump" "DumpSymbols.ps1"
            & $cmd @passableArgs
            return
        }
    }

    # Sub-help command
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


# Help command
Write-Host
Write-Host "`e[1;34mCatalystUI Configuration Script`e[0m"
Write-Host
Write-Host "Usage: `e[33m./configure`e[0m `e[90m[command] [options]`e[0m"
Write-Host "Available Commands:"
Write-Host
Write-Host "    `e[33msetup`e[0m                           Perform initial environment setup"
Write-Host "    `e[33mclean`e[0m                           Clean build artifacts and temporary files"
Write-Host "    `e[33msignatures`e[0m                      Generate or update code signatures"
Write-Host "    `e[33mlocal-publish`e[0m `e[90m[project name]`e[0m    Publish a project to the local repository"
Write-Host "    `e[33mdump`e[0m `e[90m[type]`e[0m                     Dump project information"
Write-Host
