# Approval System Implementation Guide

## Overview

This document provides a complete guide for the approval system implemented in the web-app. The system allows users to review and manage approval requests for initiatives, return orders, leave requests, and other business processes.

---

## 🎯 What Was Created

### 1. Type Definitions (`src/types/approval.types.ts`)

Complete TypeScript interfaces for:

- `ApprovalRequest` - Main approval request
- `ApprovalHierarchy` - Approval chain/levels
- `ApprovalLog` - Approval history/audit trail
- `AllApprovalRequest` - Universal linking to any entity
- `ViewChangeRequestApproval` - Approval listing view
- `ApprovalStatistics` - Dashboard statistics
- `InitiativeApprovalData` - Initiative-specific data
- `ReturnOrderApprovalData` - Return order-specific data
- `LeaveRequestApprovalData` - Leave request-specific data

### 2. Service Layer (`src/services/approval.service.ts`)

API integration with backend endpoints:

- `getApprovalLog()` - Get approval history
- `getApprovalHierarchy()` - Get approval chain
- `getAllChangeRequests()` - List all approvals
- `getChangeRequestData()` - Paginated approvals
- `getChangeRequestByUid()` - Get specific approval
- `approveRequest()` - Approve an approval
- `rejectRequest()` - Reject an approval
- `reassignRequest()` - Reassign to another user
- `getApprovalStatistics()` - Get dashboard stats

### 3. Reusable Components (`src/components/approvals/`)

#### a) `approval-card.tsx`

- Displays approval in card format
- Shows status, type, requester, dates
- Action buttons (View, Approve, Reject)

#### b) `approval-list.tsx`

- Table view of approvals
- Filtering by status, type, date
- Search functionality
- Sorting and pagination
- Inline actions

#### c) `approval-timeline.tsx`

- Visual timeline of approval history
- Shows all approval actions
- Displays comments and reassignments
- Color-coded by status

#### d) `approval-action-modal.tsx`

- Modal for approve/reject/reassign actions
- Form validation
- Comments field
- User selection for reassignment
- Confirmation warnings

#### e) `approval-statistics.tsx`

- Dashboard statistics cards
- Approval/rejection rates
- Breakdown by type
- Progress indicators

### 4. Pages (`src/app/administration/approvals/`)

#### a) Main Page (`page.tsx`)

- Dashboard with statistics
- Tabbed view (Pending, Approved, Rejected, All)
- Count badges on tabs
- Quick actions

#### b) Detail Page (`[id]/page.tsx`)

- Complete approval information
- Approval hierarchy display
- Timeline of all actions
- Action buttons (Approve/Reject/Reassign)
- Breadcrumb navigation

---

## 🚀 How to Use

### For Developers

#### 1. Import Components

```typescript
import {
  ApprovalList,
  ApprovalCard,
  ApprovalTimeline,
  ApprovalActionModal,
  ApprovalStatistics
} from "@/components/approvals";
```

#### 2. Use Service

```typescript
import { approvalService } from "@/services/approval.service";

// Get all approvals
const approvals = await approvalService.getAllChangeRequests();

// Get approval history
const logs = await approvalService.getApprovalLog(requestId);

// Approve a request
await approvalService.approveRequest({
  requestId: "123",
  approverId: "EMP001",
  action: "approve",
  comments: "Looks good"
});
```

#### 3. Use Types

```typescript
import { ApprovalStatus, ApprovalItemType } from "@/types/approval.types";

const isPending = approval.status === ApprovalStatus.PENDING;
const isInitiative = approval.linkedItemType === ApprovalItemType.INITIATIVE;
```

### For End Users

#### 1. Access Approvals

Navigate to: **Administration → Approvals**

#### 2. View Pending Approvals

- Click on "Pending" tab
- See all approvals waiting for your action
- Filter by type or search

#### 3. Review Approval

- Click "View Details" on any approval
- See complete information
- Check approval history
- Review hierarchy

#### 4. Take Action

