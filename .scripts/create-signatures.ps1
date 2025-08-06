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

param(
    [switch]$verbose
)

# Save the initial working directory
$InitialCwd = Get-Location

# Find the signing tool
$sn = Get-Command sn -ErrorAction SilentlyContinue
if (-not $sn) {
    Write-Host "sn.exe not found in PATH. Searching..."
    if ($IsWindows) {
        $roots = @(
            "${env:ProgramFiles(x86)}",
            "${env:ProgramFiles}"
        );
        $years = @("2017", "2019", "2022");
        $versions = @("BuildTools", "Enterprise", "Professional", "Community");
        foreach ($root in $roots) {
            # Search for Windows SDKs
            $sdkpath = Join-Path $root "Microsoft SDKs";
            if (Test-Path $sdkpath) {
                Get-ChildItem -Path $sdkpath -Recurse -Filter sn.exe -ErrorAction SilentlyContinue | ForEach-Object {
                    $sn = $_.FullName;
                    $env:PATH += "$([IO.Path]::PathSeparator)$($sn | Split-Path -Parent)"
                    break;
                }
            }

            # Perform a longer search for sn.exe in Visual Studio directories
            foreach ($year in $years) {
                foreach ($version in $versions) {
                    $studiopath = Join-Path $root "Microsoft Visual Studio\$year\${version}\"
                    if (!(Test-Path $studiopath)) { continue; }
                    Get-ChildItem -Path $testpath -Recurse -Filter sn.exe -ErrorAction SilentlyContinue | ForEach-Object {
                        $sn = $_.FullName;
                        $env:PATH += "$([IO.Path]::PathSeparator)$($sn | Split-Path -Parent)"
                        break;
                    }
                }
            }
        }
    } else {
        $roots = @(
            "/usr/bin",
            "/usr/local/bin",
            "/Library/Frameworks/Mono.framework/"
        );
        foreach ($root in $roots) {
            if (Test-Path $root) {
                Get-ChildItem -Path $root -Recurse -Filter sn -ErrorAction SilentlyContinue | ForEach-Object {
                    $sn = $_.FullName;
                    $env:PATH += "$([IO.Path]::PathSeparator)$($sn | Split-Path -Parent)"
                    break;
                }
            }
        }
    }
}
if (-not $sn) {
    Write-Host "Could not locate 'sn' or 'sn.exe' in known locations or PATH."
    if (-not $IsWindows) {
        Write-Host "On Linux/macOS, try: sudo apt install mono-devel   or   brew install mono"
    }
    exit 1
}
$resolvedSn = (Get-Command $sn).Source
Write-Host "Using sn from: $resolvedSn"

# Get the solution directory (set to ../Crystal relative to script)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$SolutionDir = Join-Path $ScriptDir ".." | Join-Path -ChildPath "Crystal" | Resolve-Path -ErrorAction Stop
Set-Location $SolutionDir

# Recursively find all .csproj files and create .snk files matching the base name
try {
    Get-ChildItem -Path . -Recurse -Filter *.csproj | ForEach-Object {
        $CsprojFile = $_
        $ProjectDir = Split-Path $CsprojFile.FullName -Parent
        $BaseName = [System.IO.Path]::GetFileNameWithoutExtension($CsprojFile.Name)
        $SnkPath = Join-Path $ProjectDir "$BaseName.snk"
        if (Test-Path $SnkPath) {
            Write-Host "Skipping $BaseName — `e[31mSNK already exists.`e[0m"
        } else {
            Write-Host "Creating SNK for $BaseName..."
            if ($verbose) {
                sn -k $SnkPath
            } else {
                sn -k $SnkPath > $null 2>&1
            }
        }
    }
} finally {
    # Always return to the initial working directory
    Set-Location $InitialCwd
}