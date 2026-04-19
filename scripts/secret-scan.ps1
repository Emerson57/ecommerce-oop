<#
Simple secret scanner for CI/publish time.
Usage: .\scripts\secret-scan.ps1 [-Path .] [-Verbose]
Exits with code 1 if any suspicious secrets are found.
This is intentionally conservative: it flags likely secrets so the build can be stopped for manual review.
#>
param(
    [string]$Path = ".",
    [switch]$Verbose
)

Write-Host "Secret scan starting in path: $Path"

$excludeDirs = @('.git','bin','obj','node_modules','.vs')

$patterns = @(
    @{ Name = 'Jwt:SigningKey (JSON)'; Regex = '(?i)"Jwt"\s*:\s*\{[\s\S]{0,200}?"SigningKey"\s*:\s*"(.{8,})"' },
    @{ Name = 'Jwt:SigningKey (colon)'; Regex = '(?i)Jwt\s*:\s*SigningKey\s*[:=]\s*\"?(.{8,})\"?' },
    @{ Name = 'ConnectionStrings (JSON)'; Regex = '(?i)"ConnectionStrings"\s*:\s*\{[\s\S]{0,400}?"[A-Za-z0-9_-]+"\s*:\s*"(.{8,})"' },
    @{ Name = 'Connection string (inline)'; Regex = '(?i)Server\s*=.+;\s*Database\s*=.+;' },
    @{ Name = 'Password in-line'; Regex = '(?i)password\s*=\s*[^;\"\']+' },
    @{ Name = 'User Id in-line'; Regex = '(?i)user\s*id\s*=\s*[^;\"\']+' },
    @{ Name = 'Password JSON'; Regex = '(?i)"password"\s*:\s*"(.{1,})"' }
)

$findings = @()

Get-ChildItem -Path $Path -Recurse -File -Force | Where-Object {
    # exclude directories
    foreach ($d in $excludeDirs) {
        if ($_.FullName -match [regex]::Escape("/$d/") -or $_.FullName -match [regex]::Escape("\\$d\\")) { return $false }
    }
    # only relevant file types
    $ext = $_.Extension.ToLower()
    return $ext -in '.json','.config','.xml','.env','.txt','.yaml','.yml','.ps1','.sh','.cs','.ini'
} | ForEach-Object {
    $file = $_
    try {
        $text = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction Stop
    } catch {
        if ($Verbose) { Write-Warning "Could not read $($file.FullName): $_" }
        return
    }

    # filename-based checks
    if ($file.Name -like '*.local.json' -or $file.Name -like '*.secrets.json') {
        $findings += [pscustomobject]@{ File = $file.FullName; Pattern = 'Local secrets filename'; Match = $file.Name }
    }

    foreach ($p in $patterns) {
        $regex = $p.Regex
        $matches = [regex]::Matches($text, $regex)
        foreach ($m in $matches) {
            # Heuristic: ignore empty or obvious placeholder values
            $value = ''
            if ($m.Groups.Count -gt 1) { $value = $m.Groups[1].Value.Trim() }
            if ($value -and ($value -notmatch 'REPLACE|CHANGE_ME|your_value_here|<secret>|""' -and $value.Length -gt 3)) {
                $findings += [pscustomobject]@{ File = $file.FullName; Pattern = $p.Name; Match = $value }
            } elseif (-not $value) {
                # If regex didn't capture group, still report the line context
                $line = ($text -split "\r?\n" | Select-String -Pattern $regex -SimpleMatch | Select-Object -First 1).Line
                if ($line) { $findings += [pscustomobject]@{ File = $file.FullName; Pattern = $p.Name; Match = $line.Trim() } }
            }
        }
    }
}

if ($findings.Count -gt 0) {
    Write-Error "Potential secrets found: $($findings.Count) items. Failing the build."
    $grouped = $findings | Group-Object -Property File
    foreach ($g in $grouped) {
        Write-Host "\nFile: $($g.Name)"
        foreach ($i in $g.Group) {
            Write-Host " - Pattern: $($i.Pattern) => $($i.Match)"
        }
    }
    exit 1
} else {
    Write-Host "No likely secrets found."
    exit 0
}
