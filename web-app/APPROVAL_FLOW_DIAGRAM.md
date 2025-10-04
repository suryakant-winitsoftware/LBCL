# Approval System Flow Diagram

## Complete End-to-End Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         APPROVAL SYSTEM ARCHITECTURE                         │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 1: USER LOGIN                                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  User: John Smith                                                           │
│  ├─ userId: EMP001                                                          │
│  ├─ role: MERCHANDISER                                                      │
│  ├─ supervisorId: EMP050                                                    │
│  ├─ branchId: BR001                                                         │
│  └─ email: john@multiplex.com                                               │
│                                                                             │
│  Session Stored in Browser ✅                                               │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 2: USER CREATES RETURN ORDER (Example)                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Return Order Created:                                                      │
│  ├─ returnOrderId: RO-12345                                                 │
│  ├─ createdBy: EMP001 (from session)                                        │
│  ├─ items: 50 units of Shampoo                                              │
│  ├─ totalAmount: $5,000                                                     │
│  ├─ reason: "Damaged in transit"                                            │
│  └─ images: [damage_photo1.jpg]                                             │
│                                                                             │
│  ⚠️  Needs Approval → System starts approval workflow                       │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 3: SYSTEM IDENTIFIES APPROVAL RULE                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Query: ApprovalRuleMapping Table                                           │
│  ┌────────────────────────────────────────┐                                │
│  │ SELECT rule_id                         │                                │
│  │ FROM ApprovalRuleMapping               │                                │
│  │ WHERE type = 'ReturnOrder'             │                                │
│  │   AND type_code = 'ALL'                │                                │
│  └────────────────────────────────────────┘                                │
│                                                                             │
│  Result: rule_id = 5 (Return Order Approval Rule) ✅                        │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 4: SYSTEM GETS APPROVAL HIERARCHY                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Query: approvalhierarchy Table                                             │
│  ┌────────────────────────────────────────┐                                │
│  │ SELECT *                               │                                │
│  │ FROM approvalhierarchy                 │                                │
│  │ WHERE ruleId = 5                       │                                │
│  │ ORDER BY level                         │                                │
│  └────────────────────────────────────────┘                                │
│                                                                             │
│  Result: Approval Chain                                                     │
│  ┌────────────────────────────────────────┐                                │
│  │ Level 1: Role = EXECUTIVE              │                                │
│  │ Level 2: Role = SUPERVISOR             │                                │
│  │ Level 3: User = EMP100 (Finance Head)  │                                │
│  └────────────────────────────────────────┘                                │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 5: SYSTEM FINDS ACTUAL APPROVERS (Walking Up Hierarchy)               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Start: EMP001 (John - Merchandiser)                                        │
│         ↓                                                                   │
│  Query: users.supervisorId = ?                                              │
│         ↓                                                                   │
│  Level 1: EMP050 (Mike - EXECUTIVE) ✅ MATCH!                               │
│         ├─ Name: Mike Johnson                                               │
│         ├─ Role: EXECUTIVE                                                  │
│         ├─ Email: mike@multiplex.com                                        │
│         └─ supervisorId: EMP070                                             │
│         ↓                                                                   │
│  Level 2: EMP070 (Sarah - SUPERVISOR) ✅ MATCH!                             │
│         ├─ Name: Sarah Williams                                             │
│         ├─ Role: SUPERVISOR                                                 │
│         ├─ Email: sarah@multiplex.com                                       │
│         └─ supervisorId: EMP100                                             │
│         ↓                                                                   │
│  Level 3: EMP100 (David - FINANCE_HEAD) ✅ MATCH!                           │
│         ├─ Name: David Lee                                                  │
│         ├─ Role: FINANCE_HEAD                                               │
│         ├─ Email: david@multiplex.com                                       │
│         └─ supervisorId: NULL (Top level)                                   │
│                                                                             │
│  Approvers Identified:                                                      │
│  1️⃣  Mike Johnson (EMP050) - Executive                                      │
│  2️⃣  Sarah Williams (EMP070) - Supervisor                                   │
│  3️⃣  David Lee (EMP100) - Finance Head                                      │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 6: SYSTEM CREATES APPROVAL REQUEST                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Insert into: approvalrequest Table                                         │
│  ┌────────────────────────────────────────┐                                │
│  │ id: 123                                │                                │
│  │ ruleId: 5                              │                                │
│  │ requesterId: EMP001                    │                                │
│  │ status: Pending                        │                                │
│  │ approverId: EMP050 (Level 1)           │                                │
│  │ createdOn: 2025-10-03 10:30:00         │                                │
│  └────────────────────────────────────────┘                                │
│                                                                             │
│  Insert into: AllApprovalRequest Table                                      │
│  ┌────────────────────────────────────────┐                                │
│  │ linkedItemType: ReturnOrder            │                                │
│  │ linkedItemUID: RO-12345                │                                │
│  │ requestID: 123                         │                                │
│  │ approvalUserDetail: JSON{...}          │                                │
│  └────────────────────────────────────────┘                                │
│                                                                             │
│  Insert into: approvalstatus Table                                          │
│  ┌────────────────────────────────────────┐                                │
│  │ approvalRequestId: 123                 │                                │
│  │ approverId: EMP050                     │                                │
│  │ status: Pending                        │                                │
│  └────────────────────────────────────────┘                                │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 7: NOTIFICATION SENT TO FIRST APPROVER                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  📧 Email to: mike@multiplex.com                                            │
│  📱 Push Notification to: Mike Johnson                                      │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │ 🔔 New Approval Request                                             │  │
│  │                                                                     │  │
│  │ Request ID: #123                                                    │  │
│  │ Type: Return Order                                                  │  │
│  │ Requester: John Smith (Merchandiser)                                │  │
│  │ Amount: $5,000                                                      │  │
│  │ Item: RO-12345                                                      │  │
│  │ Reason: Damaged in transit                                          │  │
│  │                                                                     │  │
│  │ [View Details] [Approve] [Reject]                                   │  │
│  └─────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 8: EXECUTIVE (MIKE) LOGS INTO WEB-APP                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Mike navigates to: /administration/approvals                               │
│                                                                             │
│  Query executed:                                                            │
│  ┌────────────────────────────────────────┐                                │
│  │ SELECT ar.*, aar.*                     │                                │
│  │ FROM approvalrequest ar                │                                │
│  │ JOIN AllApprovalRequest aar            │                                │
│  │   ON ar.id = aar.requestID             │                                │
│  │ WHERE ar.approverId = 'EMP050'         │  ← Mike's ID from login        │
│  │   AND ar.status = 'Pending'            │                                │
│  └────────────────────────────────────────┘                                │
│                                                                             │
│  Mike's Dashboard Shows:                                                    │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │ 📊 Approval Dashboard                                               │  │
│  │ ┌─────────────┬─────────────┬─────────────┬─────────────┐          │  │
│  │ │  Pending    │  Approved   │  Rejected   │  Total      │          │  │
│  │ │     3       │     45      │     2       │    50       │          │  │
│  │ └─────────────┴─────────────┴─────────────┴─────────────┘          │  │
│  │                                                                     │  │
│  │ Pending Approvals (3):                                              │  │
│  │ ┌───────────────────────────────────────────────────────────────┐  │  │
│  │ │ 🟠 Return Order #RO-12345                           [VIEW]    │  │  │
│  │ │    By: John Smith | Amount: $5,000                  [APPROVE] │  │  │
│  │ │    Created: Oct 3, 10:30 AM                         [REJECT]  │  │  │
│  │ └───────────────────────────────────────────────────────────────┘  │  │
│  │ ┌───────────────────────────────────────────────────────────────┐  │  │
│  │ │ 🟠 Initiative #INIT-456                              [VIEW]    │  │  │
│  │ │    By: Mary Johnson | Type: Display Setup           [APPROVE] │  │  │
│  │ │    Created: Oct 3, 09:15 AM                         [REJECT]  │  │  │
│  │ └───────────────────────────────────────────────────────────────┘  │  │
│  └─────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 9: MIKE CLICKS "VIEW DETAILS" ON RO-12345                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Navigate to: /administration/approvals/123                                 │
│                                                                             │
│  Mike sees:                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │ ← Back to Approvals                    [APPROVE] [REJECT] [REASSIGN]│  │
│  │                                                                     │  │
│  │ Approval Details - Request #123                                     │  │
│  │                                                                     │  │
│  │ ┌─────────────────────────────────────────────────────────────────┐│  │
│  │ │ 📋 Request Information                                          ││  │
│  │ │ ────────────────────────────────────────────────────────────────││  │
│  │ │ Status: 🟠 Pending                                              ││  │
│  │ │ Type: Return Order                                               ││  │
│  │ │ Item: RO-12345                                                   ││  │
│  │ │ Requester: John Smith (Merchandiser)                             ││  │
│  │ │ Current Approver: Mike Johnson (Executive)                       ││  │
│  │ │ Created: Oct 3, 2025 10:30:00                                    ││  │
│  │ │                                                                  ││  │
│  │ │ Return Details:                                                  ││  │
│  │ │ • 50 units of Shampoo X Brand                                    ││  │
│  │ │ • Total Amount: $5,000                                           ││  │
│  │ │ • Reason: Damaged in transit                                     ││  │
│  │ │ • Reference Invoice: INV-2024-1234                               ││  │
│  │ │ • Images: [damage_photo1.jpg] 📷                                 ││  │
│  │ └─────────────────────────────────────────────────────────────────┘│  │
│  │                                                                     │  │
│  │ ┌─────────────────────────────────────────────────────────────────┐│  │
│  │ │ 🔄 Approval Hierarchy                                           ││  │
│  │ │ ────────────────────────────────────────────────────────────────││  │
│  │ │ ● Level 1: Mike Johnson (Executive) ← YOU ARE HERE              ││  │
│  │ │ ○ Level 2: Sarah Williams (Supervisor)                           ││  │
│  │ │ ○ Level 3: David Lee (Finance Head)                              ││  │
│  │ └─────────────────────────────────────────────────────────────────┘│  │
│  │                                                                     │  │
│  │ ┌─────────────────────────────────────────────────────────────────┐│  │
│  │ │ 📜 Approval History                                             ││  │
│  │ │ ────────────────────────────────────────────────────────────────││  │
│  │ │ ⏱️  Created - Oct 3, 10:30 AM                                    ││  │
│  │ │     By: John Smith                                               ││  │
│  │ │     Status: Submitted for approval                               ││  │
│  │ └─────────────────────────────────────────────────────────────────┘│  │
│  └─────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 10: MIKE CLICKS "APPROVE"                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Modal Opens:                                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │ ✅ Approve Request                                                   │  │
│  │ ─────────────────────────────────────────────────────────────────   │  │
│  │                                                                     │  │
│  │ Request ID: #123                                                    │  │
│  │ Type: Return Order                                                  │  │
│  │ Requester: John Smith                                               │  │
│  │                                                                     │  │
│  │ Comments (Optional):                                                │  │
│  │ ┌───────────────────────────────────────────────────────────────┐  │  │
│  │ │ Products verified as damaged. Approved for return.            │  │  │
│  │ │                                                               │  │  │
│  │ └───────────────────────────────────────────────────────────────┘  │  │
│  │                                                                     │  │
│  │ ℹ️ Note: Approving will move this to Level 2 (Sarah Williams)      │  │
│  │                                                                     │  │
│  │                                 [CANCEL]  [APPROVE] ✅              │  │
│  └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  Mike clicks "APPROVE" ✅                                                   │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 11: SYSTEM PROCESSES APPROVAL                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1️⃣  Insert into ApprovalLogs:                                              │
│  ┌────────────────────────────────────────┐                                │
│  │ requestId: 123                         │                                │
│  │ approverId: EMP050 (Mike)              │                                │
│  │ level: 1                               │                                │
│  │ status: Approved                       │                                │
│  │ comments: "Products verified..."       │                                │
│  │ createdOn: 2025-10-03 11:00:00         │                                │
│  └────────────────────────────────────────┘                                │
│                                                                             │
│  2️⃣  Update approvalrequest:                                                │
│  ┌────────────────────────────────────────┐                                │
│  │ approverId: EMP070 (Sarah - Level 2)   │  ← Move to next level          │
│  │ status: Pending                        │                                │
│  │ modifiedBy: EMP050                     │                                │
│  │ modifiedOn: 2025-10-03 11:00:00        │                                │
│  └────────────────────────────────────────┘                                │
│                                                                             │
│  3️⃣  Update approvalstatus:                                                 │
│  ┌────────────────────────────────────────┐                                │
│  │ approverId: EMP070 (Sarah)             │  ← Next approver               │
│  │ status: Pending                        │                                │
│  └────────────────────────────────────────┘                                │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 12: NOTIFICATION TO LEVEL 2 APPROVER (SARAH)                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  📧 Email to: sarah@multiplex.com                                           │
│  📱 Push Notification                                                        │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │ 🔔 Approval Forwarded to You                                        │  │
│  │                                                                     │  │
│  │ Request #123 - Return Order RO-12345                                │  │
│  │ Approved by: Mike Johnson (Level 1)                                 │  │
│  │ Comments: "Products verified as damaged. Approved for return."      │  │
│  │                                                                     │  │
│  │ Awaiting your approval (Level 2)                                    │  │
│  │                                                                     │  │
│  │ [View Details] [Approve] [Reject]                                   │  │
│  └─────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 13: SARAH APPROVES (LEVEL 2)                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Sarah logs in → Sees pending approval → Approves                          │
│                                                                             │
│  Same process:                                                              │
│  • ApprovalLog created (Level 2, Approved)                                  │
│  • approvalrequest updated (approverId → EMP100, Level 3)                   │
│  • Notification sent to David (Finance Head)                                │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 14: DAVID APPROVES (LEVEL 3 - FINAL)                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  David (Finance Head) approves → FINAL APPROVAL                             │
│                                                                             │
│  System Processing:                                                         │
│  1️⃣  Insert ApprovalLog (Level 3, Approved)                                 │
│  2️⃣  Update approvalrequest.status = "Approved"                             │
│  3️⃣  Execute UpdateChangesInMainTable:                                      │
│     • Process return order in ERP                                           │
│     • Generate credit note                                                  │
│     • Update inventory                                                      │
│  4️⃣  Notify requester (John)                                                │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 15: REQUESTER (JOHN) GETS NOTIFICATION                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  📧 Email to: john@multiplex.com                                            │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │ ✅ Your Request Has Been Approved                                   │  │
│  │                                                                     │  │
│  │ Request #123 - Return Order RO-12345                                │  │
│  │ Final Status: APPROVED ✅                                            │  │
│  │                                                                     │  │
│  │ Approval Timeline:                                                  │  │
│  │ ✅ Level 1: Mike Johnson (Executive) - Oct 3, 11:00 AM             │  │
│  │ ✅ Level 2: Sarah Williams (Supervisor) - Oct 3, 14:30 PM          │  │
│  │ ✅ Level 3: David Lee (Finance Head) - Oct 3, 16:00 PM             │  │
│  │                                                                     │  │
│  │ Credit Note #CN-2025-001 has been generated                         │  │
│  │ Amount: $5,000                                                      │  │
│  │                                                                     │  │
│  │ [View Credit Note]                                                  │  │
│  └─────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘


