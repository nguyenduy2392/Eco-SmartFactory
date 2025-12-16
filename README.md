# SmartFactory Backend API

ASP.NET Core 6.0 backend API sử dụng Clean Architecture với CQRS pattern.

## Cấu trúc project

```
SmartFactory/
├── SmartFactory.sln
├── SmartFactory.Api/              # API Layer (Controllers, Program.cs)
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── BaseApiController.cs
│   │   ├── HealthController.cs
│   │   └── UsersController.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
└── SmartFactory.Application/      # Application Layer (Business Logic)
    ├── Behaviors/                 # MediatR Pipeline Behaviors
    │   ├── LoggingBehavior.cs
    │   ├── PerformanceBehavior.cs
    │   ├── UnhandledExceptionBehavior.cs
    │   └── ValidationBehavior.cs
    ├── Commands/                  # CQRS Commands (Write operations)
    │   └── Auth/
    │       ├── LoginCommand.cs
    │       └── RegisterCommand.cs
    ├── Data/                      # Database Context
    │   ├── ApplicationDbContext.cs
    │   └── ApplicationDbContextFactory.cs
    ├── DTOs/                      # Data Transfer Objects
    │   ├── AuthDto.cs
    │   └── UserDto.cs
    ├── Entities/                  # Domain Entities
    │   └── User.cs
    ├── Helpers/                   # Helper classes
    │   └── JwtHelper.cs
    └── Queries/                   # CQRS Queries (Read operations)
        └── Users/
            └── GetUserByIdQuery.cs
```

## Công nghệ sử dụng

- **ASP.NET Core 6.0** - Web API Framework
- **Entity Framework Core 6.0** - ORM
- **SQL Server** - Database
- **MediatR** - CQRS Pattern implementation
- **JWT Bearer Authentication** - API Authentication
- **Hangfire** - Background job processing
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API Documentation
- **BCrypt.Net** - Password hashing

## Yêu cầu

- .NET 6.0 SDK hoặc cao hơn
- SQL Server (LocalDB hoặc SQL Server instance)

## Cấu hình

### 1. Cập nhật Connection String

Mở file `SmartFactory.Api/appsettings.json` và cập nhật connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SmartFactory;User Id=sa;Password=YourPassword123;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  }
}
```

### 2. Chạy Migration

```bash
cd SmartFactory.Application
dotnet ef migrations add InitialCreate --startup-project ../SmartFactory.Api
dotnet ef database update --startup-project ../SmartFactory.Api
```

### 3. Chạy API

```bash
cd SmartFactory.Api
dotnet run
```

API sẽ chạy tại:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

## API Endpoints

### Authentication

**POST** `/api/auth/register`
```json
{
  "email": "user@example.com",
  "fullName": "Nguyen Van A",
  "password": "password123",
  "phoneNumber": "0123456789"
}
```

**POST** `/api/auth/login`
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

### Users (Requires Authentication)

**GET** `/api/users/me` - Lấy thông tin user hiện tại

**GET** `/api/users/{id}` - Lấy thông tin user theo ID

### Health Check

**GET** `/api/health` - Kiểm tra trạng thái API

## Features

### ✅ Clean Architecture
- Separation of concerns
- Dependency Inversion
- Domain-centric design

### ✅ CQRS Pattern
- Commands cho write operations
- Queries cho read operations
- MediatR pipeline behaviors

### ✅ Authentication & Authorization
- JWT Bearer token
- Secure password hashing với BCrypt
- Protected API endpoints

### ✅ Logging
- Serilog structured logging
- Console và File sinks
- Request/Response logging

### ✅ Background Jobs
- Hangfire integration
- Dashboard UI tại `/hangfire`

### ✅ API Documentation
- Swagger/OpenAPI
- JWT authentication support trong Swagger UI

## MediatR Pipeline Behaviors

Các behaviors được thực thi theo thứ tự:

1. **UnhandledExceptionBehavior** - Bắt tất cả exceptions
2. **ValidationBehavior** - Validate request
3. **PerformanceBehavior** - Monitor performance
4. **LoggingBehavior** - Log request/response

## Development Notes

- Source code sử dụng nullable reference types
- Implicit usings được enable
- Log files được lưu trong folder `logs/`
- Hangfire dashboard available tại `/hangfire`

## License

MIT

