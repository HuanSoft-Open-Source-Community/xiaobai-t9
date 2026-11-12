# 给自制程序逐个签名，然后再签安装包
# 用法：登录 SimplySign Desktop 后，在 PowerShell 里执行  .\sign_all.ps1
$ErrorActionPreference = 'Continue'

$thumb = '6FD3F71AC84A6CD47CF5959E1C38D18D39D789C5'
$cert  = Get-ChildItem "Cert:\CurrentUser\My\$thumb" -ErrorAction SilentlyContinue
if (-not $cert) { throw "找不到签名证书 $thumb" }

$targets = @(
    'C:\weasel\output\t9keyboard.exe',
    'C:\weasel\output\t9configui.exe',
    'C:\weasel\output\t9skin.exe',
    'C:\weasel\output\numkeyboard.exe',
    'C:\weasel\output\helpme.exe',
    'C:\weasel\output\update.exe',
    'C:\weasel\output\t9s2t\t9s2t.exe'
)

foreach ($f in $targets) {
    if (-not (Test-Path $f)) { Write-Host "跳过（不存在）: $f" -ForegroundColor DarkGray; continue }
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $r = Set-AuthenticodeSignature -FilePath $f -Certificate $cert -HashAlgorithm SHA256 `
            -TimestampServer 'http://time.certum.pl/' -ErrorAction SilentlyContinue
    $ok = ($r.Status -eq 'Valid')
    $line = "{0,-22} {1,-8} {2,5:N1}s" -f (Split-Path $f -Leaf), $r.Status, $sw.Elapsed.TotalSeconds
    if ($ok) { Write-Host $line -ForegroundColor Green } else { Write-Host $line -ForegroundColor Red; Write-Host ("  " + $r.StatusMessage) -ForegroundColor Red }
}

Write-Host ""
Write-Host "==== 复核 ===="
foreach ($f in $targets) {
    if (-not (Test-Path $f)) { continue }
    $s = Get-AuthenticodeSignature $f
    "{0,-22} {1}" -f (Split-Path $f -Leaf), $s.Status
}
