
try {
    $dllPath = "c:\Users\USER\Desktop\Sis-ERP-Sis-Erp2\Hevelab2026\bin\Debug\net8.0\MySqlConnector.dll"
    Add-Type -Path $dllPath

    $appSettings = Get-Content -Raw -Path "c:\Users\USER\Desktop\Sis-ERP-Sis-Erp2\Hevelab2026\appsettings.json" | ConvertFrom-Json
    $connStr = $appSettings.ConnectionStrings.SisErp
    if (!$connStr) {
        $connStr = $appSettings.ConnectionStrings.DefaultConnection
    }
    
    Write-Output "Connecting to database: $connStr"
    $conn = New-Object MySqlConnector.MySqlConnection($connStr)
    $conn.Open()

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SHOW TABLES;"
    $reader = $cmd.ExecuteReader()
    
    $tables = @()
    while ($reader.Read()) {
        $tables += $reader.GetString(0)
    }
    $reader.Close()

    Write-Output "Found tables: $($tables -join ', ')"

    $report = @()
    $report += "=== DATABASE SCHEMA REPORT ==="
    $report += "Connection String: $connStr"
    $report += ""

    foreach ($table in $tables) {
        $report += "Table: $table"
        $report += "--------------------------------------"
        
        $cmd.CommandText = "DESCRIBE `$table`;"
        $r = $cmd.ExecuteReader()
        while ($r.Read()) {
            $field = $r.GetString(0)
            $type = $r.GetString(1)
            $nullVal = $r.GetString(2)
            $key = $r.GetString(3)
            $defaultVal = if ($r.IsDBNull(4)) { "NULL" } else { $r.GetValue(4).ToString() }
            $extra = $r.GetString(5)
            $report += "  $field - $type - Null:$nullVal - Key:$key - Default:$defaultVal - Extra:$extra"
        }
        $r.Close()
        
        # Check count of rows
        $cmd.CommandText = "SELECT COUNT(*) FROM `$table`;"
        $count = $cmd.ExecuteScalar()
        $report += "  Total Rows: $count"
        $report += ""
    }

    $report | Out-File -FilePath "c:\Users\USER\Desktop\Sis-ERP-Sis-Erp2\db_schema.txt" -Encoding utf8
    Write-Output "Schema report written to db_schema.txt successfully!"
    
    $conn.Close()
} catch {
    Write-Error $_
}
