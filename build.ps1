$csFiles = Get-ChildItem -Path . -Filter wcs*.cs
foreach ($f in $csFiles) {
    Write-Host "Compiling $($f.Name)..."
    csc $f.Name
}
