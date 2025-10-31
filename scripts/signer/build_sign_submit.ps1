Param(
  [Parameter(Mandatory=$true)][string]$Body,
  [Parameter(Mandatory=$true)][string]$SKey,
  [Parameter(Mandatory=$false)][switch]$Submit
)

$submitApi = $env:SUBMIT_API_URL
if (-not $submitApi) { $submitApi = "http://localhost:8090" }

Write-Host "Signing placeholder; integrate offline signing here." -ForegroundColor Yellow
$tmp = New-Item -ItemType Directory -Path ([System.IO.Path]::GetTempPath()) -Name ([System.Guid]::NewGuid())
$cbor = Join-Path $tmp.FullName "signed.cbor"
[IO.File]::WriteAllBytes($cbor, [byte[]]@(129,130,88,32,0))

if ($Submit) {
  Invoke-WebRequest -Method Post -Uri "$submitApi/api/submit/tx" -ContentType "application/cbor" -InFile $cbor | Select-Object -ExpandProperty Content
} else {
  Write-Host "Signed CBOR at: $cbor"
}
