Integrate PO detail view with versioning and lock logic.

UI behavior:
- Display PO data in read-only mode.
- If PO version status = LOCKED:
  - Disable edit buttons (if any).
- Do not display:
  - BOM
  - Tool
  - Machine
  - Production information

Data source:
- Use APPROVED PO version for display if exists.
- Otherwise, show latest DRAFT version.
