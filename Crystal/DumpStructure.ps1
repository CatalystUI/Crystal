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

$OutputFile = "DumpStructure.txt"
$Count = -1
if ($Count -eq -1) {
    $Commits = git log --pretty=format:"%H"
} else {
    $Commits = git log -n $Count --pretty=format:"%H"
}
$AnsiPattern = "`e\[[\d;]*m"

# Clear file and define write-line
Clear-Content -Path $OutputFile -ErrorAction SilentlyContinue
function Write-Line {
    param ($line)
    Write-Host $line
    $Plain = $line -replace $AnsiPattern, ""
    Add-Content -Path $OutputFile -Value $Plain
}

$SlnPath = (Get-ChildItem -Filter *.sln | Select-Object -First 1).FullName
if (-not (Test-Path $SlnPath)) {
    Write-Host "No solution (.sln) file found in the current directory."
    exit 1
}

# Try to find an explicit solution name comment, else fallback to file name
$lines = Get-Content $SlnPath
$solutionName = $null
foreach ($line in $lines) {
    if ($line -match '^\s*#\s*Solution\s*:\s*(.+)\s*$') {
        $solutionName = $matches[1].Trim()
        break
    }
}
if (-not $solutionName) {
    $solutionName = [System.IO.Path]::GetFileNameWithoutExtension($SlnPath)
}

$lines = Get-Content $SlnPath
$projectInfo = @{}
$childrenByParent = @{}
$parentByChild = @{}

foreach ($line in $lines) {
    if ($line -match '^Project\("\{([A-F0-9\-]+)\}"\)\s=\s"(.+?)",\s*"(.+?)",\s*"\{([A-F0-9\-]+)\}"') {
        $typeGuid = $matches[1].ToUpper()
        $name     = $matches[2]
        $relPath  = $matches[3]
        $guid     = $matches[4].ToUpper()
        $isFolder = $typeGuid -eq '2150E333-8FDC-42A3-9474-1A3956D46DE8'
        $projectInfo[$guid] = @{
            Name     = $name
            Path     = $relPath
            TypeGuid = $typeGuid
            IsFolder = $isFolder
            Guid     = $guid
        }
    }
}

# Parse NestedProjects section to build parent->children mapping
$inSection = $false
foreach ($line in $lines) {
    if ($line -match '^\s*GlobalSection\(NestedProjects\)') { $inSection = $true; continue }
    if ($inSection) {
        if ($line -match '^\s*\{([A-F0-9\-]+)\}\s*=\s*\{([A-F0-9\-]+)\}') {
            $child = $matches[1].ToUpper()
            $parent = $matches[2].ToUpper()
            $parentByChild[$child] = $parent
            if (-not $childrenByParent.ContainsKey($parent)) {
                $childrenByParent[$parent] = @()
            }
            $childrenByParent[$parent] += $child
        } elseif ($line -match '^\s*EndGlobalSection') { $inSection = $false }
    }
}

# Find root items: those not nested under any parent
$rootGuids = $projectInfo.Keys | Where-Object { -not $parentByChild.ContainsKey($_) }

function PrintIndentedTree([string]$guid, [int]$level = 0) {
    $item = $projectInfo[$guid]
    $indent = " " * ($level * 4)
    if ($item.IsFolder) {
        Write-Line "`e[38;2;52;152;219m$indent$($item.Name)/`e[0m"
    } else {
        Write-Line "$indent$($item.Name) [`e[38;2;192;192;192m$($item.Path)`e[0m]"
    }

    if ($childrenByParent.ContainsKey($guid)) {
        foreach ($childGuid in $childrenByParent[$guid]) {
            PrintIndentedTree -guid $childGuid -level ($level + 1)
        }
    }
}

Write-Line ""
Write-Line "Solution Structure for `e[38;2;0;200;0m$solutionName`e[0m [`e[38;2;192;192;192m$SlnPath`e[0m]"
Write-Line "======================================="
Write-Line ""

foreach ($guid in $rootGuids) {
    PrintIndentedTree -guid $guid -level 0
}

Write-Line ""
Write-Line ""
Write-Line "Project Structures for `e[38;2;0;200;0m$solutionName`e[0m [`e[38;2;192;192;192m$SlnPath`e[0m]"
Write-Line "======================================="
Write-Line ""

# Exclude these directories (case-insensitive, anywhere in the path)
$excludeDirs = @('bin', 'obj', '.vs', '.git', 'lib')
$excludeExts = @('dll', 'exe', 'pdb', 'so', 'dylib', 'o', 'a', 'lib', 'log', 'snk', 'gz', 'csproj')

function Is-ExcludedDir {
    param($dir)
    foreach ($ex in $excludeDirs) {
        if ($dir -ieq $ex) { return $true }
    }
    return $false
}

function Is-ExcludedExtension {
    param($file)
    # Remove leading dot from extension (if any), and compare lowercased
    $ext = $file.Extension
    if ($ext) {
        $ext = $ext.TrimStart('.').ToLowerInvariant()
        return $excludeExts -contains $ext
    }
    return $false
}

function List-FilesIndented {
    param (
        [string]$baseDir,
        [int]$level = 1
    )
    $indent = " " * ($level * 4)
    # List files, skipping excluded extensions
    Get-ChildItem -LiteralPath $baseDir -File | Where-Object {
        -not (Is-ExcludedExtension $_)
    } | ForEach-Object {
        Write-Line "$indent$($_.Name)"
    }
    # List directories, skipping excluded ones
    Get-ChildItem -LiteralPath $baseDir -Directory | Where-Object {
        -not (Is-ExcludedDir $_.Name)
    } | ForEach-Object {
        Write-Line "`e[38;2;52;152;219m$indent$($_.Name)/`e[0m"
        List-FilesIndented -baseDir $_.FullName -level ($level + 1)
    }
}

foreach ($guid in $projectInfo.Keys) {
    $item = $projectInfo[$guid]
    if (-not $item.IsFolder) {
        $projDir = Split-Path -Parent $item.Path
        $projTitle = "`e[38;2;0;200;0m$($item.Name)`e[0m [`e[38;2;192;192;192m$($item.Path)`e[0m]"
        Write-Line "$projTitle"
        Write-Line "---------------------------------------"
        List-FilesIndented -baseDir $projDir -level 1
        Write-Line ""
    }
}