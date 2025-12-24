I. TỔNG QUAN LUỒNG NGHIỆP VỤ (BUSINESS OVERVIEW)
Hải Tân là đơn vị GIA CÔNG, không phải chủ sản phẩm.
•	Hải Tân nhận PO gia công từ nhiều nhà máy / chủ hàng khác nhau
•	Mỗi PO:
o	Thuộc 1 chủ hàng
o	Có nhiều sản phẩm
o	Mỗi sản phẩm có nhiều linh kiện
•	Mỗi linh kiện:
o	Có thể tham gia nhiều loại hình gia công
	ÉP
	SƠN
	LẮP RÁP
•	Mỗi loại hình gia công:
o	Gồm nhiều loại công đoạn
•	Mỗi loại công đoạn:
o	Có thể gồm nhiều công đoạn con (bước gia công)
•	Mỗi công đoạn con:
o	Sử dụng:
	1 loại vật tư/nguyên liệu
	1 loại tool/công cụ
📌 Nhà máy (chủ hàng) cung cấp:
•	Tool: khuôn, kẹp, …
•	Nguyên liệu:
o	Nhựa (ép)
o	Sơn, dung môi (sơn)
•	Linh kiện bán thành phẩm (nếu có)
II. ĐỊNH NGHĨA CÁC THÀNH TỐ (CHỐT THUẬT NGỮ)
Phần này rất quan trọng để BA & RD thống nhất.
1️⃣ Chủ hàng / Nhà máy (OWNER / CUSTOMER)
Đơn vị sở hữu sản phẩm, ký hợp đồng gia công với Hải Tân.
Thuộc tính chính:
•	Customer_ID
•	Tên
•	Địa chỉ
•	Người liên hệ
•	Điều khoản thanh toán
2️⃣ PO GIA CÔNG (PROCESSING PO)
Đơn đặt hàng gia công do chủ hàng gửi cho Hải Tân.
PO dùng để:
•	Ràng buộc pháp lý
•	Tính giá
•	Theo dõi tiến độ
•	Quyết toán
Thuộc tính chính:
•	PO_ID
•	Customer_ID
•	Ngày nhận PO
•	Ngày giao dự kiến
•	Trạng thái
3️⃣ SẢN PHẨM (PRODUCT)
Mã sản phẩm tổng (ví dụ: PKW4180-0002)
•	Một PO có thể có nhiều sản phẩm
4️⃣ LINH KIỆN (PART)
Chi tiết cấu thành sản phẩm, là đối tượng gia công thực tế.
Ví dụ:
•	Thân trên
•	Thân dưới
•	Đầu
•	Chân
Thuộc tính chính:
•	Part_ID
•	Product_ID
•	Tên linh kiện
•	Vị trí (trước/sau/trái/phải…)
III. CẤU TRÚC GIA CÔNG – CHỐT MÔ HÌNH
Chuẩn hoá thành 4 tầng rõ ràng.
5️⃣ LOẠI HÌNH GIA CÔNG (PROCESSING TYPE)
Nhóm gia công lớn, phản ánh bản chất kỹ thuật chính.
Ví dụ:
•	ÉP
•	SƠN
•	LẮP RÁP
📌 Dùng cho:
•	Phân loại công việc
•	Báo cáo tổng
•	Không đi sâu thao tác
6️⃣ LOẠI CÔNG ĐOẠN (PROCESS METHOD)
Phương pháp gia công trong một loại hình gia công.
Ví dụ (thuộc SƠN):
•	Phun kẹp
•	Phun tay biên
•	In sơn
•	Kẻ vẽ
•	Xóc màu
📌 Đây là cách làm kỹ thuật, chưa phải bước chi tiết
7️⃣ CÔNG ĐOẠN / BƯỚC GIA CÔNG (PROCESS STEP)
Một đơn vị thực hiện cụ thể, có thể song song hoặc lặp lại.
Ví dụ:
•	Phun kẹp – Kẹp số 1
•	Phun kẹp – Kẹp số 2
•	Phun kẹp – Kẹp số 3
•	Phun kẹp – Kẹp số 4
📌 Đây là nơi gắn:
•	Tool
•	Vật tư
•	Nhân công
•	Ghi nhận sản lượng
8️⃣ VẬT TƯ / NGUYÊN LIỆU (MATERIAL)
Thứ bị tiêu hao trong quá trình gia công.
Ví dụ:
•	Nhựa ABS (ép)
•	Sơn PMS 340C
•	Dung môi
•	Mực in
📌 Do chủ hàng cung cấp, Hải Tân quản lý tồn & hao hụt.
9️⃣ TOOL / CÔNG CỤ (TOOLING)
Thứ không tiêu hao, dùng để gia công.
Ví dụ:
•	Khuôn ép
•	Kẹp sơn
•	Đầu in
•	Jig lắp ráp
📌 Tool:
•	Thuộc sở hữu chủ hàng
•	Hải Tân quản lý sử dụng – bàn giao – hoàn trả
IV. LUỒNG NGHIỆP VỤ TỔNG THỂ (DIỄN GIẢI DẠNG SƠ ĐỒ)
1️⃣ Luồng logic nghiệp vụ
CHỦ HÀNG
   ↓
