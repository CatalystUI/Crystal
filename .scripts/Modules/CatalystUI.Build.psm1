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
# Name: CatalystUI.Build.psm1
# Author: FireController#1847
# Date: 2025-08-24
# Version: 2.0.1

function Show-Loading {
    <#
    .SYNOPSIS
        Displays a loading spinner while executing a script block.
        
    .DESCRIPTION
        This function shows a loading spinner in the console while
        executing a provided script block. It runs the script block
        in a background thread and updates the spinner until the action
        completes. If the action fails, it captures and displays any
        errors or exceptions.
        
    .PARAMETER message
        The message to display alongside the spinner.
        
    .PARAMETER action
        The script block to execute while showing the spinner.
        
    .PARAMETER argumentList
        An array of arguments to pass to the script block.
        
    .PARAMETER spinnerState
        A reference variable to track the maximum length of the spinner line.
        
    .INPUTS
        [string] message
        The message to display alongside the spinner.
        
        [ScriptBlock] action
        The script block to execute while showing the spinner.
        
        [object[]] argumentList
        An array of arguments to pass to the script block.
        
        [ref] spinnerState
        A reference variable to track the maximum length of the spinner line.
        
    .EXAMPLE
        PS C:\> Show-Loading -message "Processing" -action { Start-Sleep -Seconds 5 }
        Displays a spinner with the message "Processing" for 5 seconds.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $message,
        [Parameter(Mandatory)]
        [ScriptBlock] $action,
        [object[]] $argumentList = @(),
        [ref] $spinnerState = ([ref]0)
    )

    $spinner = @('|','/','-','\')
    $i = 0

    # Start the work in a background thread
    $job = Start-ThreadJob -ScriptBlock {
        param($sb, $argsList)
        try {
            & $sb @argsList
        } catch {
            throw
        }
    } -argumentList $action, $argumentList

    try {
        while ($true) {
            $state = $job.State
            if ($state -in 'Completed','Failed','Stopped') {
                break
            }
            $clear = ' ' * $spinnerState.Value
            Write-Host -NoNewline "`r$message `e[33m$($spinner[$i % $spinner.Length])`e[0m$clear`r$message `e[33m$($spinner[$i % $spinner.Length])`e[0m"
            Start-Sleep -Milliseconds 100
            $i++
            $spinnerState.Value = [Math]::Max($spinnerState.Value, ($message.Length + 4))
        }

        # Wait for the job to finish, gather results and errors
        $results = Receive-Job -Job $job -Keep -Erroraction SilentlyContinue -ErrorVariable jobErr
        $jobReason = $job.ChildJobs[0].JobStateInfo.Reason

        # Final clear of spinner line
        $clear = ' ' * $spinnerState.Value
        Write-Host -NoNewline "`r$clear`r"

        # Write output if there was an error or exception
        if ($job.State -ne 'Completed' -or $jobReason -or $jobErr) {
            $stdOut   = Receive-Job -Job $job -Keep -ErrorAction SilentlyContinue
            $errStream = $job.ChildJobs[0].Error
            $wrnStream = $job.ChildJobs[0].Warning
            $vrbStream = $job.ChildJobs[0].Verbose
            $infStream = $job.ChildJobs[0].Information
            $dbgStream = $job.ChildJobs[0].Debug

            if ($stdOut) {
                Write-Host "`n`e[31m[stdout]`e[0m`n$($stdOut | Out-String)"
            }
            if ($errStream) {
                Write-Host "`n`e[31m[stderr]`e[0m`n$($errStream | ForEach-Object { $_.ToString() } | Out-String)"
            }
            if ($wrnStream) {
                Write-Host "`n`e[33m[warning]`e[0m`n$($wrnStream | ForEach-Object { $_.ToString() } | Out-String)"
            }
            if ($vrbStream) {
                Write-Host "`n`e[36m[verbose]`e[0m`n$($vrbStream | ForEach-Object { $_.ToString() } | Out-String)"
            }
            if ($infStream) {
                Write-Host "`n`e[34m[information]`e[0m`n$($infStream | ForEach-Object { $_.ToString() } | Out-String)"
            }
            if ($dbgStream) {
                Write-Host "`n`e[35m[debug]`e[0m`n$($dbgStream | ForEach-Object { $_.ToString() } | Out-String)"
            }

            if ($jobErr) {
                Write-Host "`n`e[31m[errors]`e[0m`n$($jobErr | Out-String)"
            }
            if ($jobReason) {
                Write-Host "`n`e[31m[exception]`e[0m`n$($jobReason | Out-String)"
            }

            throw "action failed (JobState: $($job.State))."
        }
        return $results
    } finally {
        if ($job) {
            Remove-Job -Job $job -Force -Erroraction SilentlyContinue
        }
    }
}

