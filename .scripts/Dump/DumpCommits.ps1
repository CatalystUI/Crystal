<#
#####################################
 This file is public domain.
 It does not fall under the same licensing as the rest of the project.
 Feel free to use it in your project.

 - FireController#1847
#####################################
#>

$OutputFile = "DumpCommits.txt"
if (-not (Test-Path $OutputFile)) {
    New-Item -Path $OutputFile -ItemType File | Out-Null
}
$OutputFile = (Resolve-Path $OutputFile).Path
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

# Search commits
foreach ($Commit in $Commits) {
    $Info = git show -s --format="%cd%n%s%n%b" --date=iso-local $Commit
    $Lines = $Info -split "`n"
    $Date = $Lines[0].Trim()
    $Title = $Lines[1].Trim()
    $Body = @()
    if ($Lines.Length -gt 2) {
        $Body = $Lines[2..($Lines.Length - 1)] |
                ForEach-Object -Begin { $i = 0 } -Process {
                    $line = $_
                    $isLast = ($i -eq ($Lines.Length - 3))  # Adjusted for slice
                    $i++

                    if (($null -eq $line -or $line.Trim() -eq "") -and -not $isLast) {
                        "*"
                    } elseif ($null -eq $line -or $line.Trim() -eq "") {
                        return  # Skip trailing blank line
                    } else {
                        $trimmed = $line.TrimEnd()

                        # Respect lines that start with optional whitespace followed by *
                        if ($trimmed -match "^\s*\*") {
                            $trimmed
                        } else {
                            "* $trimmed"
                        }
                    }
                }
    }

    # Parse and format the date
    $DateTime = [datetime]::Parse($Date)
    $Day = $DateTime.Day
    switch ($Day % 10) {
        1 { $Suffix = if ($Day -eq 11) { "th" } else { "st" } }
        2 { $Suffix = if ($Day -eq 12) { "th" } else { "nd" } }
        3 { $Suffix = if ($Day -eq 13) { "th" } else { "rd" } }
        default { $Suffix = "th" }
    }
    $Month = $DateTime.ToString("MMMM")
    $Year = $DateTime.Year
    $FormattedDateTime = $DateTime.ToString("h:mmm tt")
    $TimeZone = [System.TimeZoneInfo]::Local.StandardName
    if ($TimeZone -like "*Standard Time") {
        $TimeZone = $TimeZone[0] + "ST";
    }
    $FormattedDate = "$Month $Day$Suffix, $Year at $FormattedDateTime $TimeZone"

    # Prase file status
    $FileStatus = git show --name-status --format="" $Commit
    $Modified = 0
    $Added = 0
    $Removed = 0
    foreach ($Line in $FileStatus) {
        if ($Line[0] -eq "M") {
            $Modified++
        } elseif ($Line[0] -eq "A") {
            $Added++
        } elseif ($Line[0] -eq "D") {
            $Removed++
        }
    }

    # Output
    Write-Line "$FormattedDate"
    Write-Line "`e[38;2;255;165;0m$Modified modified`e[0m, `e[38;2;0;200;0m$Added added`e[0m, `e[38;2;220;50;47m$Removed removed`e[0m"
    Write-Line "`e[38;2;0;255;144m$Title`e[0m"
    if ($Body.Count -gt 0) {
        $Body | ForEach-Object { Write-Line "`e[38;2;192;192;192m$_`e[0m" }
    }
    Write-Line ""
}