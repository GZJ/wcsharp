param(
    [string]$BinDir = (Join-Path $PSScriptRoot 'bin'),
    [string]$OutputZip = (Join-Path $PSScriptRoot 'wcsharp.zip')
)

if (-not (Test-Path $BinDir)) {
    Write-Error "Binary directory not found: $BinDir"
    Write-Host "Please run build.ps1 first to compile the executables."
    exit 1
}

$exeFiles = Get-ChildItem -Path $BinDir -Filter *.exe
if ($exeFiles.Count -eq 0) {
    Write-Error "No executables found in $BinDir"
    Write-Host "Please run build.ps1 first to compile the executables."
    exit 1
}

Write-Host "Packing $($exeFiles.Count) executable(s) from $BinDir..."

if (Test-Path $OutputZip) {
    Remove-Item $OutputZip -Force
    Write-Host "Removed existing $OutputZip"
}

Compress-Archive -Path (Join-Path $BinDir '*.exe') -DestinationPath $OutputZip -CompressionLevel Optimal

Write-Host "Successfully created: $OutputZip" -ForegroundColor Green
