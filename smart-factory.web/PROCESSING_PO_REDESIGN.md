# Processing PO UI Redesign - Phase 1

## Tổng quan
Đã xóa hoàn toàn phần PO cũ và thiết kế lại UI thông minh theo các yêu cầu nghiệp vụ trong các file prompt. UI mới tập trung vào 3 chức năng chính của Phase 1:
1. **Processing PO Import**
2. **Process BOM Configuration**
3. **Availability Check**

---

## Các thay đổi đã thực hiện

### 1. XÓA PHẦN CŨ

#### Components đã xóa:
- ✅ `purchase-orders/po-list` (component, html, scss)
- ✅ `purchase-orders/po-detail` (component, html, scss)
- ✅ `purchase-orders/product-detail` (component, html, scss)
- ✅ `purchase-orders/part-detail` (component, html, scss)

#### Routes đã xóa:
- ✅ Route `/purchase-orders` và tất cả child routes cũ

---

### 2. CẤU TRÚC MỚI

#### A. Models mới (`src/app/models/`)

**`purchase-order.interface.ts` (đã cập nhật)**
```typescript
- PurchaseOrder: Processing PO - FINANCIAL BASELINE only
  - processingType: EP_NHUA | PHUN_IN | LAP_RAP
  - version: V0, V1, V2, ...
  - status: DRAFT | APPROVED_FOR_PMC | LOCKED
  
- POOperation: From NHAP_PO sheet (pricing, revenue, settlement)
  - productCode, partCode, contractQty, unitPrice
  
- POMaterialBaseline: From NHAP_NGUYEN_VAT_LIEU sheet (availability check only)
  - materialCode, quantity
  
- ImportPORequest, ImportPOResponse: Excel import
- ClonePOVersionRequest: Version cloning
- AvailabilityCheckRequest, AvailabilityCheckResult
```

**`process-bom.interface.ts` (mới)**
```typescript
- ProcessBOM: Material consumption per 1 PCS of part
  - partId, processingType, version
  - status: ACTIVE | INACTIVE
  - bomDetails: BOMDetail[]
  
- BOMDetail: Material line
  - materialCode, qtyPerUnit, scrapRate, uom, processStep
```

#### B. Services mới (`src/app/services/`)

**`purchase-order.service.ts` (đã cập nhật)**
- `getAll()`: Lấy danh sách Processing PO
- `getById()`: Lấy chi tiết PO (APPROVED version if exists)
- `getVersions()`: Lấy tất cả versions của PO
- `importFromExcel()`: Import PO từ Excel - ONLY way to create
- `cloneVersion()`: Clone PO để tạo version mới
- `approveForPMC()`: Approve PO version cho PMC
- `delete()`: Xóa PO (only DRAFT)

**`process-bom.service.ts` (mới)**
- `getAll()`: Lấy danh sách Process BOM
- `getById()`: Lấy chi tiết BOM
- `getActiveBOM()`: Lấy ACTIVE BOM cho part + processing type
- `getBOMHistory()`: Lấy BOM history
- `create()`: Tạo BOM version mới
- `delete()`: Xóa BOM (only INACTIVE)

**`availability-check.service.ts` (mới)**
- `checkAvailability()`: Kiểm tra khả dụng NVL
  - Input: PO ID (APPROVED), Planned quantity
  - Output: PASS | FAIL | WARNING
  - Does NOT change inventory, create production data, or affect pricing

#### C. Components mới (`src/app/components/`)

**1. Processing PO List** (`processing-po/po-list/`)
- **Chức năng:**
  - Hiển thị danh sách Processing PO với filters
  - Import PO từ Excel (ONLY way to create)
  - Xóa PO (only DRAFT status)
  
- **Import Dialog:**
  - User chọn processing type (EP_NHUA/PHUN_IN/LAP_RAP)
  - User upload Excel file (must have 2 sheets: NHAP_PO, NHAP_NGUYEN_VAT_LIEU)
  - Show validation errors with row number if import fails
  - Show PO ID, version = V0, status = DRAFT if success

- **UI Features:**
  - Filter by: status, processingType, customerId
  - Search by: poNumber, customerName
  - Table with sorting, pagination
  - Status badges, processing type tags
  - Action buttons: View, Delete (conditional)

**2. Processing PO Detail** (`processing-po/po-detail/`)
- **Chức năng:**
  - Hiển thị PO data ở read-only mode
  - Version management (V0, V1, V2, ...)
  - Clone version, Approve for PMC
  - Integrated Availability Check
  
- **Version Management:**
  - Display all versions with selector
  - Show APPROVED version by default if exists
  - Lock version after approval (no edits)
  - Only ONE version can be APPROVED at a time

