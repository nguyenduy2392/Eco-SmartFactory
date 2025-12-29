Implement Process BOM configuration.

Business meaning:
- Process BOM defines material consumption per 1 PCS of a part.
- BOM belongs to HOW TO MAKE, not HOW TO CHARGE.
- BOM is independent from PO and pricing.

BOM scope:
- Linked to:
  - Part
  - Processing Type (EP_NHUA / PHUN_IN / LAP_RAP)

Data rules:
- One ACTIVE BOM per (part + processing type).
- BOM must have versioning.
- BOM must contain at least one material line.

BOM detail fields:
- material_code
- qty_per_unit
- scrap_rate (>= 0)
- uom
- process_step (for traceability)

Behavior:
- Creating new BOM version automatically sets old version to INACTIVE.
- Editing ACTIVE BOM is forbidden; create new version instead.
- BOM changes must NOT affect PO pricing.

If UI for BOM already exists:
- Adapt logic to existing fields.
- Do not add mandatory UI fields.
