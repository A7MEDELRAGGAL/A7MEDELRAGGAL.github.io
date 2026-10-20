param()
$ErrorActionPreference = "Stop"
$script = "C:\Users\ahmed\OneDrive\Documents\Default Project\REA\tools\extract\Extract-MuckStrings.ps1"
$enc = New-Object System.Text.UTF8Encoding $true
$lines = [System.IO.File]::ReadAllLines($script)
$old = '$peOff=[int]RU32(0x3C)'
$new = '$peOff=[int]$(RU32 0x3C)'
$fixed = $false
for ($i = 0; $i -lt $lines.Length; $i++) {
    if ($lines[$i] -eq $old) {
        $lines[$i] = $new
        $fixed = $true
        break
    }
}
if (-not $fixed) {
    $probe = $false
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -like "*peOff*RU32*0x3C*") {
            Write-Host ("LINE11_MATCH_FOUND at {0}: {1}" -f ($i + 1), $lines[$i])
            $probe = $true
        }
    }
    if (-not $probe) {
        Write-Host "LINE11_NOT_FOUND"
        exit 1
    }
}
[System.IO.File]::WriteAllLines($script, $lines, $enc)
Write-Host "LINE11_PATCH_DONE"
