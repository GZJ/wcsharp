$csFiles = Get-ChildItem -Path . -Filter *.cs
foreach ($f in $csFiles) {
    Write-Host "Compiling $($f.Name)..."
    dotnet csharpier $f.Name
}
