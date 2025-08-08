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
param ()


# --- Parse Verbose Flag ---
if ($VerbosePreference -eq "Continue") {
    $verbose = $true
} else {
    $verbose = $false
}

# --- Hash/Fingerprint Helpers ---
function Get-ProjectFingerprint {
    param([string]$projectPath)

    $root = Split-Path -Parent $projectPath
    $interesting = @(
        '*.cs',
        '*.csproj',
        '*.props',
        '*.targets',
        'Directory.Packages.props',
        'NuGet.Config',
        'packages.lock.json',
        'global.json'
    )

    # Wrap foreach in parentheses so we can sort/unique afterwards
    $files = @(foreach ($p in $interesting) {
        Get-ChildItem -Path $root -Recurse -File -Filter $p -ErrorAction SilentlyContinue
    }) | Sort-Object FullName -Unique

    if ($verbose) {
        Write-Host "Fingerprinting files for {$projectPath}:" -ForegroundColor Cyan
    }

    $sha = [System.Security.Cryptography.SHA256]::Create()
    foreach ($f in $files) {
        $bytes = [System.IO.File]::ReadAllBytes($f.FullName)
        $null = $sha.TransformBlock($bytes, 0, $bytes.Length, $bytes, 0)
        if ($verbose) {
            $fileHash = [BitConverter]::ToString(
                    ([System.Security.Cryptography.SHA256]::Create()).ComputeHash($bytes)
            ) -replace '-', ''
            Write-Host "  $($f.FullName) [$fileHash]" -ForegroundColor DarkGray
        }
    }
    $null = $sha.TransformFinalBlock([byte[]]::new(0), 0, 0)
    -join ($sha.Hash | ForEach-Object { $_.ToString('x2') })
}

# --- Check if Project Should Be Built ---
function Test-BuildProject {
    param(
        [string]$projectPath,
        [string]$cacheDir
    )
    if (-not (Test-Path $cacheDir)) {
        New-Item -ItemType Directory -Path $cacheDir | Out-Null
        if ($IsWindows) {
            (Get-Item $cacheDir).Attributes += 'Hidden'
        }
    }
    $name = [IO.Path]::GetFileNameWithoutExtension($projectPath)
    $cacheFile = Join-Path $cacheDir "$name.fingerprint"
    $current = Get-ProjectFingerprint $projectPath
    if (Test-Path $cacheFile) {
        $previous = Get-Content $cacheFile -Raw
        if ($previous -eq $current) {
            if ($verbose) { Write-Host "No change in fingerprint for $projectPath." -ForegroundColor Green }
            return $false
        } elseif ($verbose) {
            Write-Host "Fingerprint changed for $projectPath." -ForegroundColor Yellow
            Write-Host "Previous: $previous" -ForegroundColor DarkYellow
            Write-Host "Current : $current" -ForegroundColor DarkYellow
        }
    }
    Set-Content -Path $cacheFile -Value $current -NoNewline
    return $true
}


# --- Loading Function ---
function Show-Loading {
    param (
        [string]$Message,
        [string]$Command,
        [ref]$SpinnerState
    )

    $spinner = @('|','/','-','\')
    $i = 0
    $psi = [System.Diagnostics.ProcessStartInfo]::new()
    $psi.FileName = "pwsh"
    $psi.Arguments = "-NoProfile -Command `$ErrorActionPreference = 'Stop'; $Command"
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true

    # Start the process
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $psi
    $null = $process.Start()
    while (-not $process.HasExited) {
        $clear = ' ' * $SpinnerState.Value
        Write-Host -NoNewline "`r$Message `e[33m$($spinner[$i % $spinner.Length])`e[0m$clear`r$Message `e[33m$($spinner[$i % $spinner.Length])`e[0m"
        Start-Sleep -Milliseconds 100
        $i++
        $SpinnerState.Value = [Math]::Max($SpinnerState.Value, ($Message.Length + 4))
    }

    # Process output after process exit
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    # Final clear of spinner
    $clear = ' ' * $SpinnerState.Value
    Write-Host -NoNewline "`r$clear`r"

    if ($process.ExitCode -ne 0) {
        Write-Host "`n`e[31mCommand failed:`e[0m $Command"
        if ($stdout) {
            Write-Host "`n`e[36m[stdout]`e[0m`n$stdout"
        }
        if ($stderr) {
            Write-Host "`n`e[31m[stderr]`e[0m`n$stderr"
        }
        throw "Command failed with exit code $($process.ExitCode)"
    }
}


# --- Build Project ---
function Build-Project($projectPath, $nugetPath, $verbose, $force = $false) {
    $cacheDir = Join-Path $nugetPath ".buildcache"
    if (-not $force -and (-not (Test-BuildProject -projectPath $projectPath -cacheDir $cacheDir))) {
        if ($verbose) { Write-Host "No changes detected for $projectPath. Skipping build/pack." }
        $global:LASTEXITCODE = 1
        return
    }

    # --- Delete Existing Files ---
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
    $projectLowerName = $projectName.ToLowerInvariant()
    if (Test-Path $nugetPath) {
        Get-ChildItem -Path $nugetPath -Recurse -Filter "$projectLowerName*" | ForEach-Object {
            if ($verbose) {
                Write-Host "Deleting existing file: `e[31m$($_.FullName)`e[0m"
            }
            Remove-Item $_.FullName -Force -Recurse -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        }
    }
    $localsOutput = dotnet nuget locals all --list
    $paths = $localsOutput | ForEach-Object {
        if ($_ -match '^[^:]+:\s*(.+)$') { $matches[1] }
    } | Where-Object { Test-Path $_ }
    foreach ($path in $paths) {
        if ($verbose) {
            Write-Host "Searching in NuGet cache path: $path"
        }
        Get-ChildItem -Path $path -Recurse -Force -ErrorAction SilentlyContinue -Filter "$projectLowerName*" | ForEach-Object {
            if ($verbose) {
                Write-Host "Deleting: `e[31m$($_.FullName)`e[0m"
            }
            Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        }
    }

    # Restore, clean, build, and pack the project
    $spinnerState = 0
    if ($verbose) {
        dotnet restore $projectPath -s $nugetPath --force-evaluate
        dotnet build $projectPath -c Release
        dotnet pack $projectPath -c Release -o $nugetPath -v:diag
    } else {
        Show-Loading "(1/3) Restoring..." "dotnet restore '$projectPath' -s '$nugetPath' --force-evaluate" ([ref]$spinnerState)
        Show-Loading "(2/3) Building project..." "dotnet build '$projectPath' -c Release" ([ref]$spinnerState)
        Show-Loading "(3/3) Packing project..." "dotnet pack '$projectPath' -c Release -o '$nugetPath'" ([ref]$spinnerState)
    }

    # Ensure .nupkg is flushed to disk
    $packageName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
    $nupkgPath = Join-Path $nugetPath "$packageName.*.nupkg"

    # Wait for the file to appear
    $retry = 0
    while (-not (Test-Path $nupkgPath) -and $retry -lt 20) {
        Start-Sleep -Milliseconds 100
        $retry++
    }

    $clear = ' ' * $spinnerState
    Write-Host -NoNewline "`r$clear`r"
}


# --- Restore Solution ---
function Restore-Solution($solutionPath, $nugetPath) {
    $spinnerState = 0
    if ($verbose) {
        dotnet restore $solutionPath -s $nugetPath --force-evaluate
    } else {
        Show-Loading "Restoring solution..." "dotnet restore '$solutionPath' -s '$nugetPath'" ([ref]$spinnerState)
    }
    $clear = ' ' * $spinnerState
    Write-Host -NoNewline "`r$clear`r"
}