function Build-Project {
    <#
    .SYNOPSIS
        Builds and packs a .NET project into a NuGet package.
        
    .DESCRIPTION
        This function builds a specified .NET project and creates a NuGet package (.nupkg).
        It first removes any existing .nupkg files for the project in the specified NuGet
        repository path. It then restores, builds, and packs the project using the `dotnet`
        CLI. The function supports a `-Force` switch to force a rebuild by cleaning the project
        first.
        
    .PARAMETER projectPath
        The path to the project directory containing the .csproj file.
        
    .PARAMETER nugetPath
        The path to the local NuGet repository where the .nupkg file will be output
        
    .PARAMETER nugetCachePath
        The path to the local NuGet cache to clean cached packages from.
        
    .PARAMETER force
        A switch to force a rebuild by cleaning the project first.
        
    .INPUTS
        [string] projectPath
        The path to the project directory containing the .csproj file.
        
        [string] nugetPath
        The path to the local NuGet repository where the .nupkg file will be output.
        
        [string] nugetCachePath
        The path to the local NuGet cache to clean cached packages from.
        
        [switch] force
        A switch to force a rebuild by cleaning the project first.
        
    .EXAMPLE
        PS C:\> Build-Project -projectPath "C:\Path\To\Project" -nugetPath "C:\Path\To\NuGetRepo" -Force
        Builds and packs the project at "C:\Path\To\Project",
        outputting the .nupkg to "C:\Path\To\NuGetRepo",
        forcing a rebuild.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string] $projectPath,
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string] $nugetPath,
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string] $nugetCachePath,
        [switch] $force
    )

    # Determine the project file's location
    $csproj = $($projectPath | Get-ChildItem -Filter *.csproj | Select-Object -First 1)
    if (-not $csproj) {
        Write-Host "WARN: No .csproj file found in project path: `e[32m$projectPath`e[0m"
        return
    }

    # Determine the project name and expected .nupkg path
    $packageName = [System.IO.Path]::GetFileName($projectPath)
    $nupkgPath = Join-Path $nugetPath "$packageName.*.nupkg"

    # Remove existing .nupkg files
    if (Test-Path $nupkgPath) {
        Get-ChildItem -Path $nugetPath -Recurse -Filter "$packageName.*.nupkg" | ForEach-Object {
            Write-Verbose "Deleting existing file: `e[31m$($_.FullName)`e[0m"
            Remove-Item $_.FullName -Force -Recurse -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        }
    }

    # Clean cached NuGet packages
    if (Test-Path $nugetCachePath) {
        Get-ChildItem -Path $nugetCachePath -Recurse -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -ieq $packageName } | ForEach-Object {
            Write-Verbose "Deleting cached package: `e[31m$($_.FullName)`e[0m"
            Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        }
    }

    # Argument sets
    $restoreArgs = @("--disable-parallel", "--force-evaluate")
    if ($force) {
        $restoreArgs += "--no-cache"
    }
    $msbuildProps = @("/p:RestorePackagesPath=$nugetCachePath")
    $buildArgs = @("--configuration", "Release", "--no-restore") + $msbuildProps
    if ($force) {
        $buildArgs += "-t:Rebuild"
    }

    # Restore, build, and pack
    if ($PSBoundParameters["Verbose"] -or $VerbosePreference -eq "Continue") {
        dotnet restore $csproj.FullName @restoreArgs @msbuildProps
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet restore failed with exit code $LASTEXITCODE"
        }

        if ($force) {
            Write-Verbose "Forcing rebuild: cleaning project..."
            dotnet clean $csproj.FullName --configuration Release @msbuildProps
            if ($LASTEXITCODE -ne 0) {
                throw "dotnet clean failed with exit code $LASTEXITCODE"
            }
        }

        dotnet build $csproj.FullName @buildArgs
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE"
        }

        dotnet pack $csproj.FullName --configuration Release --no-build --output $nugetPath /p:ContinuousIntegrationBuild=true @msbuildProps
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet pack failed with exit code $LASTEXITCODE"
        }
    } else {
        $spinnerState = 0
        $i = 0
        $count = 3
        if ($force) {
            $count++
        }

        $i++;
        Show-Loading -Message "(${i}/${count}) Restoring..." -Action {
            param($project, $rArgs, $props)
            dotnet restore $project @rArgs @props
            if ($LASTEXITCODE -ne 0) {
                throw "dotnet restore failed with exit code $LASTEXITCODE"
            }
        } -SpinnerState ([ref] $spinnerState) -ArgumentList @($csproj.FullName, $restoreArgs, $msbuildProps) | Out-Null

        if ($force) {
            $i++;
            Show-Loading -Message "(${i}/${count}) Cleaning..." -Action {
                param($project, $props)
                dotnet clean $project --configuration Release @props
                if ($LASTEXITCODE -ne 0) {
                    throw "dotnet clean failed with exit code $LASTEXITCODE"
                }
            } -SpinnerState ([ref] $spinnerState) -ArgumentList @($csproj.FullName, $msbuildProps) | Out-Null
        }

        $i++;
        Show-Loading -Message "(${i}/${count}) Building..." -Action {
            param($project, $bArgs)
            dotnet build $project @bArgs
            if ($LASTEXITCODE -ne 0) {
                throw "dotnet build failed with exit code $LASTEXITCODE"
            }
        } -SpinnerState ([ref] $spinnerState) -ArgumentList @($csproj.FullName, $buildArgs) | Out-Null

        $i++;
        Show-Loading -Message "(${i}/${count}) Packing..." -Action {
            param($project, $outputPath, $props)
            dotnet pack $project --configuration Release --no-build --output $outputPath /p:ContinuousIntegrationBuild=true @props
            if ($LASTEXITCODE -ne 0) {
                throw "dotnet pack failed with exit code $LASTEXITCODE"
            }
        } -SpinnerState ([ref] $spinnerState) -ArgumentList @($csproj.FullName, $nugetPath, $msbuildProps) | Out-Null
    }
}