PO GIA CÔNG
   ↓
SẢN PHẨM
   ↓
LINH KIỆN
   ↓
LOẠI HÌNH GIA CÔNG (ÉP / SƠN / LẮP)
   ↓
LOẠI CÔNG ĐOẠN (Phun kẹp / In / Kẻ vẽ...)
   ↓
CÔNG ĐOẠN / BƯỚC GIA CÔNG
   ↓
[VẬT TƯ] + [TOOL]
   ↓
THỰC HIỆN SẢN XUẤT
2️⃣ Ví dụ cụ thể – SƠN THÂN TRÊN
PO A
 └─ Sản phẩm PKW4180
    └─ Linh kiện: Thân trên trước
       └─ Loại hình: SƠN
          └─ Loại công đoạn: PHUN KẸP
             ├─ Công đoạn 1: Kẹp #1 (Sơn xanh)
             ├─ Công đoạn 2: Kẹp #2 (Sơn xanh)
             ├─ Công đoạn 3: Kẹp #3 (Sơn xanh)
             └─ Công đoạn 4: Kẹp #4 (Sơn xanh)

Mỗi công đoạn:
- Dùng 1 kẹp (tool)
- Dùng 1 màu sơn (material)

V. GỢI Ý THIẾT KẾ DATABASE (Ở MỨC BA/RD HIỂU)
Không viết SQL, chỉ logic quan hệ.
Customer
  └─ PO
      └─ Product
          └─ Part
              └─ Processing_Type
                  └─ Process_Method
                      └─ Process_Step
                          ├─ Tool
                          └─ Material
📌 Bảng liên kết quan trọng:
•	PO_Part
•	Part_ProcessingType
•	ProcessingType_Method
•	Method_Step
•	Step_Material
•	Step_Tool

	Điều chỉnh quan trọng
Trong tình huống đơn hàng thì có các bước công đoạn đã ghi như vậy nhưng thực tế Hải Tân có thể cải tiến ( ví dụ một công đoạn phun kẹp có thể chuyển sang In, hoặc 2 công đoạn dùng 2 kẹp có thể ghép thành 1 kẹp ) thì bản chất sẽ có sự khác nhau giữa ghi nhận công đoạn của PO và công đoạn thực tế sản xuất , cần xây dựng phần mềm để sao cho sau khi PO sẽ gợi ý sang công đoạn thực tế ( PMC của Hải Tân có thể tự điều chỉnh công đoạn thực tế cho từng giai đoạn)
(Do đó Trong hệ thống cần tách rõ công đoạn theo PO (dùng để tính tiền, quyết toán) và công đoạn thực tế sản xuất (do PMC điều chỉnh để tối ưu).
Phần mềm cần cho phép sinh công đoạn thực tế từ PO, đồng thời cho phép PMC gộp, tách hoặc thay đổi phương pháp gia công mà không làm ảnh hưởng đến giá trị PO.
Việc quyết toán luôn dựa trên công đoạn theo PO, còn công đoạn thực tế chỉ phục vụ quản lý sản xuất, vật tư và năng suất.)
I. BẢN CHẤT VẤN ĐỀ (CHỐT RÕ)
👉 PO của chủ hàng = mô tả “công đoạn để TÍNH TIỀN”
👉 Thực tế Hải Tân = có thể tối ưu “cách LÀM” để giảm chi phí / tăng năng suất
Vì vậy BẮT BUỘC phải tách 2 lớp:
Lớp	Mục đích	Ai quyết
Công đoạn theo PO	Tính giá – quyết toán – pháp lý	Chủ hàng ↔ Hải Tân
Công đoạn thực tế sản xuất	Làm thế nào cho hiệu quả	PMC Hải Tân
❌ Không được dùng 1 khái niệm cho cả hai
II. ĐỊNH NGHĨA LẠI CÁC THÀNH TỐ (CHỐT ĐỂ BA + RD KHÔNG NHẦM)
1️⃣ Công đoạn theo PO (PO Operation / Charge Operation)
Là đơn vị gia công được dùng để tính tiền, mô tả “khối lượng công việc” theo hợp đồng, không ràng buộc cách làm thực tế.
Đặc điểm
•	Gắn với:
o	PO
o	Linh kiện
o	Loại hình gia công
•	Có:
o	Tên (ví dụ: phun kẹp, in sơn…)
o	Số lần (加工次数)
o	Đơn giá
•	KHÔNG gắn tool, máy, nhân sự
👉 Ví dụ:
•	Phun kẹp × 4 công đoạn
•	Đơn giá: 217 VND
2️⃣ Công đoạn thực tế sản xuất (Production Operation)
Là cách Hải Tân thực sự triển khai sản xuất, có thể khác với PO, miễn là đảm bảo chất lượng & tiến độ.
Đặc điểm
•	Do PMC cấu hình
•	Gắn với:
o	Máy
o	Tool
o	Nhân sự
o	Vật tư
•	Có thể:
o	Gộp
o	Tách
o	Thay phương pháp
👉 Ví dụ:
•	PO ghi: Phun kẹp × 4
•	Thực tế:
o	Gộp 2 kẹp → 1 kẹp lớn
o	Hoặc chuyển 1 kẹp sang in
📌 Quyết toán vẫn theo PO, không theo thực tế
3️⃣ MỐI QUAN HỆ GIỮA 2 LỚP (CỰC KỲ QUAN TRỌNG)
PO Operation (tính tiền)
        ↓ (mapping / phân bổ)
