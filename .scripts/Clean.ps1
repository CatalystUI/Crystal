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

# Remove the local repository
if (-not (Remove-NuGetRepository -Verbose:$verbose)) {
    Write-Error "Failed to remove NuGet repository. Aborting clean."
    exit 1
}

# Run clean on the solution file
Reset-Solution (Get-LocationSolution) -Verbose:$verbose

Write-Host "`e[32mClean completed successfully!`e[0m"