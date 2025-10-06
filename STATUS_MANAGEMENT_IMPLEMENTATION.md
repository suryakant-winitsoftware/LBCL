# Status Management Implementation Guide

## Overview
Implemented separate status tracking for delivery loading and stock receiving workflows instead of managing status in `purchase_order_header` table.

## Database Changes

### 1. DeliveryLoadingTracking Table
**New Column:** `Status` (VARCHAR(50), DEFAULT 'PENDING')

**Valid Status Values:**
- `PENDING` - Initial state when delivery loading is created
- `APPROVED_FOR_SHIPMENT` - When approved for shipment
- `SHIPPED` - When goods have been shipped

**Purpose:** Track the delivery/loading workflow stages independently

### 2. StockReceivingTracking Table
**New Column:** `Status` (VARCHAR(50), DEFAULT 'PENDING')

**Valid Status Values:**
- `PENDING` - Initial state when stock receiving starts
- `COMPLETED` - When stock receiving is complete

**Purpose:** Track the stock receiving workflow stages independently

## Migration Script
**Location:** `/backend/migrations/add_status_columns.sql`

**To Apply Migration:**
```bash
psql -h <hostname> -U <username> -d <database> -f backend/migrations/add_status_columns.sql
```

## Backend Changes

### Models Updated:
1. `IDeliveryLoadingTracking.cs` - Added `Status` property
2. `DeliveryLoadingTracking.cs` - Added `Status` property
3. `IStockReceivingTracking.cs` - Added `Status` property
4. `StockReceivingTracking.cs` - Added `Status` property

### Data Layer Updated:
1. `PGSQLDeliveryLoadingTrackingDL.cs`
   - Added `Status` to all SELECT queries
   - Added `Status` to INSERT query with default 'PENDING'
   - Added `Status` to UPDATE query
   - Modified `GetByStatusAsync` to filter by `dlt."Status"` instead of `poh.status`

2. `PGSQLStockReceivingTrackingDL.cs`
   - Added `Status` to all SELECT queries
   - Added `Status` to INSERT query with default 'PENDING'
   - Added `Status` to UPDATE query

### Default Behavior:
- When creating new records, if Status is not provided, it defaults to 'PENDING'
- Status is preserved during updates unless explicitly changed

## Frontend Changes Required

### Services to Update:
1. **deliveryLoadingService.ts**
   - Add `Status` field to interface
   - Set status when creating/updating delivery loading records

2. **stockReceivingService.ts**
   - Add `Status` field to interface
   - Set status when creating/updating stock receiving records

### Example Usage:

#### Delivery Loading (activity-log page)
```typescript
// When submitting the form with "Shipped" status
const deliveryLoadingData = {
  ...existingData,
  Status: "SHIPPED",  // Set to SHIPPED when form is submitted
  // other fields...
};
```

#### Stock Receiving (stock-receiving page)
```typescript
// When completing stock receiving
const stockReceivingData = {
  ...existingData,
  Status: "COMPLETED",  // Set to COMPLETED when form is submitted
  // other fields...
};
```

## Workflow Status Transitions

### Delivery Loading Workflow:
```
PENDING → APPROVED_FOR_SHIPMENT → SHIPPED
```

### Stock Receiving Workflow:
```
PENDING → COMPLETED
```

## Benefits

1. **Separation of Concerns:** Workflow status is tracked separately from purchase order status
2. **Better Tracking:** Each stage has its own status in its respective table
3. **Edit History:** Easier to track and edit workflow-specific data in the future
4. **Independent Workflows:** Delivery and receiving workflows can progress independently
5. **No Pollution:** Doesn't clutter the main `purchase_order_header` table with workflow statuses

## Database Indexes
Created indexes on Status columns for better query performance:
- `idx_delivery_loading_tracking_status`
- `idx_stock_receiving_tracking_status`

## Migration Status
✅ Backend models updated
✅ Backend DL layer updated
✅ Backend compiled successfully
✅ Migration script created
✅ Database migration completed successfully
✅ Frontend service interfaces updated with Status field
✅ Activity log page updated to set Status = "SHIPPED"
✅ Stock receiving page updated to set Status = "COMPLETED"

## Implementation Complete! 🎉

All changes have been successfully deployed:
- Database columns added
- Backend code updated and compiled
- Frontend interfaces and pages updated
- Status workflow is now tracked separately from purchase_order_header
