Implement PO versioning logic.

Business rules:
- Each PO can have multiple versions: V0, V1, V2...
- V0 is the original imported version.
- Versions are immutable once locked.

Rules:
1. Cloning a PO creates a new version:
   - Same data
   - Status = DRAFT
2. Only ONE version can be APPROVED_FOR_PMC.
3. Once approved:
   - Version becomes LOCKED
   - No further edits allowed

System behavior:
- Availability check can ONLY use APPROVED version.
- UI behavior must remain unchanged.
