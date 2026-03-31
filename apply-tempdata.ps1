$files = Get-ChildItem -Path "Controllers" -Filter "*.cs"
foreach ($f in $files) {
    if ($f.Name -in @("SalesController.cs", "PurchasesController.cs", "CustomerReceiptsController.cs")) { continue }
    $lines = Get-Content $f.FullName
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match "^\s*return RedirectToAction\(") {
            $hasTempData = $false
            $hasSave = $false
            for ($k = [math]::max(0, $i-20); $k -lt $i; $k++) {
                if ($lines[$k] -match "TempData\[`"SuccessMessage`"\]") { $hasTempData = $true }
                if ($lines[$k] -match "SaveChangesAsync") { $hasSave = $true }
            }
            if ($hasSave -and -not $hasTempData) {
                $lines[$i] = $lines[$i] -replace "(^\s*)", "`$1TempData[`"SuccessMessage`"] = `"تمت العملية بنجاح`";`r`n`$1"
            }
        }
    }
    Set-Content -Path $f.FullName -Value ($lines -join "`r`n")
}
