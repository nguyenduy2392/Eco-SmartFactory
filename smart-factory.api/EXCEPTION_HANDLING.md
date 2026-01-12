# Global Exception Handling

## Overview
Hệ thống Global Exception Handler đã được tích hợp vào SmartFactory API để xử lý tất cả các exceptions một cách thống nhất và trả về response có cấu trúc rõ ràng.

## Architecture

### 1. Custom Exception Classes
Located in `SmartFactory.Application/Exceptions/`:

- **BaseException**: Base class cho tất cả custom exceptions
- **NotFoundException** (404): Resource không tồn tại
- **ValidationException** (400): Lỗi validation dữ liệu
- **BusinessException** (400): Vi phạm business rules
- **UnauthorizedException** (401): Chưa đăng nhập hoặc token không hợp lệ
- **ForbiddenException** (403): Không có quyền truy cập

### 2. Exception Handling Middleware
`SmartFactory.Api/Middleware/ExceptionHandlingMiddleware.cs`

Middleware này:
- Bắt tất cả exceptions trong request pipeline
- Chuyển đổi thành ErrorResponse chuẩn
- Log exceptions với Serilog
- Trả về HTTP status code phù hợp

### 3. Standard Error Response
```json
{
  "errorCode": "NOT_FOUND",
  "message": "Product with key 'ABC123' was not found.",
  "details": null,
  "validationErrors": null,
  "timestamp": "2026-01-12T02:35:00.000Z",
  "traceId": "0HMVKQFQ7N5CH:00000001"
}
```

## Usage Examples

### 1. Using NotFoundException
```csharp
var product = await _context.Products.FindAsync(id);
if (product == null)
{
    throw new NotFoundException("Product", id);
}
```

### 2. Using ValidationException
```csharp
var errors = new Dictionary<string, string[]>
{
    { "Email", new[] { "Email is required", "Email must be valid" } },
    { "Password", new[] { "Password must be at least 8 characters" } }
};
throw new ValidationException(errors);
```

### 3. Using BusinessException
```csharp
if (product.Orders.Any())
{
    throw new BusinessException(
        "Cannot delete product because it has active orders",
        "PRODUCT_HAS_ORDERS"
    );
}
```

### 4. Using UnauthorizedException
```csharp
var user = await _context.Users.FindAsync(userId);
if (user == null || !user.IsActive)
{
    throw new UnauthorizedException("Invalid credentials");
}
```

### 5. Using ForbiddenException
```csharp
if (!currentUser.HasPermission("DELETE_PRODUCT"))
{
    throw new ForbiddenException("You do not have permission to delete this resource");
}
```

## Built-in Exception Handling

Middleware cũng tự động xử lý các .NET built-in exceptions:

| Exception Type | HTTP Status | Error Code | Description |
|---------------|-------------|------------|-------------|
| UnauthorizedAccessException | 401 | UNAUTHORIZED | Unauthorized access |
| KeyNotFoundException | 404 | NOT_FOUND | Resource not found |
| ArgumentException | 400 | BAD_REQUEST | Invalid argument |
| ArgumentNullException | 400 | BAD_REQUEST | Null argument |
| InvalidOperationException | 409 | CONFLICT | Invalid operation |
| TimeoutException | 408 | TIMEOUT | Request timeout |
| Exception (other) | 500 | INTERNAL_SERVER_ERROR | Unexpected error |

## Testing

Test controller đã được tạo tại `SmartFactory.Api/Controllers/TestExceptionController.cs` với các endpoints:

- GET `/api/testexception/not-found` - Test NotFoundException
- GET `/api/testexception/validation-error` - Test ValidationException
- GET `/api/testexception/business-error` - Test BusinessException
- GET `/api/testexception/unauthorized` - Test UnauthorizedException
- GET `/api/testexception/forbidden` - Test ForbiddenException
- GET `/api/testexception/server-error` - Test unhandled exception
- GET `/api/testexception/argument-error` - Test ArgumentException
- GET `/api/testexception/invalid-operation` - Test InvalidOperationException

### Example Test:
```bash
curl http://localhost:5000/api/testexception/not-found
```

Response:
```json
{
  "errorCode": "NOT_FOUND",
  "message": "Product with key 'ABC123' was not found.",
  "details": null,
  "validationErrors": null,
  "timestamp": "2026-01-12T02:35:00.000Z",
  "traceId": "0HMVKQFQ7N5CH:00000001"
}
```

## Best Practices

### 1. Use Specific Exceptions
```csharp
// ✅ Good
throw new NotFoundException("Product", productId);

// ❌ Bad
throw new Exception("Product not found");
```

### 2. Provide Meaningful Error Codes
```csharp
// ✅ Good
throw new BusinessException(
    "Cannot process order because inventory is insufficient",
    "INSUFFICIENT_INVENTORY"
);

// ❌ Bad
throw new BusinessException("Error");
```

### 3. Include Validation Details
```csharp
// ✅ Good
var errors = new Dictionary<string, string[]>
{
    { "Email", new[] { "Email is required", "Must be valid format" } },
    { "Phone", new[] { "Phone number is invalid" } }
};
throw new ValidationException(errors);

// ❌ Bad
throw new ValidationException("Validation failed");
```

### 4. Log Before Throwing (when necessary)
```csharp
// For critical business errors
_logger.LogWarning("Attempting to delete product {ProductId} with active orders", productId);
throw new BusinessException("Cannot delete product with active orders");
```

## Configuration

Middleware được đăng ký trong `Program.cs`:

```csharp
// Must be registered FIRST in the pipeline
app.UseGlobalExceptionHandler();
```

## Development vs Production

- **Development**: Error details và stack trace được included trong response
- **Production**: Chỉ trả về error message, không bao gồm sensitive information

## Logging

Tất cả exceptions được log tự động với Serilog:
- Custom exceptions: Warning level
- Unhandled exceptions: Error level
- Server errors: Error level with full stack trace

Logs được lưu tại: `SmartFactory.Api/logs/log-{Date}.txt`

## Future Enhancements

- [ ] Add exception metrics/monitoring
- [ ] Implement retry logic for transient failures
- [ ] Add localization support for error messages
- [ ] Create exception filters for specific scenarios
- [ ] Add correlation IDs for distributed tracing
