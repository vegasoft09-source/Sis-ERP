Set-Location $PSScriptRoot
& "$PSScriptRoot\stop.ps1"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet run @args
