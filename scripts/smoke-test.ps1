$ErrorActionPreference = 'Stop'
$baseUrl = 'http://localhost:5080/api'

Write-Host 'Checking InventoryHub API health...' -ForegroundColor Cyan
$health = Invoke-RestMethod -Method Get -Uri "$baseUrl/health"
if ($health.status -ne 'healthy') { throw 'Health check failed.' }
Write-Host 'Health check passed.' -ForegroundColor Green

$sku = "SMOKE-$([Guid]::NewGuid().ToString('N').Substring(0, 8).ToUpperInvariant())"
$createdId = $null
try {
    $body = @{
        name = 'Smoke Test Product'
        sku = $sku
        category = 'Testing'
        price = 12.34
        stockQuantity = 3
        supplier = 'InventoryHub Test'
    } | ConvertTo-Json

    $created = Invoke-RestMethod -Method Post -Uri "$baseUrl/products" -ContentType 'application/json' -Body $body
    $createdId = $created.productId
    if (-not $createdId) { throw 'Create did not return a productId.' }
    Write-Host "Create passed (ID $createdId)." -ForegroundColor Green

    $updateBody = @{
        name = 'Smoke Test Product Updated'
        sku = $sku
        category = 'Testing'
        price = 23.45
        stockQuantity = 6
        supplier = 'InventoryHub Test'
    } | ConvertTo-Json
    $updated = Invoke-RestMethod -Method Put -Uri "$baseUrl/products/$createdId" -ContentType 'application/json' -Body $updateBody
    if ($updated.price -ne 23.45) { throw 'Update returned an unexpected price.' }
    Write-Host 'Update passed.' -ForegroundColor Green

    Invoke-RestMethod -Method Delete -Uri "$baseUrl/products/$createdId" | Out-Null
    $createdId = $null
    Write-Host 'Delete passed.' -ForegroundColor Green
}
finally {
    if ($createdId) {
        try { Invoke-RestMethod -Method Delete -Uri "$baseUrl/products/$createdId" | Out-Null } catch { Write-Warning 'Cleanup delete failed; remove the smoke test product manually if needed.' }
    }
}

Write-Host 'InventoryHub API smoke test completed successfully.' -ForegroundColor Green
