# Quick Test SmartFactory API

Write-Host "Testing SmartFactory API..." -ForegroundColor Cyan

# 1. Register User
Write-Host "`n1. Registering user..." -ForegroundColor Yellow
$registerJson = '{"email":"test@smartfactory.com","fullName":"Test User","password":"Test123!","phoneNumber":"0123456789"}'

try {
    $registerResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" -Method Post -Body $registerJson -ContentType "application/json"
    Write-Host "Success! Token:" $registerResponse.token.Substring(0, 50) "..." -ForegroundColor Green
    $token = $registerResponse.token
}
catch {
    Write-Host "User already exists, trying login..." -ForegroundColor Yellow
    $loginJson = '{"email":"test@smartfactory.com","password":"Test123!"}'
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $loginJson -ContentType "application/json"
    Write-Host "Logged in! Token:" $loginResponse.token.Substring(0, 50) "..." -ForegroundColor Green
    $token = $loginResponse.token
}

# 2. Create Product
Write-Host "`n2. Creating product..." -ForegroundColor Yellow
$productJson = '{"name":"Test Product","description":"This is a test product","price":99.99,"stockQuantity":100}'
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $productResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/products" -Method Post -Body $productJson -Headers $headers
    Write-Host "Product created successfully!" -ForegroundColor Green
    Write-Host ($productResponse | ConvertTo-Json)
    $productId = $productResponse.id
}
catch {
    Write-Host "Failed to create product: $_" -ForegroundColor Red
    exit 1
}

# 3. Get All Products
Write-Host "`n3. Getting all products..." -ForegroundColor Yellow
try {
    $allProducts = Invoke-RestMethod -Uri "http://localhost:5000/api/products" -Method Get -Headers @{"Authorization" = "Bearer $token"}
    Write-Host "Found $($allProducts.Count) products" -ForegroundColor Green
    Write-Host ($allProducts | ConvertTo-Json)
}
catch {
    Write-Host "Failed to get products: $_" -ForegroundColor Red
}

# 4. Get Product by ID
Write-Host "`n4. Getting product by ID: $productId..." -ForegroundColor Yellow
try {
    $product = Invoke-RestMethod -Uri "http://localhost:5000/api/products/$productId" -Method Get -Headers @{"Authorization" = "Bearer $token"}
    Write-Host "Product details:" -ForegroundColor Green
    Write-Host ($product | ConvertTo-Json)
}
catch {
    Write-Host "Failed to get product: $_" -ForegroundColor Red
}

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Test completed!" -ForegroundColor Green
Write-Host "Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

