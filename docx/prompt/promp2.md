Implement business logic for Processing PO.

Business meaning:
- Processing PO is a FINANCIAL BASELINE.
- PO defines price, quantity, and settlement only.
- PO does NOT define how production is executed.

Creation rules:
- PO can ONLY be created by importing Excel.
- Manual PO creation is forbidden.

Processing types:
- EP_NHUA
- PHUN_IN
- LAP_RAP

Excel contract:
- Each file MUST contain exactly 2 sheets:
  1. NHAP_PO
  2. NHAP_NGUYEN_VAT_LIEU

NHAP_PO rules:
- Represents chargeable operations (PO Operation).
- Used for pricing, revenue, and settlement.
- Must not contain:
  - Tool
  - Machine
  - BOM
  - Production logic

NHAP_NGUYEN_VAT_LIEU rules:
- Represents customer-committed materials.
- Used ONLY for availability check.
- Must not affect pricing or settlement.

Import behavior:
1. Validate sheet names.
2. Validate required columns.
3. Validate each row:
   - product_code not null
   - part_code not null
   - contract_qty > 0
   - unit_price > 0
4. If ANY error:
   - Reject entire import
   - Return error list with row number and reason
5. If valid:
   - Create PO
   - Set version = V0
   - Set status = DRAFT
   - Save PO Operation records
   - Save PO Material Baseline records

Do NOT:
- Calculate BOM
- Touch inventory
- Generate production data
