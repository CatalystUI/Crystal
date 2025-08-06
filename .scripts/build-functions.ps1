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
function Build-Project($projectPath, $nugetPath, $verbose) {
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
        dotnet clean $projectPath
        dotnet build $projectPath -c Release
        dotnet pack $projectPath -c Release -o $nugetPath -v:diag
    } else {
        Show-Loading "(1/4) Restoring..." "dotnet restore '$projectPath' -s '$nugetPath' --force-evaluate" ([ref]$spinnerState)
        Show-Loading "(2/4) Cleaning project..." "dotnet clean '$projectPath'" ([ref]$spinnerState)
        Show-Loading "(3/4) Building project..." "dotnet build '$projectPath' -c Release" ([ref]$spinnerState)
        Show-Loading "(4/4) Packing project..." "dotnet pack '$projectPath' -c Release -o '$nugetPath'" ([ref]$spinnerState)
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