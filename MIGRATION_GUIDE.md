# Hướng dẫn Migration và Triển khai

## 1. Tạo Migration cho Database

Chạy các lệnh sau trong terminal tại thư mục `SmartFactory.Api`:

```bash
# Tạo migration mới
dotnet ef migrations add AddPOMaterialManagement --project ../SmartFactory.Application --startup-project .

# Cập nhật database
dotnet ef database update --project ../SmartFactory.Application --startup-project .
```

## 2. Các Bảng Mới Được Tạo

### PurchaseOrderMaterials
- Lưu danh sách nguyên vật liệu từ sheet NVL trong file Excel PO
- KHÔNG phải dữ liệu nhập kho thực tế
- Chỉ dùng để hiển thị kế hoạch/định mức NVL

### MaterialReceiptHistories
- Lịch sử nhập kho cho PO
- Ghi lại các lần nhập kho liên quan đến PO
- Có thể nullable PurchaseOrderId (nhập kho không gắn PO)

### Cập nhật PurchaseOrders
- Thêm cột `IsMaterialFullyReceived` (bit/boolean)
- Đánh dấu PO đã hoàn thành nhập NVL

## 3. API Endpoints Mới

### Stock In (Nhập Kho)
```http
POST /api/stockin
Content-Type: application/json

{
  "purchaseOrderId": "guid hoặc null",
  "customerId": "guid",
  "warehouseId": "guid",
  "receiptDate": "2026-01-09T10:00:00Z",
  "receiptNumber": "PN-2026-001",
  "notes": "Nhập kho từ chủ hàng",
  "materials": [
    {
      "materialId": "guid",
      "quantity": 100.5,
      "unit": "kg",
      "batchNumber": "BATCH-001",
      "supplierCode": "SUP-01",
      "purchasePOCode": "PO-001",
      "notes": ""
    }
  ]
}
```

### Cập nhật trạng thái hoàn thành NVL của PO
```http
PUT /api/purchaseorders/{id}/material-status
Content-Type: application/json

{
  "isMaterialFullyReceived": true
}
```

### Lấy lịch sử nhập kho của PO
```http
GET /api/purchaseorders/{id}/receipt-history
```

### Lấy danh sách PO cho dropdown (search)
```http
GET /api/purchaseorders/for-selection?searchTerm=PO-2026
```

## 4. Luồng Nghiệp Vụ

### Import Excel PO
1. User upload file Excel có 2 sheet:
   - Sheet PO: thông tin PO chính
   - Sheet NVL: danh sách nguyên vật liệu
2. Hệ thống:
   - Insert PO vào bảng PurchaseOrders
   - Insert NVL vào bảng **PurchaseOrderMaterials** (KHÔNG phải Warehouse)
   - KHÔNG ảnh hưởng tồn kho

### Nhập Kho (Stock In)
1. User vào màn hình Nhập Kho
2. Chọn hoặc không chọn PO (có search theo mã PO)
3. Nhập thông tin nguyên vật liệu
4. Hệ thống:
   - Insert vào **MaterialReceipts** (tồn kho thực tế)
   - Cập nhật **Materials.CurrentStock**
   - Tạo **MaterialTransactionHistory**
   - Nếu có gắn PO: tạo **MaterialReceiptHistory**

### Quản lý PO
1. Admin xem chi tiết PO
2. Xem tab "Lịch sử nhập kho"
3. Tick checkbox "Đã hoàn thành nhập NVL"
4. Trạng thái hiển thị trên list PO

## 5. Tóm tắt các Service và Command/Query

### Services
- `StockInService`: Xử lý nghiệp vụ nhập kho

### Commands
- `UpdatePOMaterialStatusCommand`: Cập nhật trạng thái hoàn thành NVL

### Queries
- `GetPOMaterialReceiptHistoryQuery`: Lấy lịch sử nhập kho của PO
- `GetPOsForSelectionQuery`: Lấy danh sách PO cho dropdown

### Controllers
- `StockInController`: API nhập kho
- `PurchaseOrdersController`: Thêm 3 endpoints mới

## 6. Các Entity Mới

### PurchaseOrderMaterial
```csharp
- Id (Guid)
- PurchaseOrderId (Guid)
- MaterialCode (string)
- MaterialName (string)
- MaterialType (string?)
- PlannedQuantity (decimal)
- Unit (string)
- ColorCode (string?)
- Notes (string?)
```

### MaterialReceiptHistory
```csharp
- Id (Guid)
- PurchaseOrderId (Guid?) - nullable
- MaterialReceiptId (Guid)
- MaterialId (Guid)
- Quantity (decimal)
- Unit (string)
- BatchNumber (string)
- ReceiptDate (DateTime)
- CreatedBy (string?)
- Notes (string?)
```

## 7. Kiểm tra sau khi Migration

```sql
-- Kiểm tra các bảng đã được tạo
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('PurchaseOrderMaterials', 'MaterialReceiptHistories')

-- Kiểm tra cột mới trong PurchaseOrders
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PurchaseOrders' 
AND COLUMN_NAME = 'IsMaterialFullyReceived'
```
