Bind Processing PO import logic to existing Import PO screen.

UI behavior:
- User selects processing type (EP_NHUA / PHUN_IN / LAP_RAP).
- User uploads Excel file.
- UI sends file + processing type to backend.

Backend response handling:
- If import fails:
  - Display error list with row number and error reason.
- If import succeeds:
  - Display generated PO ID.
  - Display PO version = V0.
  - Display PO status = DRAFT.

Restrictions:
- Disable all edit actions on imported data.
- Do not allow manual PO creation.
- Do not allow price editing.
