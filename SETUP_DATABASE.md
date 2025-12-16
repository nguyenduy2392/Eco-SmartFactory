# Hướng dẫn Setup Database cho SmartFactory

## Yêu cầu

- SQL Server (LocalDB hoặc SQL Server instance)
- .NET 6.0 SDK

## Các bước thực hiện

### 1. Cập nhật Connection String

Cập nhật connection string trong các file sau:

**SmartFactory.Api/appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SmartFactory;User Id=sa;Password=YourPassword123;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  }
}
```

**SmartFactory.Application/appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SmartFactory;User Id=sa;Password=YourPassword123;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  }
}
```

### 2. Cài đặt EF Core Tools (nếu chưa có)

```bash
dotnet tool install --global dotnet-ef
```

Hoặc cập nhật nếu đã có:

```bash
dotnet tool update --global dotnet-ef
```

### 3. Tạo Migration đầu tiên

Từ thư mục root của solution:

```bash
dotnet ef migrations add InitialCreate --project SmartFactory.Application --startup-project SmartFactory.Api
```

### 4. Áp dụng Migration vào Database

```bash
dotnet ef database update --project SmartFactory.Application --startup-project SmartFactory.Api
```

### 5. Kiểm tra Database

Kết nối vào SQL Server và kiểm tra:

```sql
USE SmartFactory;

-- Xem danh sách bảng
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Xem dữ liệu trong bảng Users (sẽ rỗng)
SELECT * FROM Users;
```

## Các lệnh EF Core hữu ích

### Tạo Migration mới

```bash
dotnet ef migrations add MigrationName --project SmartFactory.Application --startup-project SmartFactory.Api
```

### Xóa Migration cuối cùng (chưa apply)

```bash
dotnet ef migrations remove --project SmartFactory.Application --startup-project SmartFactory.Api
```

### Áp dụng Migration

```bash
dotnet ef database update --project SmartFactory.Application --startup-project SmartFactory.Api
```

### Rollback về Migration cụ thể

```bash
dotnet ef database update MigrationName --project SmartFactory.Application --startup-project SmartFactory.Api
```

### Xóa toàn bộ Database

```bash
dotnet ef database drop --project SmartFactory.Application --startup-project SmartFactory.Api
```

### Tạo SQL script từ Migrations

```bash
dotnet ef migrations script --project SmartFactory.Application --startup-project SmartFactory.Api --output migration.sql
```

## Xử lý lỗi thường gặp

### Lỗi: "Login failed for user 'sa'"

- Kiểm tra username/password trong connection string
- Đảm bảo SQL Server đang chạy
- Kiểm tra SQL Server Authentication mode (phải enable Mixed Mode)

### Lỗi: "A network-related or instance-specific error"

- Kiểm tra SQL Server service đang chạy
- Kiểm tra tên server trong connection string
- Kiểm tra firewall

### Lỗi: "Cannot open database"

- Đảm bảo user có quyền tạo database
- Thử tạo database thủ công trước:

```sql
CREATE DATABASE SmartFactory;
```

## Connection String Examples

### SQL Server LocalDB

```
Server=(localdb)\\mssqllocaldb;Database=SmartFactory;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;
```

### SQL Server với Windows Authentication

```
Server=localhost;Database=SmartFactory;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=True;
```

### SQL Server với SQL Authentication

```
Server=localhost;Database=SmartFactory;User Id=sa;Password=YourPassword123;MultipleActiveResultSets=true;TrustServerCertificate=True;
```

### SQL Server trên máy chủ khác

```
Server=192.168.1.100,1433;Database=SmartFactory;User Id=sa;Password=YourPassword123;MultipleActiveResultSets=true;TrustServerCertificate=True;
```

## Seeding Data (Optional)

Nếu muốn tạo dữ liệu mẫu, bạn có thể:

1. Tạo file seed trong `SmartFactory.Application/Data/`
2. Gọi seed trong `Program.cs` khi startup
3. Hoặc tạo migration riêng cho seed data

Ví dụ tạo user mẫu:

```sql
USE SmartFactory;

INSERT INTO Users (Email, FullName, PasswordHash, PhoneNumber, IsActive, CreatedAt)
VALUES 
('admin@smartfactory.com', 'Administrator', '$2a$11$abcdefghijklmnopqrstuvwxyz...', '0123456789', 1, GETUTCDATE()),
('user@smartfactory.com', 'Test User', '$2a$11$abcdefghijklmnopqrstuvwxyz...', '0987654321', 1, GETUTCDATE());
```

**Lưu ý:** PasswordHash phải được tạo bằng BCrypt. Có thể dùng API register để tạo user.