- **Tabs:**
  1. **Thông tin chung**: PO metadata
  2. **Thao tác PO (Tính phí)**: From NHAP_PO sheet
     - productCode, partCode, contractQty, unitPrice
     - Does NOT contain: Tool, Machine, BOM, Production logic
  3. **Nguyên vật liệu baseline**: From NHAP_NGUYEN_VAT_LIEU sheet
     - Used ONLY for availability check
     - Does NOT affect pricing or settlement

- **Actions:**
  - Clone version: Create new DRAFT version
  - Approve for PMC: Set to APPROVED_FOR_PMC, lock version
  - Check availability: Only for APPROVED versions

**3. Process BOM** (`process-bom/`)
- **Chức năng:**
  - Quản lý Process BOM (material consumption per PCS)
  - BOM độc lập với PO và pricing
  - Version history, ACTIVE/INACTIVE status
  
- **Features:**
  - Filter by: part, processingType, status
  - View BOM details with material list
  - Create new BOM version
  - Delete BOM (only INACTIVE)

- **Create BOM:**
  - Select part + processing type
  - Add material lines (materialCode, qtyPerUnit, scrapRate, uom)
  - At least one material line required
  - Creating new version auto-sets old to INACTIVE

- **Business Rules:**
  - One ACTIVE BOM per (part + processing type)
  - ACTIVE BOM is read-only
  - Editing requires creating new version
  - BOM changes do NOT affect PO pricing

**4. Availability Check** (`availability-check/`)
- **Chức năng:**
  - Standalone page cho Availability Check
  - Quyết định xem PMC có được phép plan production hay không
  
- **Input:**
  - PO selection (only APPROVED versions)
  - Planned production quantity

- **Calculation:**
  ```
  For each material:
    Required_Qty = Planned_Qty × BOM_Qty × (1 + Scrap_Rate)
    Available_Qty = Inventory_Qty + PO_Material_Baseline_Qty
    Shortage = Required_Qty - Available_Qty
  
  Result:
    - Shortage > 0 → FAIL (CRITICAL)
    - Available_Qty < Required_Qty × 1.1 → WARNING
    - Else → PASS
  ```

- **Output:**
  - Overall status: PASS / FAIL / WARNING with visual indicators
  - Summary cards: Total required vs Total available
  - Material-level table with shortage details
  - Action messages with recommendations

- **Hard Rules:**
  - Does NOT change inventory
  - Does NOT create production data
  - Does NOT affect pricing

---

### 3. ROUTES MỚI (`src/app/app.routes.ts`)

```typescript
{
  path: 'processing-po',
  children: [
    { path: '', component: POListComponent },
    { path: ':id', component: PODetailComponent }
  ]
},
{
  path: 'process-bom',
  component: ProcessBOMComponent
},
{
  path: 'availability-check',
  component: AvailabilityCheckComponent
}
```

---

## Nguyên tắc thiết kế (theo prompt)

### 1. Processing PO
- ✅ PO là FINANCIAL BASELINE - định nghĩa price, quantity, settlement only
- ✅ PO KHÔNG định nghĩa cách sản xuất
- ✅ PO chỉ có thể tạo bằng import Excel (forbidden manual creation)
- ✅ Excel phải có đúng 2 sheets: NHAP_PO, NHAP_NGUYEN_VAT_LIEU
- ✅ Imported data là read-only, không cho edit

### 2. PO Versioning
- ✅ V0 là version gốc từ import
- ✅ Versions immutable sau khi locked
- ✅ Chỉ ONE version được APPROVED at a time
- ✅ Availability check chỉ dùng APPROVED version

### 3. Process BOM
- ✅ BOM belongs to "HOW TO MAKE", not "HOW TO CHARGE"
- ✅ BOM độc lập với PO và pricing
- ✅ One ACTIVE BOM per (part + processing type)
- ✅ ACTIVE BOM read-only, edit phải tạo version mới
- ✅ BOM changes KHÔNG affect PO pricing

### 4. Availability Check
- ✅ Chỉ check với APPROVED PO
- ✅ Sử dụng ACTIVE BOM cho calculation
- ✅ KHÔNG touch inventory
- ✅ KHÔNG generate production data
- ✅ KHÔNG affect pricing

---

## Phase 1 Scope

### ✅ IN SCOPE (Đã hoàn thành)
1. Processing PO Import
2. Process BOM Configuration
3. Availability Check

### ❌ OUT OF SCOPE
- Production planning
- Production execution
- Tool usage tracking
- Cost variance

---

## UI/UX Highlights

