param(
    [string]$Server = ".\SQLEXPRESS",
    [string]$Database = "AbuAmenPharmaDb",
    [string]$OutputPath = ".\sql\seed-data.sql"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Escape-SqlString {
    param([string]$Value)
    return $Value.Replace("'", "''")
}

function ConvertTo-SqlLiteral {
    param([object]$Value)

    if ($null -eq $Value -or $Value -is [System.DBNull]) {
        return "NULL"
    }

    if ($Value -is [string]) {
        return "N'" + (Escape-SqlString -Value $Value) + "'"
    }

    if ($Value -is [char]) {
        return "N'" + (Escape-SqlString -Value ([string]$Value)) + "'"
    }

    if ($Value -is [bool]) {
        return $(if ($Value) { "1" } else { "0" })
    }

    if ($Value -is [datetime]) {
        return "'" + $Value.ToString("yyyy-MM-dd HH:mm:ss.fffffff", [System.Globalization.CultureInfo]::InvariantCulture) + "'"
    }

    if ($Value -is [timespan]) {
        return "'" + $Value.ToString() + "'"
    }

    if ($Value -is [guid]) {
        return "'" + $Value.ToString() + "'"
    }

    if ($Value -is [byte[]]) {
        $hex = [System.BitConverter]::ToString($Value).Replace("-", "")
        return "0x$hex"
    }

    if ($Value -is [System.IFormattable]) {
        return $Value.ToString($null, [System.Globalization.CultureInfo]::InvariantCulture)
    }

    return "N'" + (Escape-SqlString -Value ($Value.ToString())) + "'"
}

function New-SqlCommand {
    param(
        [System.Data.SqlClient.SqlConnection]$Connection,
        [string]$Sql
    )

    $cmd = $Connection.CreateCommand()
    $cmd.CommandText = $Sql
    return $cmd
}

$tables = @(
    "Units",
    "Manufacturers",
    "Salesmen",
    "Customers",
    "Suppliers",
    "Items",
    "ItemBatches",
    "Purchases",
    "PurchaseLines",
    "PurchaseReturns",
    "PurchaseReturnLines",
    "Sales",
    "SaleLines",
    "SaleAllocations",
    "CustomerReceipts",
    "CustomerReceiptAllocations",
    "SaleReturns",
    "SaleReturnLines",
    "SaleReturnAllocations",
    "CustomerLedgers",
    "StockMovements"
)

$connString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"
$connection = New-Object System.Data.SqlClient.SqlConnection($connString)
$connection.Open()

try {
    $scriptLines = New-Object System.Collections.Generic.List[string]
    $scriptLines.Add("-- Generated $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') from [$Server].[$Database]")
    $scriptLines.Add("SET NOCOUNT ON;")
    $scriptLines.Add("SET XACT_ABORT ON;")
    $scriptLines.Add("BEGIN TRY")
    $scriptLines.Add("    BEGIN TRAN;")
    $scriptLines.Add("")

    foreach ($table in $tables) {
        $metadataCmd = New-SqlCommand -Connection $connection -Sql @"
SELECT
    c.name AS ColumnName,
    c.column_id AS ColumnId,
    c.is_identity AS IsIdentity
FROM sys.columns c
INNER JOIN sys.tables t ON t.object_id = c.object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = 'dbo'
  AND t.name = @TableName
  AND c.is_computed = 0
ORDER BY c.column_id;
"@
        [void]$metadataCmd.Parameters.AddWithValue("@TableName", $table)
        $metadataAdapter = New-Object System.Data.SqlClient.SqlDataAdapter($metadataCmd)
        $metadata = New-Object System.Data.DataTable
        [void]$metadataAdapter.Fill($metadata)

        if ($metadata.Rows.Count -eq 0) {
            continue
        }

        $columnNames = @($metadata.Rows | ForEach-Object { $_["ColumnName"].ToString() })
        $columnList = ($columnNames | ForEach-Object { "[{0}]" -f $_ }) -join ", "
        $hasIdentity = ($metadata.Select("IsIdentity = 1").Count -gt 0)
        $hasIdColumn = $columnNames -contains "Id"

        $pkCmd = New-SqlCommand -Connection $connection -Sql @"
SELECT c.name AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
INNER JOIN sys.tables t ON t.object_id = i.object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE i.is_primary_key = 1
  AND s.name = 'dbo'
  AND t.name = @TableName
ORDER BY ic.key_ordinal;
"@
        [void]$pkCmd.Parameters.AddWithValue("@TableName", $table)
        $pkAdapter = New-Object System.Data.SqlClient.SqlDataAdapter($pkCmd)
        $pkData = New-Object System.Data.DataTable
        [void]$pkAdapter.Fill($pkData)

        $orderBy = ""
        if ($pkData.Rows.Count -gt 0) {
            $pkColumns = @($pkData.Rows | ForEach-Object { "[{0}]" -f $_["ColumnName"].ToString() })
            $orderBy = " ORDER BY " + ($pkColumns -join ", ")
        }

        $dataSql = "SELECT $columnList FROM [dbo].[$table]$orderBy;"
        $dataCmd = New-SqlCommand -Connection $connection -Sql $dataSql
        $dataAdapter = New-Object System.Data.SqlClient.SqlDataAdapter($dataCmd)
        $dataTable = New-Object System.Data.DataTable
        [void]$dataAdapter.Fill($dataTable)

        $scriptLines.Add("-- [$table] rows: $($dataTable.Rows.Count)")
        if ($dataTable.Rows.Count -eq 0) {
            $scriptLines.Add("")
            continue
        }

        if ($hasIdentity) {
            $scriptLines.Add("SET IDENTITY_INSERT [dbo].[$table] ON;")
        }

        foreach ($row in $dataTable.Rows) {
            $values = @()
            foreach ($name in $columnNames) {
                $values += ConvertTo-SqlLiteral -Value $row[$name]
            }
            $valuesSql = $values -join ", "
            if ($hasIdColumn) {
                $idLiteral = ConvertTo-SqlLiteral -Value $row["Id"]
                $scriptLines.Add("IF NOT EXISTS (SELECT 1 FROM [dbo].[$table] WHERE [Id] = $idLiteral)")
                $scriptLines.Add("    INSERT INTO [dbo].[$table] ($columnList) VALUES ($valuesSql);")
            }
            else {
                $scriptLines.Add("INSERT INTO [dbo].[$table] ($columnList) VALUES ($valuesSql);")
            }
        }

        if ($hasIdentity) {
            $scriptLines.Add("SET IDENTITY_INSERT [dbo].[$table] OFF;")
        }
        $scriptLines.Add("")
    }

    $scriptLines.Add("    COMMIT;")
    $scriptLines.Add("END TRY")
    $scriptLines.Add("BEGIN CATCH")
    $scriptLines.Add("    IF @@TRANCOUNT > 0 ROLLBACK;")
    $scriptLines.Add("    THROW;")
    $scriptLines.Add("END CATCH;")

    $fullOutput = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $OutputPath))
    $outputDir = Split-Path -Parent $fullOutput
    if (-not (Test-Path -LiteralPath $outputDir)) {
        [void](New-Item -ItemType Directory -Path $outputDir -Force)
    }

    [System.IO.File]::WriteAllLines($fullOutput, $scriptLines, [System.Text.UTF8Encoding]::new($false))
    Write-Output "Seed SQL exported to: $fullOutput"
}
finally {
    $connection.Close()
}
