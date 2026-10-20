# Test-ParserSyntax.ps1
# Empirical parse + runtime tests for candidate PowerShell call syntaxes.
# ASCII only. Run: powershell -NoProfile -ExecutionPolicy Bypass -File Test-ParserSyntax.ps1
$ErrorActionPreference = 'Continue'

$defs = @'
function RU32 { param($addr) if ($addr -is [int] -or $addr -is [long]) { return 1000 + $addr } return "BAD_TYPE" }
function RU16 { param($addr) return 1600 + $addr }
function RU64 { param($addr) return 6400 + $addr }
function R    { param($n, $bytes) return "R:$n" }
function RvaOff { param([uint32]$rva) return 5000 + $rva }
'@

$candidates = [ordered]@{
  'cast_call_known_fail'  = '$peOff=[int]RU32(0x3C); $peOff'
  'cast_subexpr_space'    = '$peOff=[int]$(RU32 0x3C); $peOff'
  'cast_paren_space'      = '$peOff=[int](RU32 0x3C); $peOff'
  'cast_subexpr_call'     = '$peOff=[int]$(RU32(0x3C)); $peOff'
  'cast_space_no_paren'   = '$peOff=[int]RU32 0x3C; $peOff'
  'assign_call'           = '$so=4; $m=RU32($so+4); $m'
  'assign_call_semicolon' = '$p=8; $off=RU32($p); $sz=RU32($p+4); "$off/$sz"'
  'assign_call_hexarg'    = '$ddOff=200; $cliRVA=RU32($ddOff+14*8); $cliRVA'
  'hashval_call'          = '$so=12; $secs=@{ VA=RU32($so+12); VS=RU32($so+8); RawPtr=RU32($so+20); RawSize=RU32($so+16) }; $secs.VA'
  'index_assign_call'     = '$a=@{}; $t=1; $a[$t]=RU32(7); $a[$t]'
  'assign_space_arg'      = '$rva=9; $o=RvaOff $rva; $o'
  'assign_paren_arg'      = '$rva=9; $o=RvaOff($rva); $o'
  'condition_call'        = '$x=5; if (RU32($x) -gt 1000) { "cond-true" } else { "cond-false" }'
  'expr_operator_after'   = '$v=RU32(4)+1; $v'
  'uint32_cast_var'       = '$rva=[uint32]9; $rva'
  'nested_arg'            = '$x=3; $y=RvaOff (RU32 $x); $y'
  'writehost_interp'      = '$y=5; Write-Host "v=$(RU32 $y)"; "ok"'
}

foreach ($name in $candidates.Keys) {
  $snippet = $candidates[$name]
  $code = "$defs`n$snippet"
  try {
    $sb = [scriptblock]::Create($code)
  } catch {
    $msg = ($_.Exception.Message -replace "`r?`n", ' | ')
    Write-Output ("{0} :: PARSE_FAIL :: {1}" -f $name, $msg)
    continue
  }
  try {
    $result = & $sb
    $out = ($result | ForEach-Object { "$_" }) -join ','
    Write-Output ("{0} :: RUN_OK :: {1}" -f $name, $out)
  } catch {
    $msg = ($_.Exception.Message -replace "`r?`n", ' | ')
    Write-Output ("{0} :: RUN_FAIL :: {1}" -f $name, $msg)
  }
}
Write-Output "TEST_DONE"
