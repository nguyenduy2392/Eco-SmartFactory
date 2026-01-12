# Hướng Dẫn Sử Dụng PMC Planning

## Tổng quan
PMC Planning là công cụ lập kế hoạch sản xuất chi tiết theo tuần cho toàn bộ sản phẩm - linh kiện - khách hàng phun/in.

## Truy cập
- URL: `http://localhost:4200/production-planning`
- Menu: **Quản lý sản xuất → Kế hoạch chi tiết**

## Cấu trúc tuần PMC
- Mỗi PMC gồm **6 ngày**: Thứ 2 → Thứ 7 của tuần kế tiếp
- Nếu tạo vào Chủ nhật → tự động tạo từ Thứ 2 đến Thứ 7 của tuần sau
- **Mỗi tuần chỉ có 1 PMC duy nhất** (không có version)

## Giao diện

### Phần 1: Thống kê tổng hợp (Trên dấu đỏ)
Hiển thị các chỉ số thống kê theo từng ngày:

1. **#CN Cần SDV**: Số lượng công nhân cần cho công ty SDV
2. **#CN Cần VM**: Số lượng công nhân cần cho công ty VM
3. **#CN Cần LUCKY**: Số lượng công nhân cần cho công ty LUCKY
4. **TỔNG CẦN CN**: Tổng số công nhân cần (highlight màu vàng)
5. **#CN Có**: Số lượng công nhân có sẵn
6. **DT THEO KHSX**: Doanh thu theo kế hoạch sản xuất
7. **DT THEO NGTV1**: Doanh thu theo nguyên tắc V1
8. **VƯỢT**: Phần trăm vượt mục tiêu (có màu sắc):
   - 🔴 Đỏ: ≥ 90%
   - 🟡 Vàng: 85-89%
   - 🟢 Xanh: < 85%
9. **#CN Thiếu**: Số công nhân thiếu (highlight màu vàng)

### Phần 2: Chi tiết kế hoạch (Dưới dấu đỏ)
Bảng chi tiết với các cột:
- **Mã vật liệu**: Mã sản phẩm
- **Tên linh kiện**: Tên chi tiết
- **Kế hoạch**: Loại kế hoạch
- **Tổng**: Tổng số lượng
- **Khách hàng**: Tên khách hàng
- **Các ngày**: 6 cột từ Thứ 2 đến Thứ 7

Mỗi sản phẩm có 3 hàng dữ liệu:
1. **Yêu cầu KH/PO数量** (màu vàng): Yêu cầu PO muốn giao công
2. **Kế hoạch SX/生产计划** (màu xanh): Số lượng sẽ sản xuất
3. **Kẹp/喷油模** (màu cam): Số lượng kẹp dùng

## Chức năng

### 1. Tạo PMC tuần mới
Khi chưa có PMC cho tuần hiện tại:
- Click **"Tạo PMC Tuần Mới"**: Tạo PMC trống
- Click **"Tạo từ Tuần Trước"**: Copy dữ liệu từ tuần trước

### 2. Xem PMC theo tuần
- **< (Tuần trước)**: Xem PMC tuần trước đó
- **> (Tuần sau)**: Xem PMC tuần tiếp theo
- Hiển thị tên tuần và trạng thái ở header

### 3. Nhập/Chỉnh sửa dữ liệu
- Click vào ô số trong phần chi tiết để nhập giá trị
- Các ô có thể nhập:
  - Yêu cầu KH/PO数量
  - Kế hoạch SX/生产计划
  - Kẹp/喷油模
- Khi có thay đổi, nút **"Lưu"** sẽ được enable

### 4. Lưu dữ liệu
- Click **"Lưu"** để lưu thay đổi
- Hệ thống tự động:
  - Cập nhật dữ liệu hiện tại (không tạo version mới)
  - Tính toán lại phần thống kê
  - Hiển thị thông báo thành công

### 5. Làm mới
- Click **"Làm mới"** để tải lại dữ liệu từ server
- Hủy bỏ các thay đổi chưa lưu

## Tự động tính toán

### Phần thống kê tự động cập nhật khi:
1. Thay đổi giá trị **Kẹp/喷油模**:
   - Tính lại #CN Cần SDV, VM, LUCKY
   - Tính lại TỔNG CẦN CN
   - Tính lại #CN Thiếu

2. Thay đổi **Kế hoạch SX**:
   - Tính lại DT THEO KHSX
   - Tính lại VƯỢT %

## Màu sắc & Highlight

### Phần thống kê:
- **Highlight vàng**: Các chỉ số quan trọng (TỔNG CẦN CN, VƯỢT, #CN Thiếu)
- **Màu động VƯỢT**: Đỏ/Vàng/Xanh theo phần trăm

### Phần chi tiết:
- **Màu vàng nhạt**: Yêu cầu KH/PO
- **Màu xanh nhạt**: Kế hoạch SX
- **Màu cam nhạt**: Kẹp
- **Màu xanh đậm**: Cột cố định (Mã VL, Tên LK, Tổng, KH)

## Trạng thái PMC
- **DRAFT**: Đang soạn thảo (màu cam)
- **APPROVED**: Đã phê duyệt (màu xanh)

## Lưu ý
- Chỉ có thể chỉnh sửa PMC ở trạng thái DRAFT
- **Mỗi tuần chỉ có 1 PMC duy nhất** - không có versioning
- Dữ liệu được cập nhật trực tiếp khi nhấn "Lưu"
- Nên lưu thường xuyên để tránh mất dữ liệu
- Không thể tạo PMC trùng tuần - phải xóa PMC cũ trước

## API Integration
Component đã được tích hợp đầy đủ với backend:
- `GET /pmc?weekStart={date}`: Lấ (không được trùng tuần)
- `POST /pmc/save`: Lưu PMC (cập nhật trực tiếp, không tạo version
- `POST /pmc/save`: Lưu PMC (tạo version mới)
- `GET /pmc/previous?weekStart={date}`: Lấy PMC tuần trước

## Keyboard Shortcuts
- **Tab**: Di chuyển giữa các ô nhập liệu
- **Enter**: Xác nhận nhập và chuyển ô tiếp theo
- **Esc**: Hủy chỉnh sửa ô hiện tại
