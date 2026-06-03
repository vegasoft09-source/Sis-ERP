# Libera el puerto 5222 (y procesos Hevelab2026) antes de dotnet run
$port = 5222
$conns = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
foreach ($c in $conns) {
    Stop-Process -Id $c.OwningProcess -Force -ErrorAction SilentlyContinue
}
Get-Process -Name "Hevelab2026" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1
if (Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue) {
    Write-Host "Advertencia: el puerto $port sigue ocupado. Cierra la otra terminal o reinicia."
    exit 1
}
Write-Host "Puerto $port libre. Ejecuta: dotnet run"
