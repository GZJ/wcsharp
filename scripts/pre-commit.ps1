$csFiles = Get-ChildItem -Path . -Filter *.cs
foreach ($f in $csFiles) {
    Write-Host "Formatting $($f.Name)..."
    dotnet csharpier $f.Name
}
