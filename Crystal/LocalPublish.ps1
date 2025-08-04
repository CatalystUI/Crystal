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

param (
    [Parameter(Mandatory = $true)]
    [string]$projectName
)

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

# --- Find Project ---
$projectPath = Get-ChildItem -Recurse -Filter "$projectName.csproj" -File | Select-Object -First 1
if (-not $projectPath) {
    Write-Host "Project '$projectName' not found in the current directory or subdirectories."
    exit 1
}
$projectLowerName = $projectPath.Name.ToLowerInvariant() -replace '\.csproj$', ''
Write-Host "Found project: `e[32m$($projectPath.Name)`e[0m"

# --- Get Project Dependencies ---
function Get-ReferencedLocalProjects {
    param([string]$projectFile)

    Write-Host "`n🔍 Scanning for <PackageReference> elements in:`e[36m $projectFile `e[0m"

    if (-not (Test-Path $projectFile)) {
        Write-Host "`e[31m❌ Project file not found:`e[0m $projectFile"
        return @()
    }

    $xml = [xml](Get-Content $projectFile)
    $packageRefs = $xml.SelectNodes("//PackageReference")
    if (-not $packageRefs -or $packageRefs.Count -eq 0) {
        Write-Host "ℹ️  No <PackageReference> elements found."
        return @()
    }

    Write-Host "📦 Found $($packageRefs.Count) PackageReference(s). Checking for matching local projects..."

    $foundProjects = @()
    foreach ($ref in $packageRefs) {
        $id = $ref.Include
        if ([string]::IsNullOrWhiteSpace($id)) { continue }

        # Try to find a matching .csproj anywhere in the repo
        $projectMatch = Get-ChildItem -Recurse -Filter "$id.csproj" -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($projectMatch) {
            Write-Host "✅ Found local project for package '$id': `e[32m$($projectMatch.FullName)`e[0m"
            $foundProjects += $projectMatch.FullName
        } else {
            Write-Host "❌ No local project found for package '$id'. Skipping."
        }
    }

    return $foundProjects
}


# --- Pack Dependencies First ---
$referencedProjects = Get-ReferencedLocalProjects $projectPath.FullName
foreach ($depPath in $referencedProjects) {
    $depName = [System.IO.Path]::GetFileNameWithoutExtension($depPath)
    Write-Host "`n🧩 Found referenced project: `e[36m$depName`e[0m — publishing it first..."
    & $PSCommandPath $depName
    Write-Host "`n🔄 Restoring after '$depName' to update NuGet resolution for main project..."
    dotnet restore $projectPath.FullName
}


# --- Prepare Local Repository ---
$nugetPath = Join-Path $HOME ".catalystui/crystal"
$nugetSource = "CatalystUI Crystal Local Repository"
if (-not (Test-Path $nugetPath)) {
    Write-Host "Creating local NuGet repository at: `e[32m$nugetPath`e[0m"
    New-Item -ItemType Directory -Path $nugetPath | Out-Null
    (Get-Item $nugetPath).Attributes += 'Hidden'
} else {
    Write-Host "Using existing local NuGet repository at: `e[32m$nugetPath`e[0m"
}

# --- Delete Existing Files ---
if (Test-Path $nugetPath) {
    Get-ChildItem -Path $nugetPath -Recurse -Filter "$projectLowerName*" | ForEach-Object {
        Write-Host "Deleting existing file: `e[31m$($_.FullName)`e[0m"
        Remove-Item $_.FullName -Force -Recurse
    }
}
$localsOutput = dotnet nuget locals all --list
$paths = $localsOutput | ForEach-Object {
    if ($_ -match '^[^:]+:\s*(.+)$') { $matches[1] }
} | Where-Object { Test-Path $_ }
foreach ($path in $paths) {
    Write-Host "Searching in: $path"
    Get-ChildItem -Path $path -Recurse -Force -ErrorAction SilentlyContinue -Filter "$projectLowerName*" | ForEach-Object {
        Write-Host "Deleting: `e[31m$($_.FullName)`e[0m"
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
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

# --- Build and Pack Project ---
Write-Host "Building and packing project: `e[32m$($projectPath.Name)`e[0m"
Write-Host ""
dotnet clean $projectPath.FullName
dotnet build $projectPath.FullName -c Release
dotnet pack $projectPath.FullName -c Release -o $nugetPath -v:diag
if ($LASTEXITCODE -ne 0) {
    Write-Host "Packing failed. Exiting."
    exit $LASTEXITCODE
}

Write-Host "`n✅ Successfully published '$projectName' to local NuGet source '$nugetSource'"