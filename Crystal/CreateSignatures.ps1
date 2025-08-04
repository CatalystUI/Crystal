<#
#####################################
 This file is public domain.
 It does not fall under the same licensing as the rest of the project.
 Feel free to use it in your project.

 - FireController#1847
#####################################
#>

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

    Write-Host "`nFor a better script experience, install PowerShell 7+ and ensure your Visual Studio is configured to use it."
    Write-Host "Download PowerShell 7+: https://aka.ms/powershell"
    Write-Host "Configure Visual Studio to use PowerShell 7+: https://stackoverflow.com/a/76045797/6472449`n"

    if ($failed) {
        exit 1
    } else {
        exit 0
    }
}
Write-Host "Using PowerShell version: $($PSVersionTable.PSVersion)"

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
Write-Host "Using sn from: $sn"
Write-Host


# Get the solution directory (current script path)
$SolutionDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $SolutionDir

# Recursively find all .csproj files and create .snk files matching the base name
Get-ChildItem -Path . -Recurse -Filter *.csproj | ForEach-Object {
    $CsprojFile = $_
    $ProjectDir = Split-Path $CsprojFile.FullName -Parent
    $BaseName = [System.IO.Path]::GetFileNameWithoutExtension($CsprojFile.Name)
    $SnkPath = Join-Path $ProjectDir "$BaseName.snk"
    if (Test-Path $SnkPath) {
        Write-Host "Skipping $BaseName — SNK already exists."
    } else {
        Write-Host "Creating SNK for $BaseName..."
        sn -k $SnkPath
    }
}