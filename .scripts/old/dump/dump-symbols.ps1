<#
#####################################
 This file is public domain.
 It does not fall under the same licensing as the rest of the project.
 Feel free to use it in your project.

 - FireController#1847
#####################################
#>

# Add .NET tools directory to PATH if not already present
$dotnetTools = Join-Path ($env:USERPROFILE ?? $env:HOME) ".dotnet/tools"
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
roslynator list-symbols ./Crystal/Crystal.sln -o DumpSymbols.txt --documentation --group-by-assembly --ignored-parts containing-namespace containing-namespace-in-type-hierarchy --empty-line-between-members