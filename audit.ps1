param (
    [string]$Mask,
    [string]$Path = ".",
    [switch]$Summary,
    [switch]$Hidden,
    [switch]$Help
)

function Show-Help {
    @"
Usage: .\audit.ps1 [OPTIONS]

Recursively search and optionally print file contents for auditing or context extraction.

Examples:
  .\audit.ps1
      Process all files under the current directory

  .\audit.ps1 -Mask "*.lua"
      Process only .lua files

  .\audit.ps1 -Path "./src"
      Start searching in ./src

  .\audit.ps1 -Summary
      Show only file paths, not content

  .\audit.ps1 -Hidden
      Include hidden files (e.g. .env)

Options:
  -Mask         Glob pattern or multiple patterns like "*.ps1|*.txt"
  -Path         Directory to start searching (default: current directory)
  -Summary      Only print file paths, not file contents
  -Hidden       Include hidden files (those starting with '.')
  -Help         Show this help message
"@ | Out-Host
    exit
}

if ($Help) {
    Show-Help
}

# Split mask into an array of patterns (e.g., "*.pl|*.ps1")
$patterns = @()
if ($Mask) {
    $patterns = $Mask -split '\|'
}

# Normalize full path
$resolvedPath = Resolve-Path -Path $Path -ErrorAction Stop

Get-ChildItem -Path $resolvedPath -Recurse -File -Force:$Hidden | ForEach-Object {
    $file = $_

    # Skip files inside Debug or Release directories
    if ($file.FullName -match "\\(Debug|Release)\\") {
        return
    }

    # Skip hidden files unless explicitly included
    if (-not $Hidden -and ($file.Name -match '^\.' -or $file.Attributes -match "Hidden")) {
        return
    }

    # Apply file mask if specified
    if ($patterns.Count -gt 0) {
        $matched = $false
        foreach ($pattern in $patterns) {
            if ($file.Name -like $pattern) {
                $matched = $true
                break
            }
        }
        if (-not $matched) {
            return
        }
    }

    if ($Summary) {
        Write-Output $file.FullName
    } else {
        Write-Output "--- START $($file.FullName) ---"
        try {
            Get-Content -Path $file.FullName -Raw
        } catch {
            Write-Warning "Could not read $($file.FullName): $_"
        }
        Write-Output "--- END $($file.FullName) ---`n"
    }
}
