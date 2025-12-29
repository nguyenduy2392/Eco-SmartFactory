You must integrate new business logic with existing UI.

Rules:
- Reuse existing screens and buttons.
- Do not change screen navigation.
- Do not rename UI fields.
- If UI already has "Check Availability" action:
  - Bind new logic behind it.
- If UI lacks some input:
  - Auto-derive from backend.
- If UI lacks some output:
  - Return additional fields via API without breaking UI.

Priority:
- Backward compatibility > Perfect UI.
