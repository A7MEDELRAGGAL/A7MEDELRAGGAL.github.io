param(
    [string]$DllPath = "D:\SteamLibrary\steamapps\common\Muck\Muck_Data\Managed\Assembly-CSharp.dll",
    [string]$OutPath = "C:\Users\ahmed\OneDrive\Documents\Default Project\REA\muck-strings.txt"
)
$ErrorActionPreference = "Stop"
$bytes = [System.IO.File]::ReadAllBytes($DllPath)
function R([int]$o,[int]$n){ $b=New-Object byte[] $n; [Array]::Copy($bytes,$o,$b,0,$n); $b }
function RU32([int]$o){ [BitConverter]::ToUInt32($bytes,$o) }
function RU16([int]$o){ [BitConverter]::ToUInt16($bytes,$o) }
function RU64([int]$o){ [BitConverter]::ToUInt64($bytes,$o) }
$peOff=[int]$(RU32 0x3C)
$machine=RU16($peOff+4)
$numSec=RU16($peOff+6)
$optSize=RU16($peOff+20)
$magic=RU16($peOff+24)
if($magic -eq 0x20b){ $ddOff=$peOff+24+112; $ddCount=16 } else { $ddOff=$peOff+24+96; $ddCount=16 }
$cliRVA=RU32($ddOff+14*8); $cliSize=RU32($ddOff+14*8+4)
$secOff=$peOff+24+$optSize
$secs=@()
for($i=0;$i -lt $numSec;$i++){
    $so=$secOff+$i*40
    $name=[System.Text.Encoding]::ASCII.GetString($bytes,$so,8).TrimEnd([char]0)
    $secs+=@{ VA=RU32($so+12); VS=RU32($so+8); RawPtr=RU32($so+20); RawSize=RU32($so+16) }
}
function RvaOff([uint32]$rva){
    foreach($s in $secs){
        $va=[uint32]$s.VA; $vs=[uint32]$s.VS
        if($rva -ge $va -and $rva -lt ($va+$vs)){
            return [int]($s.RawPtr + ($rva - $va))
        }
    }
    return -1
}
$cliOff=RvaOff $cliRVA
$metaRVA=RU32($cliOff+8); $metaOff=RvaOff $metaRVA
$verLen=RU32($metaOff+12)
$ver=[System.Text.Encoding]::Unicode.GetString($bytes,$metaOff+16,$verLen).TrimEnd([char]0)
$flags=RU16($metaOff+$verLen+16)
$streams=RU16($metaOff+$verLen+18)
$streamBase=$metaOff+$verLen+20
$streamList=@()
$p=$streamBase
for($i=0;$i -lt $streams;$i++){
    $off=RU32($p); $sz=RU32($p+4)
    $nameStart=$p+8; $nameEnd=$nameStart
    while($bytes[$nameEnd] -ne 0){ $nameEnd++ }
    $nm=[System.Text.Encoding]::ASCII.GetString($bytes,$nameStart,$nameEnd-$nameStart)
    $p=$nameEnd+1; $p=($p+3) -band (-bnot 3)
    $streamList+=@{ Name=$nm; Offset=$off; Size=$sz }
}
function GetStream([string]$n){
    foreach($s in $streamList){ if($s.Name -eq $n){ return $s } }
    return $null
}
$stringsS=GetStream "#Strings"; $usS=GetStream "#US"; $blobS=GetStream "#Blob"
$tablesS=GetStream "#~"
$tablesOff=$metaOff+$tablesS.Offset
$heapSizes=$bytes[$tablesOff+6]
$valid=RU64($tablesOff+8)
$sortedBits=RU64($tablesOff+16)
$tablePtr=$tablesOff+24
$rowCounts=@{}
for($t=0;$t -lt 64;$t++){
    if($valid -band ([uint64]1 -shl $t)){
        $rowCounts[$t]=RU32($tablePtr); $tablePtr+=4
    }
}
$strHeapOff=$metaOff+$stringsS.Offset
$usHeapOff=$metaOff+$usS.Offset
function ReadStr([uint32]$idx){
    if($idx -eq 0){ return "" }
    $p=$strHeapOff+$idx; $e=$p
    while($bytes[$e] -ne 0){ $e++ }
    return [System.Text.Encoding]::UTF8.GetString($bytes,$p,$e-$p)
}
$out=New-Object System.Text.StringBuilder
[void]$out.AppendLine("=== Muck Assembly-CSharp.dll Metadata ===")
[void]$out.AppendLine("File: $DllPath")
[void]$out.AppendLine("Size: $($bytes.Length) bytes")
[void]$out.AppendLine("CLI version: $ver")
[void]$out.AppendLine("Streams: $(($streamList|%{$_.Name}) -join ', ')")
[void]$out.AppendLine("Valid tables bitmask: 0x$($valid.ToString('X16'))")
[void]$out.AppendLine("")
[void]$out.AppendLine("--- Table row counts (non-zero) ---")
foreach($k in ($rowCounts.Keys|Sort-Object)){
    [void]$out.AppendLine("  Table $k : $($rowCounts[$k]) rows")
}
[void]$out.AppendLine("")
[void]$out.AppendLine("=== #Strings heap (unique, non-empty) ===")
$strSet=New-Object System.Collections.Generic.HashSet[string]
$strEnd=$strHeapOff+$stringsS.Size
$sp=$strHeapOff+1
while($sp -lt $strEnd){
    $e=$sp; while($e -lt $strEnd -and $bytes[$e] -ne 0){$e++}
    if($e -gt $sp){
        $s=[System.Text.Encoding]::UTF8.GetString($bytes,$sp,$e-$sp)
        [void]$strSet.Add($s)
    }
    $sp=$e+1
}
$sorted=$strSet|Sort-Object
[void]$out.AppendLine("Total unique: $($sorted.Count)")
foreach($s in $sorted){ [void]$out.AppendLine("  $s") }
[void]$out.AppendLine("")
[void]$out.AppendLine("=== #US heap (user strings) ===")
if($usS){
    $usEnd=$usHeapOff+$usS.Size
    $up=$usHeapOff+1
    $usCount=0
    while($up -lt $usEnd -and $usCount -lt 500){
        $start=$up; $len=0; $shift=0
        while($up -lt $usEnd){
            $b=$bytes[$up]; $up++
            $len=($len -bor (($b -band 0x7F) -shl $shift)); $shift+=7
            if(($b -band 0x80) -eq 0){break}
        }
        if($len -gt 0 -and ($up+$len) -le $usEnd){
            try{
                $s=[System.Text.Encoding]::Unicode.GetString($bytes,$up,$len-1).TrimEnd([char]0)
                if(-not [string]::IsNullOrWhiteSpace($s)){
                    [void]$out.AppendLine("  [$usCount] $s"); $usCount++
                }
            }catch{}
        }
        $up+=$len
    }
    [void]$out.AppendLine("Total US strings printed: $usCount")
}else{
    [void]$out.AppendLine("  (no #US stream)")
}
[System.IO.File]::WriteAllText($OutPath,$out.ToString(),(New-Object System.Text.UTF8Encoding $true))
Write-Host "DONE. Wrote $($out.Length) chars to $OutPath"
Write-Host "Strings heap: $($sorted.Count) unique entries"
