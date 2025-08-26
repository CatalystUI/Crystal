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
param()

$verbose = $PSBoundParameters["Verbose"] -or $VerbosePreference -eq "Continue"

. "${PSScriptRoot}/ImportModules.ps1"

# Ensure signatures are created
if (-not (Initialize-Signatures (Get-LocationAllProjects) -Verbose:$verbose)) {
    Write-Error "Failed to initialize signatures. Aborting setup."
    exit 1
}

# Prepare and clean the local repository
if (-not (Initialize-NuGetRepository (Get-LocationSolution) -Verbose:$verbose)) {
    Write-Error "Failed to initialize NuGet repository. Aborting setup."
    exit 1
}
if (-not (Clear-NuGetRepository -Verbose:$verbose)) {
    Write-Error "Failed to clear NuGet repository. Aborting setup."
    exit 1
}

# Build all projects in order based on dependents
$nugetPath = Get-NuGetRepository -Verbose:$verbose
$nugetCachePath = Get-NuGetCache -Verbose:$verbose
$projectsPath = Get-LocationSolution -Verbose:$verbose
$projectsList = @(
# Dependency modules must appear above dependent modules in this list
    @{
        Module = "Core"
        Projects = @(
            @{ Folder = "Core"; Name = "Crystal.Core" }
        )
        PromptIgnore = $true
        Depends = @()
    },
    @{
        Module = "Internal_WindowingCore"
        Projects = @(
            @{ Folder = "Windowing"; Name = "Crystal.Windowing" }
        )
        PromptIgnore = $true
        Depends = @("Core")
    },
    @{
        Module = "Windowing (Glfw3)"
        Projects = @(
            @{ Folder = "Windowing"; Name = "Crystal.Windowing.Glfw3" }
        )
        PromptIgnore = $false
        Depends = @("Internal_WindowingCore")
    },
    @{
        Module = "Windowing (All)"
        Projects = @()
        PromptIgnore = $false
        Depends = @(
            "Windowing (Glfw3)"
        )
    }
)

# Build prompt options
$promptOptions = @("All") + (
$projectsList |
        Where-Object { -not $_.PromptIgnore -and $_.Module -ne "All" } |
        Select-Object -ExpandProperty Module -Unique |
        Sort-Object
)

# Prompt user for module selection
Write-Host "Select module to set up:"
for ($i = 0; $i -lt $promptOptions.Count; $i++) {
    Write-Host " [$($i+1)] $($promptOptions[$i])"
}
$selection = Read-Host "Enter the number of your choice"
if ([int]::TryParse($selection, [ref]$null) -and ($selection -ge 1 -and $selection -le $promptOptions.Count)) {
    $selectedPrompt = $promptOptions[$selection - 1]
} else {
    Write-Host "`e[31mInvalid selection. Defaulting to "All".`e[0m"
    $selectedPrompt = "All"
}

# Resolve dependencies
function Get-ModulesWithDependencies {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
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

# Filter modules
if ($selectedPrompt -eq "All") {
    $selectedModules = $projectsList
} else {
    $allModules = Get-ModulesWithDependencies @($selectedPrompt)
    $selectedModules = $projectsList | Where-Object { $allModules.Contains($_.Module) }
}

# Build selected projects
foreach ($module in $selectedModules) {
    foreach ($project in $module.Projects) {
        $projectPath = Get-LocationProject $project.name -Verbose:$verbose
        Write-Host "Processing project: `e[33m$($project.name)`e[0m at `e[32m$projectPath`e[0m"
        Build-Project $projectPath $nugetPath $nugetCachePath -Force -Verbose:$verbose
    }
}

# Final output
Write-Host "`e[32mSetup completed successfully!`e[0m"
Write-Host "You may need to restart your terminal or IDE to apply changes."