function Reset-Solution {
    <#
    .SYNOPSIS
        Cleans the solution by running `dotnet clean`.
        
    .DESCRIPTION
        This function runs `dotnet clean` on the solution file to clean all
        projects. It ensures that the environment
        is reset before building or setting up the projects.
        
    .PARAMETER solutionPath
        The path to the solution file (.sln) to clean.
        
    .INPUTS
        [string] solutionPath
        The path to the solution file (.sln) to clean.
        
    .EXAMPLE
        PS C:\> Clean-Solution -solutionPath "C:\Path\To\Solution.sln"
        Cleans the solution at "C:\Path\To\Solution.sln" by running `dotnet clean`.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string] $solutionPath
    )

    # Run clean on the solution file
    if ($PSBoundParameters["Verbose"] -or $VerbosePreference -eq "Continue") {
        dotnet clean $solutionPath --configuration Debug
        dotnet clean $solutionPath --configuration Release
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet clean failed with exit code $LASTEXITCODE"
        }
    } else {
        $spinnerState = 0
        Show-Loading -Message "Cleaning..." -Action {
            param($solution)
            dotnet clean $solution --configuration Debug
            dotnet clean $solution --configuration Release
            if ($LASTEXITCODE -ne 0) {
                throw "dotnet clean failed with exit code $LASTEXITCODE"
            }
        } -SpinnerState ([ref] $spinnerState) -ArgumentList @($solutionPath) | Out-Null
    }
}

Export-ModuleMember -Function Show-Loading, Build-Project, Reset-Solution