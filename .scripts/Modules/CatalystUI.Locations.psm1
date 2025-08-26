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
# Name: CatalystUI.Locations.psm1
# Author: FireController#1847
# Date: 2025-08-24
# Version: 1.0.0

function Get-LocationRepository {
    <#
    .SYNOPSIS
        Gets the root directory of the CatalystUI repository.
        
    .DESCRIPTION
        Gets the root directory of the CatalystUI repository
        by navigating two levels up from the script's location.
        Returns the full path to the repository root.
        
    .OUTPUTS
        [string]
        The full path to the root directory of the repository.
        
    .EXAMPLE
        PS C:\> Get-LocationRepository
        C:\Path\To\CatalystUI
    #>
    [CmdletBinding()]
    param()
    
    $script = $PSCommandPath
    return Resolve-Path (Join-Path (Split-Path $script -Parent) "../../")
}

function Get-LocationSolution {
    <#
    .SYNOPSIS
        Gets the directory containing the main solution file.
    
    .DESCRIPTION
        Gets the directory containing the main solution file
        by searching for any .sln file within 2 levels of the
        repository root. Returns the full path to the directory
        containing the first found solution file.
    
    .OUTPUTS
        [string]
        The full path to the directory containing the solution file.
     
    .EXAMPLE
        PS C:\> Get-LocationSolution
        C:\Path\To\CatalystUI\CatalystUI
    #>
    [CmdletBinding()]
    param()
    
    $repositoryRoot = Get-LocationRepository
    $slnFile = Get-ChildItem -Path $repositoryRoot -Filter "*.sln" -Recurse -Depth 2 -File | Select-Object -First 1
    if ($null -eq $slnFile) {
        throw "No solution (.sln) file found within 2 levels of $repositoryRoot."
    }
    return Split-Path $slnFile.FullName -Parent
}

function Get-LocationAllProjects {
    <#
    .SYNOPSIS
        Gets the directories of all projects in the solution.
    
    .DESCRIPTION
        Gets the directories of all projects in the solution by
        searching for all .csproj files within the solution directory.
        
        Returns an array of unique full paths to the directories
        containing each project file.
    
    .OUTPUTS
        [string[]]
        An array of full paths to the directories containing each project file.
    
    .EXAMPLE
        PS C:\> Get-LocationAllProjects
        C:\Path\To\CatalystUI\Project1
        C:\Path\To\CatalystUI\Project2
        C:\Path\To\CatalystUI\Subfolder\Project3
    #>
    [CmdletBinding()]
    param()
    
    $solutionRoot = Get-LocationSolution
    return Get-ChildItem -Path $solutionRoot -Recurse -Filter "*.csproj" -File | ForEach-Object {
        Split-Path $_.FullName -Parent
    } | Sort-Object -Unique
}

function Get-LocationProject {
    <#
    .SYNOPSIS
        Gets the directory of a specific project by its name.
        
    .DESCRIPTION
        Gets the directory of a specific project by its name by
        searching for the corresponding .csproj
        file within the solution directory.
        
        Returns the full path to the directory containing
        the specified project file, or $null if not found
        or if the projectName is null or whitespace.
    
    .PARAMETER projectName
        The name of the project (without the .csproj extension).
    
    .INPUTS
        [string] projectName
        The name of the project to locate.
    
    .OUTPUTS
        [string]
        The full path to the directory containing the specified project file, or $null if not found
        or if the projectName is null or whitespace.
    
    .EXAMPLE
        PS C:\> Get-LocationProject -projectName "CatalystUI.Core"
        C:\Path\To\CatalystUI\CatalystUI.Core
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline=$true)]
        [string] $projectName
    )
    
    if ([string]::IsNullOrWhiteSpace($projectName)) {
        return $null
    }
    $solutionRoot = Get-LocationSolution
    $projectPath = Get-ChildItem -Path $solutionRoot -Recurse -Filter "$projectName.csproj" -File | Select-Object -First 1
    if ($null -ne $projectPath) {
        return Split-Path $projectPath.FullName -Parent
    } else {
        return $null
    }
}

Export-ModuleMember -Function Get-LocationRepository, Get-LocationSolution, Get-LocationAllProjects, Get-LocationProject
