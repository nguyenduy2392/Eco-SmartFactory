# Hướng Dẫn Sử Dụng - Tính Năng Nhập Kho Nguyên Vật Liệu

## Mục Lục
1. [Giới thiệu](#giới-thiệu)
2. [Truy cập màn hình Nhập Kho](#truy-cập-màn-hình-nhập-kho)
3. [Nhập kho có gắn PO](#nhập-kho-có-gắn-po)
4. [Nhập kho không gắn PO](#nhập-kho-không-gắn-po)
5. [Xem lịch sử nhập kho](#xem-lịch-sử-nhập-kho)
6. [Cập nhật trạng thái NVL của PO](#cập-nhật-trạng-thái-nvl-của-po)

## Giới thiệu

Tính năng Nhập Kho cho phép:
- Nhập nguyên vật liệu vào kho
- Gắn phiếu nhập với PO (nếu có)
- Ghi nhận lịch sử nhập kho cho mỗi PO
- Theo dõi trạng thái nhập đủ/chưa đủ NVL cho PO

## Truy cập màn hình Nhập Kho

### Cách 1: Từ Menu
1. Mở sidebar menu (nếu đang thu gọn)
2. Click vào **"Quản lý sản xuất"**
3. Click vào **"Nhập kho"**

### Cách 2: URL trực tiếp
- Truy cập: `http://your-domain/stock-in`

## Nhập kho có gắn PO

### Bước 1: Chọn PO
1. Click vào ô **"PO Liên Quan"**
2. Nhập mã PO để tìm kiếm (ví dụ: `PO-2026-001`)
3. Chọn PO từ danh sách dropdown
4. Hệ thống sẽ hiển thị:
   - Mã PO
   - Tên khách hàng
   - Ngày PO
   - Trạng thái nhập NVL (Đã hoàn thành / Chưa đủ)

### Bước 2: Điền thông tin phiếu nhập
1. **Khách Hàng**: Chọn khách hàng (bắt buộc)
2. **Kho Nhập**: Chọn kho cần nhập (bắt buộc)
3. **Ngày Nhập**: Chọn ngày nhập kho (mặc định là hôm nay)
4. **Số Phiếu Nhập**: Nhập số phiếu (ví dụ: `PN-2026-001`)
5. **Ghi Chú**: Nhập ghi chú nếu cần (không bắt buộc)

### Bước 3: Thêm nguyên vật liệu
1. Click button **"Thêm Dòng"** để thêm NVL
2. Với mỗi dòng NVL:
   - **Nguyên Vật Liệu**: Chọn từ dropdown (bắt buộc)
   - **Số Lượng**: Nhập số lượng (bắt buộc)
   - **Đơn Vị**: Tự động điền sau khi chọn NVL
   - **Số Lô**: Nhập mã batch (bắt buộc)
   - **Mã NCC**: Nhập mã nhà cung cấp (không bắt buộc)
   - **Mã PO Mua**: Nhập mã PO mua hàng (không bắt buộc)
   - **Ghi Chú**: Ghi chú cho NVL này (không bắt buộc)
3. Click icon **Thùng rác** để xóa dòng (không thể xóa nếu chỉ có 1 dòng)

### Bước 4: Submit
1. Click button **"Nhập Kho"** (màu xanh)
2. Hệ thống sẽ:
   - Validate dữ liệu
   - Tạo phiếu nhập kho
   - Cập nhật số lượng tồn kho
   - Ghi nhận lịch sử nhập kho
   - Hiển thị thông báo thành công
3. Form sẽ reset và sẵn sàng cho lần nhập tiếp theo

## Nhập kho không gắn PO

### Bước 1: Bỏ qua chọn PO
- Không chọn PO hoặc click nút **X** để clear PO đã chọn

### Bước 2: Điền thông tin
- Làm giống như [Nhập kho có gắn PO](#nhập-kho-có-gắn-po) từ Bước 2 trở đi
- Khác biệt: Lịch sử nhập kho sẽ không được gắn với PO nào

## Xem lịch sử nhập kho

### Từ màn hình PO Detail

#### Bước 1: Mở PO Detail
1. Vào **"Quản lý sản xuất"** > **"Chủ hàng"**
2. Click vào một chủ hàng
3. Chọn tab **"Processing PO"**
4. Click icon **Mắt** để xem chi tiết PO

#### Bước 2: Xem lịch sử
1. Scroll xuống phần **"Lịch Sử Nhập Kho Nguyên Vật Liệu"**
2. Bảng hiển thị:
   - Ngày nhập
   - Số phiếu
   - Mã NVL
   - Tên NVL
   - Số lượng
   - Đơn vị
   - Số lô
   - Mã NCC
   - Mã PO Mua
   - Kho
   - Ghi chú
3. Nếu chưa có lịch sử, hiển thị thông báo "Chưa có lịch sử nhập kho cho PO này"

### Từ màn hình PO List

#### Xem trạng thái nhanh
1. Vào **"Quản lý sản xuất"** > **"Chủ hàng"**
2. Chọn tab **"Processing PO"**
3. Cột **"Trạng thái NVL"** hiển thị:
   - Badge màu xanh: **"Đã hoàn thành"** - Đã nhập đủ NVL
   - Badge màu cam: **"Chưa đủ"** - Chưa nhập đủ NVL

## Cập nhật trạng thái NVL của PO

### Khi nào cần cập nhật?
- Khi admin xác nhận đã nhập đủ NVL theo kế hoạch
- Khi cần đánh dấu PO đã hoàn tất về mặt nguyên vật liệu

### Cách cập nhật

#### Bước 1: Mở PO Detail
- Làm giống như [Xem lịch sử nhập kho](#xem-lịch-sử-nhập-kho)

#### Bước 2: Toggle checkbox
1. Tìm phần **"Lịch Sử Nhập Kho Nguyên Vật Liệu"**
2. Ở góc phải header, có checkbox **"Trạng thái:"**
3. Click vào checkbox để:
   - **Check**: Đánh dấu "Đã hoàn thành"
   - **Uncheck**: Đánh dấu "Chưa đủ"
4. Trạng thái sẽ được cập nhật ngay lập tức
5. Badge bên cạnh thay đổi màu:
   - Xanh: "Đã hoàn thành"
   - Cam: "Chưa đủ"

## Tips & Tricks

### 1. Tìm kiếm PO nhanh
- Khi gõ mã PO, hệ thống tự động search và hiển thị kết quả
- Có thể click icon dropdown để xem tất cả PO

### 2. Thêm nhiều NVL cùng lúc
- Click "Thêm Dòng" nhiều lần trước khi điền
- Điền song song nhiều dòng để nhanh hơn

### 3. Xóa PO đã chọn
- Click nút **X** màu đỏ bên phải ô PO để clear

### 4. Reset form
- Click button **"Hủy"** để reset toàn bộ form
- Hoặc form tự động reset sau khi nhập kho thành công

### 5. Validation errors
- Các trường bắt buộc có dấu **\*** màu đỏ
- Error message hiển thị màu đỏ dưới field khi invalid
- Phải touch (click) vào field trước khi error hiện

### 6. Filter NVL
- Dropdown NVL có chức năng filter
- Gõ tên hoặc mã NVL để tìm nhanh

## Lưu ý quan trọng

### ⚠️ Dữ liệu bắt buộc
- Khách hàng
- Kho nhập
- Ngày nhập
- Số phiếu nhập
- Ít nhất 1 NVL với đầy đủ thông tin:
  - NVL
  - Số lượng
  - Đơn vị
  - Số lô

### ⚠️ Số lượng tồn kho
- Sau khi nhập kho thành công, số lượng tồn kho sẽ tăng
- Kiểm tra lại tại màn hình **"Kho nguyên vật liệu"**

### ⚠️ Không thể xóa phiếu nhập
- Sau khi nhập kho thành công, không thể xóa
- Chỉ có thể điều chỉnh tồn kho qua chức năng **Điều chỉnh kho** (nếu có)

### ⚠️ Lịch sử nhập kho
- Chỉ PO nào có phiếu nhập gắn với nó mới hiển thị lịch sử
- Phiếu nhập không gắn PO sẽ không xuất hiện ở đây

### ⚠️ Trạng thái NVL
- Trạng thái "Đã hoàn thành" chỉ là cờ đánh dấu
- Hệ thống KHÔNG tự động kiểm tra có đủ số lượng hay không
- Admin phải tự kiểm tra và tick checkbox

## Các trường hợp sử dụng phổ biến

### 1. Nhập NVL theo PO
**Tình huống**: Khách hàng đặt PO, admin import Excel, sheet NVL có list nguyên vật liệu cần. Khi NVL về, cần nhập kho và gắn với PO.

**Các bước**:
1. Vào màn hình Nhập kho
2. Search và chọn PO
3. Điền thông tin phiếu nhập
4. Thêm từng NVL đã về (có thể khác với list trong Excel)
5. Submit
6. Sau khi nhập đủ, vào PO detail tick checkbox "Đã hoàn thành"

### 2. Nhập NVL không có PO
**Tình huống**: Mua NVL chung, không thuộc PO cụ thể nào.

**Các bước**:
1. Vào màn hình Nhập kho
2. KHÔNG chọn PO
3. Chọn khách hàng (có thể là "Nội bộ" hoặc customer nào đó)
4. Điền thông tin và NVL
5. Submit

### 3. Nhập NVL nhiều lần cho 1 PO
**Tình huống**: NVL về từng đợt, mỗi lần nhập một ít.

**Các bước**:
1. Mỗi lần NVL về, làm lại quy trình nhập kho có gắn PO
2. Tất cả các lần nhập đều được ghi nhận trong lịch sử
3. Khi đủ, admin tick "Đã hoàn thành"

### 4. Kiểm tra đã nhập kho chưa
**Tình huống**: Cần kiểm tra xem PO đã nhập kho NVL chưa, đã đủ chưa.

**Các bước**:
1. Vào PO List, check cột "Trạng thái NVL"
2. Hoặc vào PO Detail, scroll xuống xem bảng lịch sử
3. So sánh với list NVL trong sheet Excel (tab Nguyên Vật Liệu)

## Troubleshooting

### ❌ "Không thể tải danh sách kho"
**Nguyên nhân**: Lỗi kết nối API hoặc chưa có kho nào trong hệ thống.
**Giải pháp**: Kiểm tra kết nối mạng, hoặc vào System > Kho để tạo kho mới.

### ❌ "Vui lòng điền đầy đủ thông tin bắt buộc"
**Nguyên nhân**: Có trường bắt buộc chưa điền.
**Giải pháp**: Scroll lên và kiểm tra các trường có dấu * màu đỏ, điền đủ thông tin.

### ❌ "Không thể nhập kho"
**Nguyên nhân**: Lỗi backend hoặc dữ liệu không hợp lệ.
**Giải pháp**: 
- Kiểm tra lại tất cả dữ liệu
- Kiểm tra số lượng có hợp lệ không
- Kiểm tra kết nối API
- Liên hệ admin nếu vẫn lỗi

### ❌ Không tìm thấy PO
**Nguyên nhân**: PO chưa tồn tại hoặc đã bị xóa.
**Giải pháp**: Kiểm tra lại mã PO, hoặc import PO từ Excel trước.

## Liên hệ hỗ trợ

Nếu gặp vấn đề, vui lòng liên hệ:
- Email: support@smartfactory.com
- Hotline: 1900-xxxx
- Hoặc tạo ticket qua hệ thống

---

**Phiên bản**: 1.0
**Ngày cập nhật**: 2026-01-09
