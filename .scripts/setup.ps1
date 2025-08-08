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


# --- Import Build Functions ---
. ./.scripts/build-functions.ps1


# --- Create Signatures ---
if ($verbose) {
    & ./.scripts/create-signatures.ps1 -Verbose
} else {
    & ./.scripts/create-signatures.ps1
}


# --- Prepare Local Repository ---
. ./.scripts/prepare-local-repository.ps1 -clear


# --- Package & Publish All Projects In-Order (based on dependents) ---
$projectsPath = Resolve-Path "./Crystal/"
$projectsList = @(
    @{ folder = 'Core'; name = 'Crystal.Core' }
)
foreach ($project in $projectsList) {
    $projectPath = Join-Path $projectsPath (Join-Path $project.folder (Join-Path $project.name ("{0}.csproj" -f $project.name)))
    Write-Host "Processing project: `e[33m$($project.name)`e[0m at `e[32m$projectPath`e[0m"
    Build-Project $projectPath $nugetPath $verbose
}
Restore-Solution $projectsPath $nugetPath


# --- Final Output ---
Write-Host "`e[32mSetup completed successfully!`e[0m"
Write-Host "You may need to restart your terminal or IDE to apply changes."