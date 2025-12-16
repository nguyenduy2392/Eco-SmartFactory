# Test SmartFactory API

$baseUrl = "http://localhost:5000"

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Testing SmartFactory API" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Health Check
Write-Host "1. Testing Health Check..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/health" -Method Get
    Write-Host "✓ Health Check OK" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json) -ForegroundColor Gray
} catch {
    Write-Host "✗ Health Check Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 2: Register User
Write-Host "2. Registering new user..." -ForegroundColor Yellow
$registerData = @{
    email = "test@smartfactory.com"
    fullName = "Test User"
    password = "Test123!"
    phoneNumber = "0123456789"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Body $registerData -ContentType "application/json"
    Write-Host "✓ User registered successfully" -ForegroundColor Green
    Write-Host "Token: $($registerResponse.token.Substring(0, 50))..." -ForegroundColor Gray
    $token = $registerResponse.token
} catch {
    Write-Host "✗ Registration Failed (User may already exist): $_" -ForegroundColor Yellow
    
    # Try to login instead
    Write-Host "Trying to login with existing user..." -ForegroundColor Yellow
    $loginData = @{
        email = "test@smartfactory.com"
        password = "Test123!"
    } | ConvertTo-Json
    
    try {
        $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginData -ContentType "application/json"
        Write-Host "✓ Login successful" -ForegroundColor Green
        $token = $loginResponse.token
    } catch {
        Write-Host "✗ Login Failed: $_" -ForegroundColor Red
        exit 1
    }
}
Write-Host ""

# Test 3: Create Product
Write-Host "3. Creating new product..." -ForegroundColor Yellow
$productData = @{
    name = "Test Product"
    description = "This is a test product created via API"
    price = 99.99
    stockQuantity = 100
} | ConvertTo-Json

$headers = @{
    Authorization = "Bearer $token"
}

try {
    $createResponse = Invoke-RestMethod -Uri "$baseUrl/api/products" -Method Post -Body $productData -ContentType "application/json" -Headers $headers
    Write-Host "✓ Product created successfully" -ForegroundColor Green
    Write-Host ($createResponse | ConvertTo-Json) -ForegroundColor Gray
    $productId = $createResponse.id
} catch {
    Write-Host "✗ Product creation Failed: $_" -ForegroundColor Red
    $productId = $null
}
Write-Host ""

# Test 4: Get All Products
Write-Host "4. Getting all products..." -ForegroundColor Yellow
try {
    $allProducts = Invoke-RestMethod -Uri "$baseUrl/api/products" -Method Get -Headers $headers
    Write-Host "✓ Retrieved $($allProducts.Count) products" -ForegroundColor Green
    Write-Host ($allProducts | ConvertTo-Json) -ForegroundColor Gray
} catch {
    Write-Host "✗ Get All Products Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 5: Get Product by ID
if ($productId) {
    Write-Host "5. Getting product by ID ($productId)..." -ForegroundColor Yellow
    try {
        $product = Invoke-RestMethod -Uri "$baseUrl/api/products/$productId" -Method Get -Headers $headers
        Write-Host "✓ Retrieved product successfully" -ForegroundColor Green
        Write-Host ($product | ConvertTo-Json) -ForegroundColor Gray
    } catch {
        Write-Host "✗ Get Product by ID Failed: $_" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Testing Complete!" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Swagger UI: $baseUrl/swagger" -ForegroundColor Green
Write-Host "Hangfire Dashboard: $baseUrl/hangfire" -ForegroundColor Green

