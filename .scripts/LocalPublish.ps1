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
param(
    [Parameter(Mandatory=$true)]
    [string]$projectName
)

$config = Import-PowerShellDataFile "$PSScriptRoot/Config.psd1"
$verbose = $PSBoundParameters["Verbose"] -or $VerbosePreference -eq "Continue"

. "${PSScriptRoot}/ImportModules.ps1"

# Ensure signatures are created
if (-not (Initialize-Signatures (Get-LocationAllProjects) -Verbose:$verbose)) {
    Write-Error "Failed to initialize signatures. Aborting setup."
    exit 1
}

# Prepare the local repository
if (-not (Initialize-NuGetRepository (Get-LocationSolution) -Verbose:$verbose)) {
    Write-Error "Failed to initialize NuGet repository. Aborting setup."
    exit 1
}

# Project dependencies helper function
function Get-ReferencedLocalProjects {
    [CmdletBinding()]
    param([string]$projectFile)
    
    Write-Verbose "`n🔍 Scanning for <PackageReference> elements in:`e[36m $projectFile `e[0m"
    if (-not (Test-Path $projectFile)) {
        Write-Verbose "`e[31m❌ Project file not found:`e[0m $projectFile"
        return @()
    }

    $xml = [xml](Get-Content $projectFile)
    $packageRefs = $xml.SelectNodes("//PackageReference")
    if (-not $packageRefs -or $packageRefs.Count -eq 0) {
        Write-Verbose "ℹ️  No <PackageReference> elements found."
        return @()
    }
    Write-Verbose "📦 Found $($packageRefs.Count) PackageReference(s). Checking for matching local projects..."

    $foundProjects = @()
    foreach ($ref in $packageRefs) {
        $id = $ref.Include
        if ([string]::IsNullOrWhiteSpace($id)) { continue }

        # Try to find a matching .csproj anywhere in the repo
        $projectMatch = Get-LocationProject $id -ErrorAction SilentlyContinue
        if (-not $projectMatch) {
            Write-Verbose "❌ No local project found for package '$id'. Skipping."
            continue
        }
        $csprojFile = Get-ChildItem -Path $projectMatch -Filter *.csproj | Select-Object -First 1
        if (-not $csprojFile) {
            Write-Verbose "❌ No .csproj file found in project path: $projectMatch. Skipping."
            continue
        } else {
            Write-Verbose "✅ Found local project for package '$id': `e[32m$($csprojFile.FullName)`e[0m"
            $foundProjects += $csprojFile.FullName
        }
    }
    return $foundProjects
}

# Determine the project file's location
$projectPath = Get-LocationProject $projectName -Verbose:$verbose
if (-not $projectPath) {
    Write-Error "Project '$projectName' not found. Aborting."
    exit 1
}
$csproj = $($projectPath | Get-ChildItem -Filter *.csproj | Select-Object -First 1)
if (-not $csproj) {
    Write-Host "WARN: No .csproj file found in project path: `e[32m$projectPath`e[0m"
    return
}

# Update and validate project name
$projectName = [System.IO.Path]::GetFileNameWithoutExtension($csproj.FullName)
$projectLowerName = $projectName.ToLowerInvariant()
Write-Host "Found project: `e[32m$($projectName)`e[0m"

# Prepare build paths
$nugetPath = Get-NuGetRepository -Verbose:$verbose
$nugetSource = $config.NuGetSource
$nugetCachePath = Get-NuGetCache -Verbose:$verbose

# Pack dependencies first using a queue to ensure correct order
$published = [System.Collections.Generic.HashSet[string]]::new()
$queue = New-Object System.Collections.Generic.Queue[System.String]
$queue.Enqueue($csproj.FullName)
while ($queue.Count -gt 0) {
    $currentProjectPath = $queue.Dequeue()
    $currentProjectName = [System.IO.Path]::GetFileNameWithoutExtension($currentProjectPath)
    if ($published.Contains($currentProjectName)) {
        continue
    }

    $dependencies = Get-ReferencedLocalProjects $currentProjectPath
    $unpublishedDeps = $dependencies | Where-Object {
        $depName = [System.IO.Path]::GetFileNameWithoutExtension($_)
        -not $published.Contains($depName)
    }
    if ($unpublishedDeps.Count -gt 0) {
        # Enqueue current again after its deps
        $queue.Enqueue($currentProjectPath)
        foreach ($dep in $unpublishedDeps) {
            $queue.Enqueue($dep)
        }
        continue
    }

    if ($currentProjectName -ne $projectName -and -not [string]::IsNullOrWhiteSpace($currentProjectName)) {
        Write-Host "Publishing dependency: `e[36m$currentProjectName`e[0m"
        Build-Project $currentProjectPath $nugetPath $nugetCachePath -Verbose:$verbose
        if ($LASTEXITCODE -ne 0) {
            Write-Verbose
            Write-Verbose "Failed to pack '$currentProjectName'. Exiting."
            exit $LASTEXITCODE
        }
        Write-Verbose "Packed '$currentProjectName'"
    }
    $published.Add($currentProjectName) | Out-Null
}

# Build the main project
Write-Host "Publishing project: `e[33m$projectName`e[0m"
Build-Project $projectPath $nugetPath $nugetCachePath -Force -Verbose:$verbose
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to pack '$projectName'. Exiting."
    exit $LASTEXITCODE
}
Write-Host "Successfully published '`e[36m$projectName`e[0m' to local NuGet source '`e[32m$nugetSource`e[0m'"