### Design System
- **PrimeNG Components**: Modern, responsive UI
- **Color Coding**: 
  - DRAFT: Yellow (warning)
  - APPROVED: Green (success)
  - LOCKED: Blue (info)
  - PASS: Green gradient
  - FAIL: Red gradient
  - WARNING: Orange gradient

### User Experience
- **Validation**: Real-time validation với detailed error messages
- **Feedback**: Toast notifications cho mọi action
- **Confirmation**: Dialog xác nhận cho destructive actions
- **Progressive Disclosure**: Sử dụng tabs, dialogs để organize thông tin
- **Responsive**: Desktop-first, responsive down to mobile
- **Accessibility**: Icons, labels, tooltips rõ ràng

### Performance
- **Pagination**: Tất cả tables có pagination
- **Filtering**: Client-side + server-side filters
- **Loading States**: Spinners, loading buttons
- **Lazy Loading**: Components lazy loaded trong routes

---

## Lưu ý khi implement Backend

### 1. Excel Import Validation
- Validate sheet names: NHAP_PO, NHAP_NGUYEN_VAT_LIEU
- Validate required columns
- Validate each row: product_code, part_code, contract_qty > 0, unit_price > 0
- Return error list with row number and reason if ANY error
- Transaction: ALL or NOTHING

### 2. Version Management
- Ensure only ONE APPROVED version at a time
- Lock version after approval
- Track originalPOId for version chain

### 3. BOM Management
- Auto-set old ACTIVE BOM to INACTIVE when creating new
- Validate at least one material line
- Ensure scrapRate >= 0

### 4. Availability Check
- Use ACTIVE BOM only
- Use APPROVED PO only
- Calculate với formula đã define
- Return detailed material-level results

---

## Testing Checklist

### PO Import
- [ ] Import with valid Excel → Success, V0, DRAFT
- [ ] Import with invalid Excel → Show errors with row numbers
- [ ] Import without required sheets → Error
- [ ] Import with empty rows → Error

### PO Versioning
- [ ] Clone PO → New version (V1, V2, ...), DRAFT status
- [ ] Approve PO → Status = APPROVED_FOR_PMC, locked
- [ ] Try approve when another version already approved → Error
- [ ] Delete DRAFT → Success
- [ ] Delete LOCKED → Error

### Process BOM
- [ ] Create BOM → New version, ACTIVE, old version INACTIVE
- [ ] Create without material → Error
- [ ] View ACTIVE BOM → Read-only
- [ ] Delete INACTIVE BOM → Success
- [ ] Delete ACTIVE BOM → Error

### Availability Check
- [ ] Check with APPROVED PO, sufficient materials → PASS
- [ ] Check with APPROVED PO, insufficient materials → FAIL
- [ ] Check with APPROVED PO, low materials → WARNING
- [ ] Check with DRAFT PO → Error
- [ ] Check without ACTIVE BOM → Error

---

## File Structure Summary

```
smart-factory.web/src/app/
├── models/
│   ├── purchase-order.interface.ts (updated)
│   └── process-bom.interface.ts (new)
│
├── services/
│   ├── purchase-order.service.ts (updated)
│   ├── process-bom.service.ts (new)
│   └── availability-check.service.ts (new)
│
├── components/
│   ├── processing-po/
│   │   ├── po-list/ (new)
│   │   │   ├── po-list.component.ts
│   │   │   ├── po-list.component.html
│   │   │   └── po-list.component.scss
│   │   └── po-detail/ (new)
│   │       ├── po-detail.component.ts
│   │       ├── po-detail.component.html
│   │       └── po-detail.component.scss
│   │
│   ├── process-bom/ (new)
│   │   ├── process-bom.component.ts
│   │   ├── process-bom.component.html
│   │   └── process-bom.component.scss
│   │
│   └── availability-check/ (new)
│       ├── availability-check.component.ts
│       ├── availability-check.component.html
│       └── availability-check.component.scss
│
└── app.routes.ts (updated)
```

---

## Next Steps

1. **Backend Implementation**: Implement các API endpoints theo spec
2. **Testing**: Unit tests, integration tests, E2E tests
3. **Data Migration**: Migrate data cũ (nếu có) sang structure mới
4. **Documentation**: API documentation, user manual
5. **Training**: Train users về workflow mới
6. **Phase 2**: Production planning, execution (out of current scope)

---

## Conclusion

✅ Đã hoàn thành việc xóa phần PO cũ và thiết kế lại UI thông minh theo đúng yêu cầu nghiệp vụ trong các file prompt. UI mới:
- Tập trung vào Phase 1 scope
- Follow strict business rules
- Modern, responsive design
- Clear separation of concerns
- Ready for backend integration


