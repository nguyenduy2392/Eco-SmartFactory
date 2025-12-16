# SmartFactory API Testing Guide

API đang chạy tại: **http://localhost:5000**

## 1. Health Check (No Auth Required)

```bash
curl http://localhost:5000/api/health
```

## 2. Register User

```bash
curl -X POST http://localhost:5000/api/auth/register `
  -H "Content-Type: application/json" `
  -d '{
    "email": "test@smartfactory.com",
    "fullName": "Test User",
    "password": "Test123!",
    "phoneNumber": "0123456789"
  }'
```

Response sẽ trả về token JWT.

## 3. Login User

```bash
curl -X POST http://localhost:5000/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{
    "email": "test@smartfactory.com",
    "password": "Test123!"
  }'
```

## 4. Create Product (Requires Auth)

**Lưu token từ bước login**, sau đó:

```bash
curl -X POST http://localhost:5000/api/products `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer YOUR_TOKEN_HERE" `
  -d '{
    "name": "Test Product",
    "description": "This is a test product",
    "price": 99.99,
    "stockQuantity": 100
  }'
```

## 5. Get All Products (Requires Auth)

```bash
curl -X GET http://localhost:5000/api/products `
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## 6. Get Product by ID (Requires Auth)

```bash
curl -X GET http://localhost:5000/api/products/1 `
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Swagger UI

Mở browser và truy cập:
- **http://localhost:5000/swagger**

Tại đây bạn có thể:
1. Test tất cả API endpoints
2. Xem documentation
3. Authorize bằng JWT token (click nút "Authorize" ở góc trên bên phải)

## Hangfire Dashboard

Mở browser và truy cập:
- **http://localhost:5000/hangfire**

Monitor background jobs và scheduled tasks.

## Test Database

Kết nối vào SQL Server và kiểm tra:

```sql
USE SmartFactory;

-- Xem users
SELECT * FROM Users;

-- Xem products
SELECT * FROM Products;
```

## Quick Test với PowerShell

```powershell
# 1. Register
$register = @{
    email = "test@smartfactory.com"
    fullName = "Test User"
    password = "Test123!"
    phoneNumber = "0123456789"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" -Method Post -Body $register -ContentType "application/json"
$token = $response.token

# 2. Create Product
$product = @{
    name = "Test Product"
    description = "Test Description"
    price = 99.99
    stockQuantity = 100
} | ConvertTo-Json

$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "http://localhost:5000/api/products" -Method Post -Body $product -ContentType "application/json" -Headers $headers

# 3. Get All Products
Invoke-RestMethod -Uri "http://localhost:5000/api/products" -Method Get -Headers $headers
```

## Stopping the API

Trong terminal đang chạy API, nhấn `Ctrl + C` để dừng.

