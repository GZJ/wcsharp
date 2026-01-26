param(
    [string]$OutputDir = (Join-Path $PSScriptRoot 'bin')
)

$null = New-Item -ItemType Directory -Path $OutputDir -Force

$csFiles = Get-ChildItem -Path . -Filter wcs*.cs
foreach ($f in $csFiles) {
    Write-Host "Compiling $($f.Name)..."
    $exeName = "{0}.exe" -f [System.IO.Path]::GetFileNameWithoutExtension($f.Name)
    $outputPath = Join-Path $OutputDir $exeName
    
    # Check if the file requires System.Windows.Forms
    $content = Get-Content $f.FullName -Raw
    if ($content -match 'System\.Windows\.Forms') {
        csc /out:$outputPath /r:System.Windows.Forms.dll /r:System.Drawing.dll $f.FullName
    } else {
        csc /out:$outputPath $f.FullName
    }
}

Write-Host "`nBuild completed. Executables are in: $OutputDir" -ForegroundColor Green
