Dựa trên các yêu cầu về 3 loại template Excel (`ep_nhu.png`, `lap_rap.png`, `phun_in.png`) và các giao diện mẫu có sẵn, dưới đây là bảng phân rã nhiệm vụ (Task Breakdown) chi tiết cho AI agent để xây dựng hệ thống quản lý PO đa phiên bản.

user story PO:
HT nhận PO từ KH file excel
HT chuyển chuyển file excel thành template bên e cung cấp.
Mỗi loại hình gia công có file template khác nhau
HT import vào hệ thống.
PM đọc dữ liệu, import các thông tin cần thiết của PO cho
các giai đoạn phía sau (tên KH, mã sp, tên sp, mã linh kiện, loại gia công....).
sinh tất cả bản ghi mới theo PO.


PM cho sửa PO ban đầu để chốt với KH. PM vẫn fai lưu bản ghi ban đầu và bản ghi chốt với KH
PMC chỉnh sửa PO chốt với KH, PM lưu trữ bản đã chỉnh sửa (thực tế sx)


### 1. Database Design (Thiết kế cơ sở dữ liệu)
Cần thiết lập cấu trúc để lưu trữ thông tin linh hoạt từ 3 loại template và quản lý 3 phiên bản dữ liệu (Gốc - Chốt - Thực tế),.

*   **Task DB-15: Bảng `Excel_Mappings` (Cấu hình Template)**
    *   Lưu thông tin mapping cột cho 3 loại hình: **ÉP NHỰA** (trọng lượng, chu kỳ), **LẮP RÁP** (nội dung lắp ráp), **PHUN IN** (vị trí phun, nội dung in),,.
*   **Task DB-16: Bảng `Purchase_Orders` (Đơn hàng đa phiên bản)**
    *   Thêm trường `version_type`: `ORIGINAL` (từ Excel), `FINAL` (PM chốt), `PRODUCTION` (PMC điều chỉnh).
    *   Thêm `template_type`: Để nhận diện PO này được import từ mẫu nào.
*   **Task DB-17: Bảng `PO_Operations` (Lớp tính tiền - PM)**
    *   Lưu các trường đặc thù từ ảnh: `Charge_Count` (số lần gia công), `Unit_Price`, `Total_Amount`,.
*   **Task DB-18: Bảng `Production_Operations` (Lớp thực tế - PMC)**
    *   Lưu thông tin: `Machine_ID`, `Tool_ID`, `Material_ID`, `Cycle_Time`.
    *   Bảng `Mapping_PO_Production`: Để liên kết 1 bước tính tiền trong PO với N bước làm thực tế.

---

### 2. Backend Development (Logic xử lý & Versioning)
Xây dựng bộ máy chuyển đổi từ file ảnh/excel thành dữ liệu hệ thống.

*   **Task BE-15: Bộ Parser cho 3 loại Template**
    *   **Logic Ép nhựa:** Đọc Product ID, Part ID, chất liệu (ABS/PVC), trọng lượng, đơn giá.
    *   **Logic Phun in:** Đọc danh sách các bước con (Sơn lót, In sơn, Kẻ vẽ) cho cùng một linh kiện.
    *   **Logic Lắp ráp:** Đọc nội dung lắp ráp tổng quát và đơn giá theo lượt.
*   **Task BE-16: Logic Sinh bản ghi tự động (Automated Generation)**
    *   Từ dữ liệu Excel, tự động tạo mới: `Customer` (nếu chưa có), `Product`, `Part`, và các `PO_Operations` tương ứng,.
*   **Task BE-17: Logic Snapshot & Cloning (Lưu phiên bản)**
    *   Khi PM sửa bản `ORIGINAL`, hệ thống nhân bản sang bản `FINAL`.
    *   Khi PMC điều chỉnh bản `FINAL`, hệ thống nhân bản sang bản `PRODUCTION` để giữ nguyên các cột mốc lịch sử,.

---

### 3. Frontend Development (Giao diện theo mẫu JPG)
Sử dụng các file ảnh, code html có sẵn bạn cung cấp để làm chuẩn UI.

File ảnh tương ứng với cái file html có sắn như sẽ được mapping đường dẫn:
po_list.jpg => po_manager.html
po.jpg => po_detail.html
detail_product.jpg => product_detail.html
vat_tu.jpg => vat_lieu.html
detail_part.jpg => part_detail.html
chu_hand.jpg => quan_ly_chu_hang.html

*   **Task FE-17: Màn hình Danh sách PO (Dựa trên `po_list.jpg`)**
    *   Hiển thị danh sách PO, trạng thái (New, In Progress, Completed) và tổng tiền.
*   **Task FE-18: Màn hình Chi tiết Đơn hàng (Dựa trên `po.jpg` & `detail_product.jpg`)**
    *   Hiển thị thông tin chung, danh sách sản phẩm và linh kiện (`SKU`, `Quantity`, `Unit Price`),.
    *   Tích hợp nút **"Chốt với khách hàng"** để đóng băng dữ liệu tài chính.
*   **Task FE-19: Màn hình Quản lý Chủ hàng (Dựa trên `chu_hand.jpg`)**
    *   Quản lý thông tin đối tác ký PO.
*   **Task FE-20: Màn hình Chi tiết Linh kiện & Gia công (Dựa trên `detail_part.jpg`)**
    *   Giao diện cho PMC điều chỉnh công đoạn thực tế: Thêm máy (`Machine ID`), thêm vật tư (`Add Material`), thêm công cụ (`Add Tool`),.
*   **Task FE-21: Màn hình Quản lý Vật tư (Dựa trên `vat_tu.jpg`)**
    *   Khu vực nhập tay thông tin vật tư tiêu hao đi kèm PO để theo dõi kho.
    
---

### Tóm tắt luồng dữ liệu cho AI Agent:
1.  **Input:** Người dùng upload file Excel và chọn loại (Ví dụ: `phun_in.png`).
2.  **Xử lý:** Backend dùng `Excel_Mappings` để bóc tách mã SP, linh kiện và các bước phun sơn tính tiền (Bản **Original**).
3.  **PM Action:** PM sửa giá/số lượng trên giao diện giống `po.jpg`, sau đó ấn chốt (Bản **Final**).
4.  **PMC Action:** PMC mở màn hình `detail_part.jpg`, hệ thống gợi ý các bước từ bản Final sang. PMC gộp/tách bước, gán máy/tool thực tế (Bản **Production**),.

**Ví dụ:** Với mẫu Phun in (`phun_in.png`), linh kiện "Thân trên trước" có 4 dòng tính tiền (Tán súng, Kẹp mô, Biên mô, Di ấn). Khi vào hệ thống, PM chốt giá cho 4 dòng này, nhưng PMC có thể gộp chúng lại thành 1 "Quy trình Sơn tổng hợp" trên giao diện sản xuất thực tế để tối ưu nhân công.




ngoài ra thông thường trong gia công thì PO sẽ đi kèm với vật tư nguyên liệu, công cụ ( nhựa hạt - dùng để ép, hoặc linh kiện trắng (đơn sắc) - nếu chỉ có gia công sơn, vật tư tiêu hao - sơn màu - mã màu sơn, tool - như khuôn - có mã khuôn với gia công ép, kẹp - mã kẹp với các công đoạn tương ứng trong gia công sơn ...) phục vụ việc nhập kho và kiểm hàng theo PO khi nhà máy/chủ hàng giao hàng cho Hải Tân

Tất cả flow theo tài liệu DIỄN GIẢI TỔNG THỂ PHẦN MỀM.docx, Tổng quan luồng nghiệp vụ và hướng thiết kế Database (1).docx