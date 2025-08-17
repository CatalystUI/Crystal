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


# --- Import Build Functions ---
. ./.scripts/build-functions.ps1


# --- Create Signatures ---
if ($verbose) {
    & ./.scripts/create-signatures.ps1 -Verbose
} else {
    & ./.scripts/create-signatures.ps1
}


# --- Find Project ---
$projectPath = Get-ChildItem -Recurse -Filter "$projectName.csproj" -File | Select-Object -First 1
if (-not $projectPath) {
    Write-Host "Project '$projectName' not found in the current directory or subdirectories."
    exit 1
}
$projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath.FullName)
$projectLowerName = $projectName.ToLowerInvariant()
Write-Host "Found project: `e[32m$($projectPath.Name)`e[0m"


# --- Prepare Local Repository ---
. ./.scripts/prepare-local-repository.ps1


# --- Get Project Dependencies ---
function Get-ReferencedLocalProjects {
    param([string]$projectFile)
    if ($verbose) {
        Write-Host "`n🔍 Scanning for <PackageReference> elements in:`e[36m $projectFile `e[0m"
    }
    if (-not (Test-Path $projectFile)) {
        if ($verbose) {
            Write-Host "`e[31m❌ Project file not found:`e[0m $projectFile"
        }
        return @()
    }

    $xml = [xml](Get-Content $projectFile)
    $packageRefs = $xml.SelectNodes("//PackageReference")
    if (-not $packageRefs -or $packageRefs.Count -eq 0) {
        if ($verbose) {
            Write-Host "ℹ️  No <PackageReference> elements found."
        }
        return @()
    }
    if ($verbose) {
        Write-Host "📦 Found $($packageRefs.Count) PackageReference(s). Checking for matching local projects..."
    }

    $foundProjects = @()
    foreach ($ref in $packageRefs) {
        $id = $ref.Include
        if ([string]::IsNullOrWhiteSpace($id)) { continue }

        # Try to find a matching .csproj anywhere in the repo
        $projectMatch = Get-ChildItem -Recurse -Filter "$id.csproj" -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($projectMatch) {
            if ($verbose) {
                Write-Host "✅ Found local project for package '$id': `e[32m$($projectMatch.FullName)`e[0m"
            }
            $foundProjects += $projectMatch.FullName
        } else {
            if ($verbose) {
                Write-Host "❌ No local project found for package '$id'. Skipping."
            }
        }
    }
    return $foundProjects
}


# --- Pack Dependencies First ---
$published = [System.Collections.Generic.HashSet[string]]::new()
$queue = New-Object System.Collections.Generic.Queue[System.String]
$queue.Enqueue($projectPath.FullName)
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

    if ($currentProjectName -ne $projectName) {
        Write-Host -NoNewline "Publishing dependency: `e[36m$currentProjectName`e[0m"
        Build-Project $currentProjectPath $nugetPath $verbose
        if ($LASTEXITCODE -eq 1) {
            Write-Host "`rSkipping `e[36m$currentProjectName`e[0m, no changes detected."
        } elseif ($LASTEXITCODE -ne 0) {
            if ($verbose) {
                Write-Host
                Write-Host "Failed to pack '$currentProjectName'. Exiting."
            }
            exit $LASTEXITCODE
        }
        if ($verbose) {
            Write-Host "Packed '$currentProjectName'"
        }
    }
    $published.Add($currentProjectName) | Out-Null
}

# --- Build and Pack Project ---
Write-Host "Building and packing project: `e[36m$($projectName)`e[0m"
Build-Project $projectPath.FullName $nugetPath $verbose -force:$true
if ($LASTEXITCODE -ne 0) {
    Write-Host "Packing failed. Exiting."
    exit $LASTEXITCODE
}

# --- Success ---
Write-Host "Successfully published '`e[36m$projectName`e[0m' to local NuGet source '`e[32m$nugetSource`e[0m'"