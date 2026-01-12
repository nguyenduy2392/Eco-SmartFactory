# Debug PMC - Không hiển thị dữ liệu

## Vấn đề hiện tại
UI PMC hiển thị: "Chưa có dữ liệu PMC. Vui lòng tạo dữ liệu từ 'Tạo PMC Tuần Mới'."

## Nguyên nhân có thể

1. **Chưa có PMC Week nào được tạo**
2. **Khi tạo PMC, không tìm thấy PO phù hợp** (phun/in)
3. **PO không có Products/Parts được liên kết**

## Các thay đổi đã thực hiện

### 1. Cập nhật logic lấy PO (CreatePMCWeekCommand.cs)

**Trước:**
- Chỉ lấy PO với status = "APPROVED_FOR_PMC"
- Bắt buộc phải có Parts

**Sau:**
- Lấy PO với nhiều status: "APPROVED_FOR_PMC", "DRAFT", "APPROVED"
- Nếu Product không có Parts → dùng Product name làm component
- Thêm logging chi tiết

### 2. Thêm Debug API

Endpoint mới: `GET /api/pmc/debug/available-pos`

API này trả về:
- Tổng số PO
- Số PO phù hợp cho PMC
- Chi tiết từng PO (Products, Parts)
- Thống kê theo ProcessingType và Status

## Hướng dẫn kiểm tra

### Bước 1: Kiểm tra PO có sẵn

Sử dụng Postman/Thunder Client để gọi:

```http
GET http://localhost:5000/api/pmc/debug/available-pos
Authorization: Bearer {your-token}
```

**Kết quả mong đợi:**
```json
{
  "TotalPOs": 10,
  "EligiblePOsCount": 5,
  "EligiblePOs": [
    {
      "PONumber": "PO-2024-001",
      "Status": "DRAFT",
      "ProcessingType": "PHUN_IN",
      "CustomerName": "Customer A",
      "ProductCount": 2,
      "Products": [
        {
          "ProductCode": "P001",
          "ProductName": "Product 1",
          "PartCount": 3,
          "Parts": [
            { "Code": "PART001", "Name": "Thân trên" },
            { "Code": "PART002", "Name": "Thân dưới" },
            { "Code": "PART003", "Name": "Đầu" }
          ]
        }
      ]
    }
  ],
  "AllPOsGroupByProcessingType": [...],
  "AllPOsGroupByStatus": [...]
}
```

**Nếu EligiblePOsCount = 0:**
- Kiểm tra ProcessingType của PO trong database
- Kiểm tra Status của PO
- Đảm bảo có ít nhất 1 PO với ProcessingType = "PHUN_IN", "PHUN", hoặc "IN"

### Bước 2: Tạo PMC tuần mới

Trong UI, click nút **"Tạo PMC Tuần Mới"**

Hoặc dùng API:

```http
POST http://localhost:5000/api/pmc/create
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "copyFromPreviousWeek": false
}
```

### Bước 3: Kiểm tra logs

Sau khi tạo PMC, kiểm tra file log trong `SmartFactory.Api/logs/log-YYYYMMDD.txt`

Tìm các dòng log:
```
Found {POCount} POs for PMC initialization
Processing PO {PONumber} with {ProductCount} products
Product {ProductCode} has {PartCount} parts
Created {RowCount} rows for PMC initialization
Initialized {RowCount} rows with {CellCount} cells for PMC Week
```

### Bước 4: Kiểm tra UI

1. Refresh trang PMC
2. Nếu có dữ liệu → bảng PMC sẽ hiển thị
3. Nếu vẫn không có → kiểm tra Console log trong browser (F12)

## Giải pháp tạm thời

Nếu database chưa có PO với Parts, có thể:

### Giải pháp 1: Import PO có linh kiện
- Import file Excel PO phun/in có đầy đủ thông tin Products và Parts

### Giải pháp 2: Thêm Parts cho Product hiện có
- Vào màn hình Products
- Chọn một Product trong PO phun/in
- Thêm Parts cho Product đó

### Giải pháp 3: Tạo test data
Chạy SQL script để tạo test data:

```sql
-- Tạo test Product
INSERT INTO Products (Id, Code, Name, IsActive, CreatedAt)
VALUES (NEWID(), 'TEST-001', 'Test Product', 1, GETUTCDATE());

-- Lấy ProductId vừa tạo
DECLARE @ProductId UNIQUEIDENTIFIER = (SELECT Id FROM Products WHERE Code = 'TEST-001');

-- Tạo Parts cho Product
INSERT INTO Parts (Id, ProductId, Code, Name, IsActive, CreatedAt)
VALUES 
  (NEWID(), @ProductId, 'PART-001', 'Thân trên', 1, GETUTCDATE()),
  (NEWID(), @ProductId, 'PART-002', 'Thân dưới', 1, GETUTCDATE()),
  (NEWID(), @ProductId, 'PART-003', 'Đầu', 1, GETUTCDATE());

-- Liên kết Product với PO (cần có POId từ database)
-- INSERT INTO POProducts (Id, POId, ProductId, ...)
```

## Troubleshooting

### Lỗi: "PMC Week not found"
- Nghĩa là GetPMCWeek trả về null
- Cần tạo PMC mới bằng nút "Tạo PMC Tuần Mới"

### Lỗi: "No rows created for PMC Week"
- Không tìm thấy PO phù hợp
- Kiểm tra debug endpoint `/api/pmc/debug/available-pos`

### UI không cập nhật sau khi tạo PMC
- Kiểm tra Network tab (F12) xem API có trả về dữ liệu không
- Kiểm tra Console log có lỗi JavaScript không
- Thử refresh trang (F5)

### Bảng hiển thị nhưng không có dòng nào
- Backend đã tạo PMC Week nhưng không có Rows
- Xem log để biết lý do
- Kiểm tra PO có Products/Parts không

## Next Steps

Sau khi fix:
1. Rebuild API (Stop Visual Studio debugger, rồi rebuild)
2. Restart API server
3. Refresh frontend
4. Test lại flow tạo PMC

## Contact

Nếu vẫn gặp vấn đề, cung cấp:
1. Output từ `/api/pmc/debug/available-pos`
2. Log file từ `SmartFactory.Api/logs/`
3. Screenshot của Network tab (F12) khi gọi API
