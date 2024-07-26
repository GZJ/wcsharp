$ProjectRoot = Split-Path -Parent $PSScriptRoot
cp  $PSScriptRoot/pre-commit $ProjectRoot/.git/hooks
cp  $PSScriptRoot/pre-commit.ps1 $ProjectRoot/.git/hooks
