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
# Name: CatalystUI.Signatures.psm1
# Author: FireController#1847
# Date: 2025-08-24
# Version: 1.1.0

function Initialize-Sn {
    <#
    .SYNOPSIS
        Ensures the Strong Name tool (`sn` or `sn.exe`) is available on the system.
        
    .DESCRIPTION
        Ensures the Strong Name tool (`sn` or `sn.exe`) is available on the system.
        If not found, attempts to locate it in common installation paths.
        If still not found, attempts to download and install it using
        platform-appropriate package managers (winget for Windows, brew for macOS, apt/dnf for Linux).
        
        Returns the full path to the `sn` executable if found, otherwise $null.
    
    .OUTPUTS
        [string]
        The full path to the `sn` executable if found, otherwise $null.
    
    .EXAMPLE
        PS C:\> Initialize-Sn
        Using `e[35msn`e[0m from: `e[32mC:\Program Files\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\sn.exe`e[0m
        C:\Program Files\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\sn.exe
    #>
    [CmdletBinding()]
    param()
    
    function Test-Sn {
        [CmdletBinding()]
        param()
        
        $sn = Get-Command sn -ErrorAction SilentlyContinue
        if ($sn) { return $sn.Source; }
        if ($IsWindows) {
            $roots = @("${env:ProgramFiles(x86)}", "${env:ProgramFiles}")
            $years = @("2017", "2019", "2022")
            $versions = @("BuildTools", "Enterprise", "Professional", "Community")
            foreach ($root in $roots) {
                # Search SDKs
                $sdkpath = Join-Path $root "Microsoft SDKs"
                if (Test-Path $sdkpath) {
                    foreach ($exe in (Get-ChildItem -Path $sdkpath -Recurse -Filter sn.exe -ErrorAction SilentlyContinue)) {
                        $env:PATH += "$([IO.Path]::PathSeparator)$($exe.DirectoryName)"
                        return $exe.FullName
                    }
                }

                # Search Visual Studio
                foreach ($year in $years) {
                    foreach ($version in $versions) {
                        $studiopath = Join-Path $root "Microsoft Visual Studio\$year\$version"
                        if (-not (Test-Path $studiopath)) { continue }
                        foreach ($exe in (Get-ChildItem -Path $studiopath -Recurse -Filter sn.exe -ErrorAction SilentlyContinue)) {
                            $env:PATH += "$([IO.Path]::PathSeparator)$($exe.DirectoryName)"
                            return $exe.FullName
                        }
                    }
                }
            }
        } else {
            $roots = @("/usr/bin", "/usr/local/bin", "/Library/Frameworks/Mono.framework/")
            foreach ($root in $roots) {
                if (Test-Path $root) {
                    foreach ($exe in (Get-ChildItem -Path $root -Recurse -Filter sn -ErrorAction SilentlyContinue)) {
                        $env:PATH += "$([IO.Path]::PathSeparator)$($exe.DirectoryName)"
                        return $exe.FullName
                    }
                }
            }
        }
        return $null
    }
    
    function Download-Sn {
        [CmdletBinding()]
        param()
        
        if ($IsWindows) {
            Write-Host "WARN: `e[35msn.exe`e[0m not found. Attempting `e[35mwinget`e[0m install..."
            $winget = Get-Command winget -ErrorAction SilentlyContinue
            if ($winget) {
                winget install --id=Microsoft.VisualStudio.2022.BuildTools --silent --accept-source-agreements --accept-package-agreements
            } else {
                Write-Host "`e[31mwinget not found. Cannot install sn.exe automatically.`e[0m"
            }
        } elseif ($IsMacOS) {
            Write-Host "WARN: `e[35msn`e[0m not found. Attempting `e[35mhomebrew`e[0m install..."
            $brew = Get-Command brew -ErrorAction SilentlyContinue
            if ($brew) {
                brew install mono
            } else {
                Write-Host "`e[31mbrew not found. Please install Homebrew or Mono manually:`e[0m `e[92mhttps://www.mono-project.com/download/stable/`e[0m"
            }
        } elseif ($IsLinux) {
            Write-Host "WARN: `e[35msn`e[0m not found. Attempting `e[35mapt`e[0m/`e[35mdnf`e[0m install..."
            $apt = Get-Command apt -ErrorAction SilentlyContinue
            $dnf = Get-Command dnf -ErrorAction SilentlyContinue
            if ($apt) {
                sudo apt update
                sudo apt install -y mono-devel
            } elseif ($dnf) {
                sudo dnf install -y mono-devel
            } else {
                Write-Host "`e[31mNo known package manager (apt/dnf) detected. Please install Mono manually:`e[0m `e[92mhttps://www.mono-project.com/download/stable/`e[0m"
            }
        }
    }

    # Attempt to find it
    $sn = Test-Sn
    
    # If it doesn't exist, attempt to download it
    if (-not $sn) {
        Download-Sn
        $sn = Test-Sn
    }
    
    # If it still doesn't exist, we can't proceed
    if (-not $sn) {
        Write-Host "`e[31mCould not locate 'sn' or 'sn.exe' in known locations or PATH.`e[0m"
        if ($IsWindows) {
            Write-Host "On Windows, try installing Visual Studio Build Tools or the .NET SDK."
        } else {
            Write-Host "On Linux/macOS, try: sudo apt install mono-devel   or   brew install mono"
        }
        return $null
    }
    
    # Resolve and return
    $resolvedSn = (Get-Command $sn).Source
    Write-Host "Using `e[35msn`e[0m from: `e[32m$resolvedSn`e[0m"
    return $resolvedSn
}

function Initialize-Signatures {
    <#
    .SYNOPSIS
        Ensures all specified projects have a strong name key (.snk) file.
     
    .DESCRIPTION
        Ensures all specified projects have a strong name key (.snk) file.
        If a project does not have an .snk file, one will be created
        using the Strong Name tool (`sn` or `sn.exe`).
        
        Returns $true if the operation completes successfully, otherwise $false.
        
    .INPUTS
        [string[]] projects
        An array of project directory paths to ensure have .snk files.
        
    .OUTPUTS
        [bool]
        $true if the operation completes successfully, otherwise $false.
        
    .EXAMPLE
        PS C:\> Initialize-Signatures -projects @("C:\Path\To\Project1", "C:\Path\To\Project2")
        Creating SNK for `e[36mProject1`e[0m...
        Creating SNK for `e[36mProject2`e[0m...
        True
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string[]] $projects
    )
    
    $cwd = Get-Location
    try {
        $sn = Initialize-Sn
        if (-not $sn) { return $false }
        foreach ($projectPath in $projects) {
            # Check path exists
            if (-not (Test-Path $projectPath)) {
                Write-Host "WARN: Project path does not exist: `e[32m$projectPath`e[0m"
                continue
            }
            
            # Determine snk path
            $csproj = $($projectPath | Get-ChildItem -Filter *.csproj | Select-Object -First 1)
            if (-not $csproj) {
                Write-Host "WARN: No .csproj file found in project path: `e[32m$projectPath`e[0m"
                continue
            }
            $name = [System.IO.Path]::GetFileNameWithoutExtension($csproj.Name)
            $snk = Join-Path $projectPath "$name.snk"
            if (-not (Test-Path $snk)) {
                Write-Host "Creating SNK for `e[36m$name`e[0m..."
                if ($PSBoundParameters['Verbose'] -or $VerbosePreference -eq 'Continue') {
                    sn -k $snk
                } else {
                    sn -k $snk > $null 2>&1
                }
            } else {
                Write-Verbose "Skipping `e[36m$name`e[0m — SNK already exists."
                continue
            }
        }
        return $true
    } finally {
        Set-Location $cwd
    }
}

Export-ModuleMember -Function Initialize-Sn, Initialize-Signatures