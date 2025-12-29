Bind Availability Check logic to existing Availability Check screen.

UI inputs:
- PO selection
- Planned production quantity

UI outputs:
- Overall availability status: PASS / FAIL / WARNING.
- Material-level result table:
  - Material code
  - Required quantity
  - Available quantity
  - Shortage
  - Severity indicator

Behavior:
- If status = FAIL:
  - Disable "Create Production Plan" button.
- If status = WARNING:
  - Allow action but show warning banner.

Restrictions:
- Do not auto deduct inventory.
- Do not create production orders.