- Click "Approve" → Add comments (optional) → Confirm
- Click "Reject" → Add reason (required) → Confirm
- Click "Reassign" → Select user → Add comments → Confirm

---

## 📊 How Approval Routing Works

### Step 1: User Creates Request

```
User logs in → userId: EMP001, role: MERCHANDISER
Creates return order → System captures createdBy: EMP001
```

### Step 2: System Identifies Rule

```
Query: ApprovalRuleMapping
WHERE type = 'ReturnOrder'
Result: ruleId = 5
```

### Step 3: System Gets Hierarchy

```
Query: approvalhierarchy
WHERE ruleId = 5
Result:
  Level 1: EXECUTIVE
  Level 2: SUPERVISOR
  Level 3: FINANCE_HEAD
```

### Step 4: System Finds Approvers

```
Start from EMP001
→ Get supervisorId = EMP050 (Role: EXECUTIVE) ✓
→ Get EMP050's supervisorId = EMP070 (Role: SUPERVISOR) ✓
→ Get EMP070's supervisorId = EMP100 (Role: FINANCE_HEAD) ✓
```

### Step 5: Notification Sent

```
Send email/notification to EMP050 (First approver)
"John created return order RO-12345. Please review."
```

### Step 6: Approver Logs In

```
Query: SELECT * FROM approvalrequest
WHERE approverId = 'EMP050' (from login session)
  AND status = 'Pending'

Shows: "Return Order RO-12345 - Pending Your Approval"
```

---

## 🔧 Backend API Endpoints Used

```
GET  /api/ApprovalEngine/GetApprovalLog?requestId={id}
GET  /api/ApprovalEngine/GetApprovalHierarchy?ruleId={id}
GET  /api/ApprovalEngine/GetAllChangeRequest
POST /api/ApprovalEngine/GetChangeRequestData
GET  /api/ApprovalEngine/GetChangeRequestDataByUid?requestUid={uid}
GET  /api/ApprovalEngine/GetApprovalDetailsByLinkedItemUid?requestUid={uid}
PUT  /api/ApprovalEngine/UpdateChangesInMainTable
DELETE /api/ApprovalEngine/DeleteApprovalRequest?requestId={id}
GET  /api/ApprovalEngine/GetRoleNames
GET  /api/ApprovalEngine/GetRuleId?type={type}&typeCode={code}
POST /api/ApprovalEngine/IntegrateRule
GET  /api/ApprovalEngine/GetApprovalRuleMasterData
GET  /api/ApprovalEngine/GetUserHierarchyForRule/{type}/{uid}/{ruleId}

// TODO: Need to add these endpoints in backend
POST /api/ApprovalEngine/ApproveRequest
POST /api/ApprovalEngine/RejectRequest
POST /api/ApprovalEngine/ReassignRequest
```

---

## 📁 File Structure

```
web-app/
├── src/
│   ├── types/
│   │   └── approval.types.ts ✅ (Type definitions)
│   │
│   ├── services/
│   │   └── approval.service.ts ✅ (API calls)
│   │
│   ├── components/
│   │   └── approvals/
│   │       ├── approval-card.tsx ✅
│   │       ├── approval-list.tsx ✅
│   │       ├── approval-timeline.tsx ✅
│   │       ├── approval-action-modal.tsx ✅
│   │       ├── approval-statistics.tsx ✅
│   │       └── index.ts ✅
│   │
│   └── app/
│       └── administration/
│           └── approvals/
│               ├── page.tsx ✅ (Main page)
│               └── [id]/
│                   └── page.tsx ✅ (Detail page)
│
└── APPROVAL_SYSTEM_IMPLEMENTATION.md ✅ (This file)
```

---

## ⚙️ Configuration Needed

### 1. Add Navigation Menu Item

Add to your navigation configuration:

