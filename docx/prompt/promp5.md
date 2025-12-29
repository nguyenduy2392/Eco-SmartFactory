Implement Availability Check logic.

Purpose:
- Decide whether PMC is allowed to plan production.

Input:
- PO ID
- Planned production quantity

Allowed PO:
- Only APPROVED PO version.

Data sources:
- PO Operations (contract quantity)
- Process BOM (ACTIVE)
- PO Material Baseline (NHAP_NGUYEN_VAT_LIEU)
- Inventory on-hand quantity

Calculation:
For each material:
Required_Qty = Planned_Qty × BOM_Qty × (1 + Scrap_Rate)
Available_Qty = Inventory_Qty + PO_Material_Baseline_Qty
Shortage = Required_Qty - Available_Qty

Result rules:
- Shortage > 0 → FAIL (CRITICAL)
- Available_Qty < Required_Qty × 1.1 → WARNING
- Else → PASS

Output:
- Overall status: PASS / FAIL
- Per material:
  - material_code
  - required_qty
  - available_qty
  - shortage
  - severity

Hard rules:
- Availability check MUST NOT:
  - Change inventory
  - Create production data
  - Affect pricing