═════════════════════════════════════════════════════════════════════════════
                              🎉 PROCESS COMPLETE 🎉
═════════════════════════════════════════════════════════════════════════════

Final State in Database:
────────────────────────────────────────────────────────────────────────────

approvalrequest:
├─ id: 123
├─ status: Approved ✅
├─ approverId: EMP100 (final approver)
└─ modifiedOn: 2025-10-03 16:00:00

ApprovalLogs (3 records):
├─ Level 1: EMP050 (Mike) - Approved - Oct 3, 11:00
├─ Level 2: EMP070 (Sarah) - Approved - Oct 3, 14:30
└─ Level 3: EMP100 (David) - Approved - Oct 3, 16:00

Return Order:
├─ Status: Approved and Processed
├─ Credit Note: CN-2025-001 generated
└─ Inventory: Updated

═════════════════════════════════════════════════════════════════════════════
```

## Alternative Flow: REJECTION

```
If Mike (or any approver) clicks "REJECT":

┌─────────────────────────────────────────────────────────────────────────┐
│ ❌ Reject Request                                                        │
│ ─────────────────────────────────────────────────────────────────────── │
│                                                                         │
│ Comments (Required):                                                    │
│ ┌───────────────────────────────────────────────────────────────────┐  │
│ │ Products are not actually damaged. Store mishandled them.         │  │
│ │ Rejecting return request.                                         │  │
│ └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│ ⚠️ Warning: This will cancel the entire approval process.              │
│                                                                         │
│                                 [CANCEL]  [REJECT] ❌                   │
└─────────────────────────────────────────────────────────────────────────┘

System Processing:
1. Create ApprovalLog (Level 1, Rejected)
2. Update approvalrequest.status = "Rejected"
3. Send notification to requester (John)
4. NO further processing
5. Request appears in "Rejected" tab

John receives:
┌─────────────────────────────────────────────────────────────────────────┐
│ ❌ Your Request Has Been Rejected                                       │
│                                                                         │
│ Request #123 - Return Order RO-12345                                    │
│ Rejected by: Mike Johnson (Level 1 - Executive)                         │
│ Reason: "Products are not actually damaged..."                          │
│                                                                         │
│ You may submit a new request with corrections.                          │
└─────────────────────────────────────────────────────────────────────────┘
```

## Key Points

1. **Login Session** determines who you are
2. **supervisorId chain** determines who approves
3. **ApprovalRuleMapping** determines which rule applies
4. **approvalhierarchy** defines approval levels
5. **approvalrequest** tracks current state
6. **ApprovalLogs** records every action
7. **Notifications** keep everyone informed
