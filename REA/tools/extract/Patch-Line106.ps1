param(
    [string]$ScriptPath = "C:\Users\ahmed\OneDrive\Documents\Default Project\REA\tools\extract\Extract-MuckStrings.ps1"
)

$lines = [System.IO.File]::ReadAllLines($ScriptPath, [System.Text.Encoding]::UTF8)

# Line 106 (index 105) is missing the closing parenthesis
$old = '=== #US heap (user strings) ==='
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match [regex]::Escape($old) -and $lines[$i] -notmatch '\)\)$') {
        # Fix: append the missing closing paren on the AppendLine call
        # Pattern in file: [void]$out.AppendLine("=== #US heap (user strings) ==="
        # Target:          [void]$out.AppendLine("=== #US heap (user strings) ===")
        if ($lines[$i] -match 'AppendLine\("=== #US heap \(user strings\) ==="$') {
            $lines[$i] = $lines[$i] + ')'
            Write-Host "Fixed line $($i+1): $($lines[$i])"
        }
    }
}

[System.IO.File]::WriteAllLines($ScriptPath, $lines, (New-Object System.Text.UTF8Encoding $true))
Write-Host "Patch complete."
