# Migration từ INT sang GUID cho User và Product

## Tóm tắt thay đổi

Đã thay đổi kiểu dữ liệu ID từ `int` sang `Guid` cho các entity User và Product.

## Backend Changes

### 1. Entities
- ✅ `User.cs` - Thay đổi `Id` từ `int` sang `Guid`
- ✅ `Product.cs` - Thay đổi `Id` từ `int` sang `Guid`

### 2. DTOs
- ✅ `UserDto.cs` - Thay đổi `Id` từ `int` sang `Guid`
- ✅ `ProductDto.cs` - Thay đổi `Id` từ `int` sang `Guid`

### 3. Commands & Queries
- ✅ `UpdateUserCommand.cs` - Thay đổi `Id` từ `int` sang `Guid`
- ✅ `DeleteUserCommand.cs` - Thay đổi `UserId` từ `int` sang `Guid`
- ✅ `GetUserByIdQuery.cs` - Thay đổi `UserId` từ `int` sang `Guid`
- ✅ `GetProductByIdQuery.cs` - Thay đổi `ProductId` từ `int` sang `Guid`

### 4. Helpers
- ✅ `JwtHelper.cs` - Thay đổi `GenerateToken()` và `GetUserIdFromToken()` từ `int` sang `Guid`

### 5. Controllers
- ✅ `UsersController.cs` - Thay đổi tất cả parameters từ `int` sang `Guid`
- ✅ `ProductsController.cs` - Thay đổi tất cả parameters từ `int` sang `Guid`
- ✅ `BaseApiController.cs` - Thay đổi `GetCurrentUserId()` từ `int?` sang `Guid?`

### 6. Database
- ✅ `ApplicationDbContext.cs` - Thêm `ValueGeneratedOnAdd()` cho Guid properties
- ✅ Xóa database cũ và tạo migration mới `InitialWithGuid`
- ✅ Apply migration thành công

## Frontend Changes

### 1. Services
- ✅ `user.service.ts` - Thay đổi tất cả parameters từ `number` sang `string`
- ✅ `product.service.ts` - Tạo mới với Guid support

### 2. Models/Interfaces
- ✅ `user.interface.ts` - Tạo mới với `id: string`
- ✅ `product.interface.ts` - Tạo mới với `id: string`

### 3. Components
- ✅ Các component hiện tại sử dụng `any` type nên không cần thay đổi
- ✅ Frontend sẽ tự động xử lý Guid dưới dạng string

## Lưu ý quan trọng

1. **Database đã được drop và recreate** - Tất cả dữ liệu cũ đã bị xóa
2. **Guid được generate tự động** - Không cần truyền Id khi tạo mới
3. **API endpoints không thay đổi** - Chỉ thay đổi kiểu dữ liệu của Id parameter
4. **Frontend xử lý Guid như string** - TypeScript/JavaScript xử lý Guid dưới dạng string

## Testing

Sau khi migration, cần test:
- ✅ Tạo mới User/Product
- ✅ Lấy User/Product theo Id
- ✅ Cập nhật User/Product
- ✅ Xóa User/Product
- ✅ Login và JWT token generation
- ✅ Frontend hiển thị và xử lý dữ liệu đúng

## Rollback (nếu cần)

Để rollback về int:
1. Revert tất cả các file đã thay đổi
2. Drop database
3. Tạo lại migration với int
4. Apply migration
