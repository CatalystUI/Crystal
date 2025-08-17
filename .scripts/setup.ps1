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
# Dependency modules must appear above dependent modules in this list
    @{
        Module = "Core"
        Projects = @(
            @{ Folder = "Core"; Name = "Crystal.Core" }
        )
        PromptIgnore = $true
        Depends = @()
    }
)


# --- Build prompt options dynamically ---
$promptOptions = @("All") + (
$projectsList |
        Where-Object { -not $_.PromptIgnore -and $_.Module -ne "All" } |
        Select-Object -ExpandProperty Module -Unique |
        Sort-Object
)


# --- Prompt User for Module Selection ---
Write-Host "Select module to set up:"
for ($i = 0; $i -lt $promptOptions.Count; $i++) {
    Write-Host " [$($i+1)] $($promptOptions[$i])"
}
$selection = Read-Host "Enter the number of your choice"


# --- Validate selection ---
if ([int]::TryParse($selection, [ref]$null) -and ($selection -ge 1 -and $selection -le $promptOptions.Count)) {
    $selectedPrompt = $promptOptions[$selection - 1]
} else {
    Write-Host "`e[31mInvalid selection. Defaulting to "All".`e[0m"
    $selectedPrompt = "All"
}


# --- Resolve dependencies ---
function Get-ModulesWithDependencies {
    param (
        [string[]] $modules
    )
    $resolved = New-Object System.Collections.Generic.HashSet[string]
    $toProcess = [System.Collections.Generic.Queue[string]]::new()
    foreach ($m in $modules) { $toProcess.Enqueue($m) }

    while ($toProcess.Count -gt 0) {
        $current = $toProcess.Dequeue()
        if (-not $resolved.Contains($current)) {
            $resolved.Add($current) | Out-Null
            $depList = ($projectsList | Where-Object { $_.Module -eq $current }).Depends
            foreach ($dep in $depList) {
                if (-not $resolved.Contains($dep)) {
                    $toProcess.Enqueue($dep)
                }
            }
        }
    }
    return $resolved
}


# --- Filter Modules ---
if ($selectedPrompt -eq "All") {
    $selectedModules = $projectsList
} else {
    $allModules = Get-ModulesWithDependencies @($selectedPrompt)
    $selectedModules = $projectsList | Where-Object { $allModules.Contains($_.Module) }
}


# --- Build Selected Projects ---
foreach ($module in $selectedModules) {
    foreach ($project in $module.Projects) {
        $projectPath = Join-Path $projectsPath (Join-Path $project.Folder (Join-Path $project.Name ("{0}.csproj" -f $project.Name)))
        Write-Host "Processing project: `e[33m$($project.Name)`e[0m at `e[32m$projectPath`e[0m"
        Build-Project $projectPath $nugetPath $verbose -force:$true
    }
}


# --- Final Output ---
Write-Host "`e[32mSetup completed successfully!`e[0m"
Write-Host "You may need to restart your terminal or IDE to apply changes."