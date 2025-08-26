<#
#####################################
 This file is public domain.
 It does not fall under the same licensing as the rest of the project.
 Feel free to use it in your project.

 - FireController#1847
#####################################
#>

$OutputFile = "DumpStructure.txt"
if (-not (Test-Path $OutputFile)) {
    New-Item -Path $OutputFile -ItemType File | Out-Null
}
$OutputFile = (Resolve-Path $OutputFile).Path
$Count = -1
$AnsiPattern = "`e\[[\d;]*m"

# Clear file and define write-line
Clear-Content -Path $OutputFile -ErrorAction SilentlyContinue
function Write-Line {
    param ($line)
    Write-Host $line
    $Plain = $line -replace $AnsiPattern, ""
    Add-Content -Path $OutputFile -Value $Plain
}

# Recursively search for the first .sln file from the current directory
$SlnPath = (Get-ChildItem -Path (Get-Location) -Filter *.sln -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1).FullName
if (($null -eq $SlnPath) -or (-not (Test-Path $SlnPath))) {
    Write-Host "No solution (.sln) file found in the current directory or subdirectories."
    exit 1
}
$oldCwd = Get-Location
Set-Location -Path (Split-Path -Path $SlnPath -Parent) | Out-Null
Write-Host "Using solution file: `e[38;2;0;200;0m$SlnPath`e[0m"
Write-Host "Current directory: `e[38;2;0;200;0m$(Get-Location)`e[0m"

try {
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
} finally {
    Set-Location -Path $oldCwd | Out-Null
}