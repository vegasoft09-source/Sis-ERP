$target = "c:\Users\USER\source\repos\Sis-ERP\Hevelab2026"
$area = "$target\Areas\Contabilidad"

# Move Controllers
Get-ChildItem -Path "$area\Controllers" -Filter *.cs | ForEach-Object {
    Move-Item $_.FullName -Destination "$target\Controllers" -Force
}

# Move Models
Get-ChildItem -Path "$area\Models" -Filter *.cs | ForEach-Object {
    Move-Item $_.FullName -Destination "$target\Models" -Force
}

# Move Views folders
$viewFolders = @("Documentos", "Facturas", "LibroMayor", "Sire")
foreach ($folder in $viewFolders) {
    if (Test-Path "$area\Views\$folder") {
        Move-Item "$area\Views\$folder" -Destination "$target\Views" -Force
        # Copy _ViewStart.cshtml to each folder so they keep their layout
        Copy-Item "$area\Views\_ViewStart.cshtml" -Destination "$target\Views\$folder\_ViewStart.cshtml" -Force
    }
}

# Move Shared Views
Get-ChildItem -Path "$area\Views\Shared" -File | ForEach-Object {
    Move-Item $_.FullName -Destination "$target\Views\Shared" -Force
}

# Delete Areas folder
Remove-Item -Path "$target\Areas" -Recurse -Force
