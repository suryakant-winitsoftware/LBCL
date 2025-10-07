# Load Service Request - Approval Workflow & APIs

## Current Implementation Analysis

Based on the backend code analysis, here's what I found about approval/rejection APIs for Load Service Requests:

## ❌ No Dedicated Approval/Rejection APIs Found

The backend **does not have dedicated endpoints** for approving or rejecting load requests like:
- ~~`POST /api/WHStock/ApproveLoadRequest`~~ ❌ Does not exist
- ~~`POST /api/WHStock/RejectLoadRequest`~~ ❌ Does not exist
- ~~`POST /api/LoadRequest/ApproveLogistics`~~ ❌ Does not exist
- ~~`POST /api/LoadRequest/ApproveForklift`~~ ❌ Does not exist

## ✅ How Approval Currently Works

The approval/rejection is handled by **updating the status and approved quantities** through the existing CUD (Create/Update/Delete) endpoint:

### Endpoint:
```
POST /api/WHStock/CUDWHStock
```

### Request Body:
```json
{
  "WHStockRequest": {
    "UID": "request-uid-here",
    "Status": "Approved",  // or "Rejected", "Under Review", etc.
    "Remarks": "Approved by logistics manager",
    "ModifiedTime": "2024-01-15T10:30:00",
    "ServerModifiedTime": "2024-01-15T10:30:00",
    "SS": 2  // Sync status: 2 = Modified
  },
  "WHStockRequestLines": [
    {
      "UID": "line-uid-1",
      "WHStockRequestUID": "request-uid-here",
      "RequestedQty": 100,
      "RequestedQty1": 10,
      "RequestedQty2": 0,
      "CPEApprovedQty": 90,      // CPE approved quantity
      "CPEApprovedQty1": 9,
      "CPEApprovedQty2": 0,
      "ApprovedQty": 90,          // Final approved quantity
      "ApprovedQty1": 9,
      "ApprovedQty2": 0,
      "SS": 2
    }
  ]
}
```

## Approval Workflow Steps

### 1. **Pending → Under Review**
When a request is submitted, it starts with status "Pending" and moves to "Under Review":

```json
{
  "WHStockRequest": {
    "UID": "request-uid",
    "Status": "Under Review",
    "Remarks": "Request under logistics review",
    "ModifiedTime": "2024-01-15T10:00:00",
    "SS": 2
  }
}
```

### 2. **CPE Approval** (if required)
CPE (Central Planning Executive) approves quantities at line level:

```json
{
  "WHStockRequestLines": [
    {
      "UID": "line-uid",
      "CPEApprovedQty": 90,
      "CPEApprovedQty1": 9,
      "CPEApprovedQty2": 0,
      "SS": 2
    }
  ]
}
```

### 3. **Final Approval**
Update status to "Approved" and set final approved quantities:

```json
{
  "WHStockRequest": {
    "UID": "request-uid",
    "Status": "Approved",
    "Remarks": "Approved by logistics manager - John Doe",
    "ModifiedTime": "2024-01-15T11:00:00",
    "SS": 2
  },
  "WHStockRequestLines": [
    {
      "UID": "line-uid",
      "ApprovedQty": 90,
      "ApprovedQty1": 9,
      "ApprovedQty2": 0,
      "SS": 2
    }
  ]
}
```

### 4. **Rejection**
Update status to "Rejected":

```json
{
  "WHStockRequest": {
    "UID": "request-uid",
    "Status": "Rejected",
    "Remarks": "Insufficient warehouse stock",
    "ModifiedTime": "2024-01-15T11:00:00",
    "SS": 2
  }
}
```

## Status Values

Common status values used in the approval workflow:

| Status | Description |
|--------|-------------|
| `Pending` | Newly created request |
| `Awaiting Approval` | Submitted for approval |
| `Under Review` | Being reviewed by approver |
| `Approved` | Request approved |
| `Rejected` | Request rejected |
| `Load Sheet Generated` | Load sheet created after approval |
| `In Transit` | Stock being delivered |
| `Delivered` | Delivery completed |
| `Cancelled` | Request cancelled |

## Approval Engine (Generic)

The backend has a **generic ApprovalEngine** for managing change requests across the system:

### Available Endpoints:

```
GET  /api/ApprovalEngine/GetApprovalLog?requestId={id}
GET  /api/ApprovalEngine/GetAllChangeRequest
GET  /api/ApprovalEngine/GetChangeRequestDataByUid?requestUid={uid}
POST /api/ApprovalEngine/GetChangeRequestData  (with pagination)
PUT  /api/ApprovalEngine/UpdateChangesInMainTable
DELETE /api/ApprovalEngine/DeleteApprovalRequest?requestId={id}
```

**Note:** These endpoints are for a generic change request approval system, not specifically for load requests. They may not be integrated with the WHStock module.

## Recommended Implementation

Since there are no dedicated approve/reject APIs, you have two options:

### Option 1: Use Existing CUD Endpoint (Current Approach)

