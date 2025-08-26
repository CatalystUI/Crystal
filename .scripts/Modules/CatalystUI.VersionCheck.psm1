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
# Name: CatalystUI.VersionCheck.psm1
# Author: FireController#1847
# Date: 2025-08-24
# Version: 1.1.0

function Test-PowerShell {
    <#
    .SYNOPSIS
        Ensures the script is running in PowerShell 7 or higher.
        
    .DESCRIPTION
        Ensures the script is running in PowerShell 7 or higher.
        If the version is lower than 7, displays an error message
        with instructions to install PowerShell 7+ and exits
        the script with a non-zero exit code.
        
        Returns the current PowerShell version if valid.
        
    .OUTPUTS
        [version]
        The current PowerShell version if it is 7 or higher.
        
    .EXAMPLE
        PS C:\> Test-PowerShell
        Using PowerShell version: 7.2.0
        Major  Minor  Build  Revision
        -----  -----  -----  --------
        7      2      0      0
    #>
    [CmdletBinding()]
    param()
    
    Write-Host "Using PowerShell version: $($PSVersionTable.PSVersion)" -ForegroundColor DarkGray
    if ($PSVersionTable.PSVersion.Major -lt 7) {
        Write-Host "`a"
        Write-Host "Invalid PowerShell version. PowerShell 7+ is required." -ForegroundColor DarkRed

        Write-Host "`nPlease install PowerShell 7+ and ensure your IDE is configured to use it."  -ForegroundColor White
        Write-Host "Download PowerShell 7+: " -ForegroundColor White -NoNewline
        Write-Host "https://aka.ms/powershell" -ForegroundColor Green

        Write-Host "`n- Configure " -NoNewline
        Write-Host "Visual Studio" -ForegroundColor DarkCyan -NoNewline
        Write-Host " to use PowerShell 7+: " -NoNewline
        Write-Host "https://stackoverflow.com/a/76045797/6472449" -ForegroundColor DarkGreen

        Write-Host "- Configure " -NoNewline
        Write-Host "Visual Studio Code" -ForegroundColor DarkCyan -NoNewline
        Write-Host " to use PowerShell 7+: " -NoNewline
        Write-Host "https://code.visualstudio.com/docs/terminal/basics#_terminal-shells" -ForegroundColor DarkGreen

        Write-Host "- Configure " -NoNewline
        Write-Host "JetBrains-based IDEs" -ForegroundColor DarkCyan -NoNewline
        Write-Host " to use PowerShell 7+: " -NoNewline
        Write-Host "https://www.jetbrains.com/help/idea/settings-tools-terminal.html#application-settings" -ForegroundColor DarkGreen

        Write-Host "`nThe executable for modern PowerShell 7+ is " -ForegroundColor White -NoNewline
        Write-Host "pwsh" -ForegroundColor DarkYellow -NoNewline
        Write-Host ", not " -ForegroundColor White -NoNewline
        Write-Host "powershell" -ForegroundColor DarkYellow -NoNewline
        Write-Host ", which is the legacy Windows PowerShell." -ForegroundColor White
        Write-Host

        # Exit after attempt
        exit 1
    }
    return $PSVersionTable.PSVersion
}

Export-ModuleMember -Function Test-PowerShell