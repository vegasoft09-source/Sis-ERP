$target = "c:\Users\USER\source\repos\Sis-ERP\Hevelab2026"

function Replace-In-Files($path, $filter, $old, $new) {
    Get-ChildItem -Path $path -Filter $filter -Recurse | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        if ($content.Contains($old)) {
            $content = $content.Replace($old, $new)
            Set-Content -Path $_.FullName -Value $content -NoNewline
        }
    }
}

# Controllers and Models namespace updates
Replace-In-Files "$target\Controllers" "*.cs" "Hevelab2026.Areas.Contabilidad.Models" "Hevelab2026.Models"
Replace-In-Files "$target\Controllers" "*.cs" "Hevelab2026.Areas.Contabilidad.Controllers" "Hevelab2026.Controllers"
Replace-In-Files "$target\Models" "*.cs" "Hevelab2026.Areas.Contabilidad.Models" "Hevelab2026.Models"

# Remove Area attribute
Get-ChildItem -Path "$target\Controllers" -Filter "*.cs" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace '(?m)^\s*\[Area\("Contabilidad"\)\]\s*\r?\n', ''
    Set-Content -Path $_.FullName -Value $content -NoNewline
}

# Update views using models
Replace-In-Files "$target\Views" "*.cshtml" "Hevelab2026.Areas.Contabilidad.Models" "Hevelab2026.Models"