```typescript
{
  key: 'approvals',
  label: 'Approvals',
  icon: <CheckCircleOutlined />,
  path: '/administration/approvals',
  badge: pendingCount, // Optional: Show pending count
  children: [
    {
      key: 'pending-approvals',
      label: 'Pending',
      path: '/administration/approvals?tab=pending'
    },
    {
      key: 'approval-history',
      label: 'History',
      path: '/administration/approvals?tab=all'
    }
  ]
}
```

### 2. Update API Base URL

In `src/services/api.ts`, ensure backend URL is configured:

```typescript
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";
```

### 3. Add User Context

The system needs current user information. Add to your auth context:

```typescript
const currentUser = {
  userId: "EMP001",
  userName: "John Smith",
  roleId: "MERCHANDISER",
  email: "john@multiplex.com"
};
```

Update in pages:

- Replace `'CURRENT_USER_ID'` with actual user ID from context
- Load users for reassignment dropdown

---

## 🎨 Styling

The components use Ant Design and Tailwind CSS:

```css
/* Add custom styles if needed */
.approval-card {
  transition: all 0.3s ease;
}

.approval-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}
```

---

## 🧪 Testing

### Test Scenarios

1. **View Pending Approvals**

   - Navigate to /administration/approvals
   - Check "Pending" tab
   - Verify counts are correct

2. **Approve Request**

   - Click "Approve" on pending request
   - Add comments
   - Click "Approve"
   - Verify success message
   - Check request moved to "Approved" tab

3. **Reject Request**

   - Click "Reject" on pending request
   - Add rejection reason (required)
   - Click "Reject"
   - Verify request moved to "Rejected" tab

4. **View Details**

   - Click "View Details" on any request
   - Verify all information displayed
   - Check timeline shows history
   - Verify hierarchy displayed correctly

5. **Search & Filter**
   - Use search box to find requests
   - Filter by status
   - Filter by type
   - Clear filters

---

## 🚨 TODO Items

### Backend

- [ ] Create `ApproveRequest` endpoint
- [ ] Create `RejectRequest` endpoint
- [ ] Create `ReassignRequest` endpoint
- [ ] Add notification system for approval actions
- [ ] Add email notifications

### Frontend

- [ ] Add user selection API for reassignment
- [ ] Integrate with auth context for current user
- [ ] Add real-time updates (WebSocket/Polling)
- [ ] Add approval notifications badge
- [ ] Add mobile responsive views
- [ ] Add export to PDF/Excel functionality
- [ ] Add bulk approval actions
- [ ] Add approval delegation feature

### Features

- [ ] Add approval templates
- [ ] Add conditional approval rules
- [ ] Add approval SLA tracking
- [ ] Add approval analytics
- [ ] Add approval workflow builder (UI)

---

## 🔒 Security Considerations

1. **Authorization**: Ensure users can only see approvals assigned to them
2. **Validation**: Validate all inputs on backend
3. **Audit**: All actions are logged in ApprovalLogs table
4. **Rate Limiting**: Prevent abuse of approval APIs
5. **Session**: Verify user session before approval actions

---

## 📱 Mobile Support

Components are responsive but can be enhanced:

```typescript
// Add mobile-specific views
const isMobile = useMediaQuery("(max-width: 768px)");

{
  isMobile ? (
    <ApprovalCard approval={item} />
  ) : (
    <ApprovalList data={approvals} />
  );
}
```

---

## 🎓 Training Resources

For end users, create:

1. Video tutorial on how to approve/reject
2. FAQ document
3. Quick reference guide
4. Troubleshooting guide

---

## 📞 Support

For issues or questions:

1. Check this documentation
2. Review backend API documentation
3. Check browser console for errors
4. Contact development team

---

## ✅ Completion Checklist

- [x] Type definitions created
- [x] Service layer implemented
- [x] Reusable components built
- [x] Main approval page created
- [x] Detail page created
- [ ] Navigation menu updated
- [ ] User context integrated
- [ ] Backend endpoints tested
- [ ] End-to-end testing completed
- [ ] Documentation reviewed

---

**Last Updated**: 2025-10-03
**Version**: 1.0
**Author**: Development Team
