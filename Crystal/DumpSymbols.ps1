<#
#####################################
 This file is public domain.
 It does not fall under the same licensing as the rest of the project.
 Feel free to use it in your project.

 - FireController#1847
#####################################
#>

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

# Add .NET tools directory to PATH if not already present
$dotnetTools = Join-Path $env:USERPROFILE ".dotnet\tools"
if (-not ($env:PATH -split ';' | ForEach-Object { $_.Trim() }) -contains $dotnetTools) {
    $env:PATH += ";$dotnetTools"
}

# Check if the Roslynator CLI is installed
$roslynator = Get-Command roslynator -ErrorAction SilentlyContinue
if (-not $roslynator) {
    Write-Host "Roslynator CLI not found. Installing as global .NET tool..."
    try {
        dotnet tool install -g roslynator.dotnet.cli
    } catch {
        Write-Host "Failed to install Roslynator CLI. Please ensure .NET SDK is installed and try again."
        exit 1
    }

    # Re-check if roslynator is now available
    $roslynator = Get-Command roslynator -ErrorAction SilentlyContinue
    if (-not $roslynator) {
        Write-Host $env:PATH
        Write-Host
        Write-Host "Roslynator was installed but could not be found in the PATH! Please check your installation."
        exit 1
    }
}
Write-Host "Using Roslynator CLI from: $($roslynator.Source)"
Write-Host

# --- Run Roslynator ---
roslynator list-symbols -o DumpSymbols.txt --documentation --group-by-assembly --ignored-parts containing-namespace containing-namespace-in-type-hierarchy --empty-line-between-members