Production Operation (thực tế)
👉 Một PO Operation
•	Có thể map tới:
o	1 Production Operation
o	hoặc N Production Operation
👉 N Production Operation
•	Có thể quy về:
o	1 PO Operation
o	hoặc nhiều PO Operation (nếu cần tracking)
III. LUỒNG NGHIỆP VỤ ĐỀ XUẤT (PMC CAN THIỆP ĐƯỢC)
Giai đoạn 1 – Nhập PO (chuẩn hợp đồng)
1.	Nhập PO từ chủ hàng
2.	Nhập:
o	Linh kiện
o	Loại hình gia công
o	Công đoạn theo PO
	Tên
	Số lần
	Đơn giá
3.	Hệ thống lưu PO Operation
👉 Đóng băng phần tính tiền
Giai đoạn 2 – Gợi ý công đoạn thực tế (KEY FEATURE)
🔹 Hệ thống làm gì?
Sau khi có PO, hệ thống:
•	Dựa vào:
o	Loại hình gia công
o	Công đoạn theo PO
o	Lịch sử các PO trước
•	Sinh ra “kế hoạch công đoạn thực tế mặc định”
👉 Ví dụ:
•	PO: Phun kẹp × 4
•	Gợi ý:
o	Phun kẹp – Kẹp #1
o	Phun kẹp – Kẹp #2
o	Phun kẹp – Kẹp #3
o	Phun kẹp – Kẹp #4
Giai đoạn 3 – PMC điều chỉnh công đoạn thực tế
PMC có quyền:
•	Gộp công đoạn
•	Tách công đoạn
•	Thay phương pháp
Ví dụ 1 – Gộp
•	PO: Phun kẹp × 4
•	Thực tế:
o	Phun kẹp – Kẹp lớn #A (làm thay 2)
o	Phun kẹp – Kẹp lớn #B (làm thay 2)
Ví dụ 2 – Thay phương pháp
•	PO: Phun kẹp × 4
•	Thực tế:
o	In sơn × 2
o	Phun kẹp × 2
📌 Phần mềm chỉ cần yêu cầu PMC khai báo mapping, KHÔNG cần can thiệp giá.
Giai đoạn 4 – Thực hiện & ghi nhận
•	Ghi nhận:
o	Sản lượng
o	Tool sử dụng
o	Vật tư tiêu hao
•	Ghi nhận theo Production Operation
Giai đoạn 5 – Quy đổi & quyết toán
•	Hệ thống quy đổi ngược:
o	Production Operation → PO Operation
•	Đảm bảo:
o	Tổng sản lượng PO Operation = sản lượng giao
o	Tiền = theo PO
👉 PMC tối ưu được, kế toán vẫn đúng
IV. GỢI Ý THIẾT KẾ DATABASE (LOGIC, KHÔNG SQL)
1️⃣ Các bảng cốt lõi
PO_Operation (Công đoạn theo PO)
•	PO_ID
•	Part_ID
•	Processing_Type
•	Operation_Name
•	Charge_Count
•	Unit_Price
Production_Operation (Công đoạn thực tế)
•	Production_Operation_ID
•	PO_ID
•	Part_ID
•	Method
•	Tool_ID
•	Machine_ID
•	Material_ID
Mapping_PO_Production
•	PO_Operation_ID
•	Production_Operation_ID
•	Allocation_Ratio (tỷ lệ quy đổi)
📌 Bảng mapping là chìa khóa
V. SƠ ĐỒ TỔNG QUÁT (DỄ HIỂU)
PO
 └─ Linh kiện
    └─ PO Công đoạn (tính tiền)
         ├─ Mapping → Công đoạn thực tế A
         ├─ Mapping → Công đoạn thực tế B
         └─ Mapping → Công đoạn thực tế C

