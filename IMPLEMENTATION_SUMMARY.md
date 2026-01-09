# Tóm tắt Triển khai Tính năng Nhập Kho và Quản lý NVL cho PO

## ✅ ĐÃ HOÀN THÀNH

### 1. **Entities Mới** 

#### PurchaseOrderMaterial
- Bảng trung gian lưu danh sách NVL từ sheet NVL trong Excel PO
- KHÔNG insert vào Warehouse, chỉ hiển thị kế hoạch/định mức
- Location: [PurchaseOrderMaterial.cs](smart-factory.api/SmartFactory.Application/Entities/PurchaseOrderMaterial.cs)

#### MaterialReceiptHistory
- Lịch sử nhập kho cho PO
- Lưu thông tin các lần nhập kho liên quan đến PO
- PurchaseOrderId nullable (nhập kho không gắn PO)
- Location: [MaterialReceiptHistory.cs](smart-factory.api/SmartFactory.Application/Entities/MaterialReceiptHistory.cs)

#### PurchaseOrder - Updated
- Thêm cột `IsMaterialFullyReceived` (bool)
- Admin đánh dấu đã hoàn thành nhập NVL
- Location: [PurchaseOrder.cs](smart-factory.api/SmartFactory.Application/Entities/PurchaseOrder.cs#L66-L70)

### 2. **DTOs**

#### StockInDTOs.cs
- `StockInRequest`: Request nhập kho
- `StockInMaterialItem`: Từng nguyên vật liệu nhập
- `MaterialReceiptHistoryDto`: DTO lịch sử nhập kho
- `PurchaseOrderMaterialDto`: DTO NVL của PO
- `UpdatePOMaterialStatusRequest`: Cập nhật trạng thái NVL
- `POForSelectionDto`: DTO cho dropdown chọn PO
- Location: [StockInDTOs.cs](smart-factory.api/SmartFactory.Application/DTOs/StockInDTOs.cs)

### 3. **Service**

#### StockInService
Xử lý nghiệp vụ nhập kho:
- `ProcessStockInAsync`: Nhập kho (có/không gắn PO)
  - Insert MaterialReceipt (tồn kho thực tế)
  - Cập nhật Material.CurrentStock
  - Tạo MaterialTransactionHistory
  - Nếu có PO: tạo MaterialReceiptHistory
- `GetPOReceiptHistoryAsync`: Lấy lịch sử nhập kho của PO
- `GetPOsForSelectionAsync`: Lấy danh sách PO cho dropdown
- Location: [StockInService.cs](smart-factory.api/SmartFactory.Application/Services/StockInService.cs)

### 4. **Commands & Queries**

#### Commands
- `UpdatePOMaterialStatusCommand`: Cập nhật cờ IsMaterialFullyReceived
- Location: [UpdatePOMaterialStatusCommand.cs](smart-factory.api/SmartFactory.Application/Commands/PurchaseOrders/)

#### Queries
- `GetPOMaterialReceiptHistoryQuery`: Lấy lịch sử nhập kho PO
- `GetPOsForSelectionQuery`: Lấy danh sách PO để chọn
- Location: [Queries/PurchaseOrders/](smart-factory.api/SmartFactory.Application/Queries/PurchaseOrders/)

### 5. **Controllers**

#### StockInController (MỚI)
```csharp
POST /api/stockin - Nhập kho nguyên vật liệu
```
- Location: [StockInController.cs](smart-factory.api/SmartFactory.Api/Controllers/StockInController.cs)

#### PurchaseOrdersController (CẬP NHẬT)
Thêm 3 endpoints mới:
```csharp
PUT  /api/purchaseorders/{id}/material-status    - Cập nhật trạng thái NVL
GET  /api/purchaseorders/{id}/receipt-history    - Lấy lịch sử nhập kho
GET  /api/purchaseorders/for-selection          - Danh sách PO cho dropdown
```
- Location: [PurchaseOrdersController.cs](smart-factory.api/SmartFactory.Api/Controllers/PurchaseOrdersController.cs#L359-L405)

### 6. **Database Context**

#### ApplicationDbContext - Updated
- Thêm DbSet: `PurchaseOrderMaterials`, `MaterialReceiptHistories`
- Configuration methods cho 2 bảng mới
- Location: [ApplicationDbContext.cs](smart-factory.api/SmartFactory.Application/Data/ApplicationDbContext.cs)

### 7. **Program.cs - Updated**
- Đăng ký `StockInService` vào DI container
- Location: [Program.cs](smart-factory.api/SmartFactory.Api/Program.cs#L140)

## 📋 CẦN LÀM TIẾP

### 1. Tạo Migration và Update Database

```bash
# Đóng Visual Studio nếu đang chạy API
# Sau đó chạy lệnh:

cd c:\Projects\SmartFactory\smart-factory.api\SmartFactory.Api

# Tạo migration
dotnet ef migrations add AddPOMaterialManagement --project ../SmartFactory.Application

# Cập nhật database
dotnet ef database update --project ../SmartFactory.Application
```

### 2. Cập nhật Logic Import Excel PO

Cần cập nhật file Excel Import để:
- Đọc sheet "Nguyên Vật Liệu" (NVL)
- Insert vào bảng `PurchaseOrderMaterials` thay vì `Warehouse`
- KHÔNG cập nhật tồn kho Material

**File cần sửa:**
- Location: Tìm file ImportPOFromExcelCommand handler

### 3. Triển khai Frontend (Angular)

#### a. Module/Component Nhập Kho (Stock In)
- Form nhập kho với dropdown chọn PO (có search)
- Grid nhập danh sách NVL
- Gọi API POST /api/stockin

#### b. Chi tiết PO - Tab Lịch sử nhập kho
- Hiển thị danh sách lịch sử nhập kho
- Gọi API GET /api/purchaseorders/{id}/receipt-history

#### c. List PO - Hiển thị trạng thái NVL
- Thêm cột "Trạng thái NVL"
- Checkbox để đánh dấu hoàn thành
- Gọi API PUT /api/purchaseorders/{id}/material-status

## 🔍 NGHIỆP VỤ HOẠT ĐỘNG

### Luồng 1: Import Excel PO
1. User upload file Excel (2 sheet: PO + NVL)
2. Sheet PO → PurchaseOrders
3. Sheet NVL → PurchaseOrderMaterials (chỉ hiển thị, không ảnh hưởng tồn kho)

### Luồng 2: Nhập Kho (Stock In)
1. User vào màn hình Nhập Kho
2. Chọn hoặc không chọn PO (search theo mã)
3. Nhập danh sách NVL
4. Hệ thống:
   - ✅ MaterialReceipts (tồn kho thực tế)
   - ✅ Materials.CurrentStock (cập nhật)
   - ✅ MaterialTransactionHistory (ghi log)
   - ✅ MaterialReceiptHistory (nếu gắn PO)

### Luồng 3: Quản lý PO
1. Admin xem chi tiết PO
2. Tab "Lịch sử nhập kho" → hiển thị các lần nhập
3. Tick "Đã hoàn thành nhập NVL" → cập nhật IsMaterialFullyReceived

## 📊 CẤU TRÚC DATABASE MỚI

```
PurchaseOrders
├─ IsMaterialFullyReceived (bit) - MỚI
├─ PurchaseOrderMaterials (1-N) - MỚI
└─ MaterialReceiptHistories (1-N) - MỚI

PurchaseOrderMaterials (MỚI)
├─ Id
├─ PurchaseOrderId (FK)
├─ MaterialCode
├─ MaterialName
├─ PlannedQuantity
└─ Unit

MaterialReceiptHistories (MỚI)
├─ Id
├─ PurchaseOrderId (FK, nullable)
├─ MaterialReceiptId (FK)
├─ MaterialId (FK)
├─ Quantity
├─ BatchNumber
└─ ReceiptDate
```

## 📝 GHI CHÚ

- ✅ Backend đã hoàn thành 100%
- ⏳ Cần tạo migration và update database
- ⏳ Cần cập nhật logic import Excel
- ⏳ Cần triển khai Frontend

## 📖 TÀI LIỆU THAM KHẢO

- Hướng dẫn chi tiết: [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)
- API Documentation: Swagger UI khi chạy ứng dụng