**Pros:**
- No backend changes needed
- Works with existing infrastructure
- Simple to implement

**Cons:**
- Less semantic (not clear it's an approval action)
- No built-in approval workflow/validation
- Manual status management

**Frontend Implementation:**
```typescript
// Approve Request
async approveRequest(requestUID: string, approvedLines: any[]) {
  const payload = {
    WHStockRequest: {
      UID: requestUID,
      Status: 'Approved',
      Remarks: 'Approved by logistics',
      ModifiedTime: new Date().toISOString(),
      SS: 2
    },
    WHStockRequestLines: approvedLines.map(line => ({
      UID: line.UID,
      ApprovedQty: line.ApprovedQty,
      ApprovedQty1: line.ApprovedQty1,
      ApprovedQty2: line.ApprovedQty2,
      SS: 2
    }))
  };

  return await api.loadRequest.update(payload);
}

// Reject Request
async rejectRequest(requestUID: string, reason: string) {
  const payload = {
    WHStockRequest: {
      UID: requestUID,
      Status: 'Rejected',
      Remarks: reason,
      ModifiedTime: new Date().toISOString(),
      SS: 2
    }
  };

  return await api.loadRequest.update(payload);
}
```

### Option 2: Create Dedicated Approval Endpoints (Recommended)

Request the backend team to create dedicated endpoints:

```csharp
// In WHStockController.cs

[HttpPost]
[Route("ApproveLoadRequest")]
public async Task<ActionResult> ApproveLoadRequest([FromBody] ApprovalRequest request)
{
    // Validation logic
    // Update status to Approved
    // Update approved quantities
    // Log approval action
    // Send notifications
}

[HttpPost]
[Route("RejectLoadRequest")]
public async Task<ActionResult> RejectLoadRequest([FromBody] RejectionRequest request)
{
    // Validation logic
    // Update status to Rejected
    // Log rejection action
    // Send notifications
}
```

**Pros:**
- Clear, semantic API
- Built-in validation and business logic
- Better audit trail
- Can add approval workflows, notifications, etc.

**Cons:**
- Requires backend development
- More complex implementation

## Quantity Fields Explanation

### Three Levels of Quantities:

1. **Requested Quantities**
   - `RequestedQty`, `RequestedQty1`, `RequestedQty2`
   - Original quantities requested by Van Sales Rep

2. **CPE Approved Quantities**
   - `CPEApprovedQty`, `CPEApprovedQty1`, `CPEApprovedQty2`
   - Quantities approved by Central Planning Executive
   - May differ from requested if stock is limited

3. **Final Approved Quantities**
   - `ApprovedQty`, `ApprovedQty1`, `ApprovedQty2`
   - Final quantities approved by logistics manager
   - These are the quantities that will be loaded

### UOM (Unit of Measure) System:
- `UOM` - Base unit
- `UOM1` - Primary unit (e.g., Cases)
- `UOM2` - Secondary unit (e.g., Bottles)
- `UOM1CNF`, `UOM2CNF` - Conversion factors

Example:
- UOM = Bottles
- UOM1 = Cases (1 case = 24 bottles, so UOM1CNF = 24)
- UOM2 = Pallets (1 pallet = 10 cases = 240 bottles, so UOM2CNF = 240)

## Integration with Frontend

Update your LSR service to handle approvals:

```typescript
// src/app/load-management/lsr/services/lsr.service.ts

/**
 * Approve load service request
 */
async approveRequest(requestUID: string, approvedLines: any[], remarks?: string) {
  try {
    const payload = {
      WHStockRequest: {
        UID: requestUID,
        Status: 'Approved',
        Remarks: remarks || 'Approved',
        ModifiedTime: new Date().toISOString(),
        ServerModifiedTime: new Date().toISOString(),
        SS: 2
      },
      WHStockRequestLines: approvedLines
    };

    const response = await api.loadRequest.update(payload);
    return response;
  } catch (error) {
    console.error('Error approving LSR request:', error);
    throw error;
  }
}

/**
 * Reject load service request
 */
async rejectRequest(requestUID: string, reason: string) {
  try {
    const payload = {
      WHStockRequest: {
        UID: requestUID,
        Status: 'Rejected',
        Remarks: reason,
        ModifiedTime: new Date().toISOString(),
        ServerModifiedTime: new Date().toISOString(),
        SS: 2
      }
    };

    const response = await api.loadRequest.update(payload);
    return response;
  } catch (error) {
    console.error('Error rejecting LSR request:', error);
    throw error;
  }
}
```

## Summary

✅ **What exists:**
- Generic CUD endpoint for updating load requests
- Status field in database
- Approved quantity fields at line level
- Generic approval engine (for change requests)

❌ **What doesn't exist:**
- Dedicated approve/reject endpoints for load requests
- Built-in approval workflow for load requests
- Approval history tracking for load requests

**Recommendation:** Use the CUD endpoint with status updates for now, or request backend team to create dedicated approval endpoints if more complex approval workflows are needed.
