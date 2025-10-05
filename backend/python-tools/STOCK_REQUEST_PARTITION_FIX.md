# Stock Request Partition Fix - Issue & Solution

## Date: 2025-10-05

---

## ❌ Problem

Stock request creation fails with error:
```
23514: no partition of relation "wh_stock_request_DIST1759603974795_WH001" found for row
```

---

## 🔍 Root Cause

The `wh_stock_request` table uses **3-level LIST partitioning**:

1. **Level 1**: Partitioned by `org_uid`
2. **Level 2**: Partitioned by `warehouse_uid` (combined as `org_uid_warehouse_uid`)
3. **Level 3**: Partitioned by `year_month`

### Partition Structure:
```
wh_stock_request (parent table)
  └─ wh_stock_request_DIST1759603974795 (org_uid = 'DIST1759603974795')
      └─ wh_stock_request_DIST1759603974795_WH001 (warehouse_uid = 'DIST1759603974795_WH001')
          ├─ wh_stock_request_DIST1759603974795_WH001_2510 (year_month = 2510)
          ├─ wh_stock_request_DIST1759603974795_WH001_2511
          └─ ...
```

### The Issue:

When inserting a stock request, PostgreSQL expects:
- `org_uid` = `'DIST1759603974795'` (matches Level 1 partition)
- `warehouse_uid` = `'DIST1759603974795_WH001'` (matches Level 2 partition - **COMBINED value**)
- `year_month` = `2510` (matches Level 3 partition)

But the frontend is sending:
- `OrgUID` = some org
- `WareHouseUID` = `'WH001'` (just the warehouse, not combined)

This mismatch causes the partition lookup to fail.

---

## ✅ Solution Options

### Option 1: Use Existing Partitions (Recommended)

**Available Working Combinations:**

#### For DIST1759603974795 org:
```json
{
  "OrgUID": "DIST1759603974795",
  "WareHouseUID": "DIST1759603974795_WH001",
  "YearMonth": 2510
}
```

#### For LBCL org:
```json
{
  "OrgUID": "LBCL",
  "WareHouseUID": "LBCL_TEST12",
  "YearMonth": 2510
}
```

### Option 2: Create New Partitions

Create partitions for LBCL with WH001:

```sql
-- Level 2: LBCL_WH001
CREATE TABLE wh_stock_request_LBCL_WH001
PARTITION OF wh_stock_request_LBCL
FOR VALUES IN ('LBCL_WH001')
PARTITION BY LIST (year_month);

-- Level 3: Year months
CREATE TABLE wh_stock_request_LBCL_WH001_2510
PARTITION OF wh_stock_request_LBCL_WH001
FOR VALUES IN (2510);

CREATE TABLE wh_stock_request_LBCL_WH001_2511
PARTITION OF wh_stock_request_LBCL_WH001
FOR VALUES IN (2511);

-- ... create for all needed year_month values
```

### Option 3: Fix Backend to Auto-Combine Values

Modify the backend to automatically combine org_uid and warehouse_uid:

**File:** `/backend/Modules/WHStock/Winit.Modules.WHStock.DL/Classes/PGSQLWHStockDL.cs`

In `CreateWHStocKRequest` method (line ~195), before inserting:

```csharp
// Auto-combine org_uid and warehouse_uid for partitioning
string combinedWarehouseUID = $"{wHStockRequest.OrgUID}_{wHStockRequest.WareHouseUID}";

var Query = @"INSERT INTO WH_stock_request (
  uid, company_uid, source_org_uid, source_wh_uid,
  target_org_uid, target_wh_uid, code, request_type, request_by_emp_uid, job_position_uid, required_by_date, status,
  remarks, stock_type, ss, created_time, modified_time, server_add_time, server_modified_time,
  route_uid ,org_uid,warehouse_uid,year_month
) VALUES (
  @UID, @CompanyUID, @SourceOrgUID, @SourceWHUID, @TargetOrgUID, @TargetWHUID,
  @Code, @RequestType, @RequestByEmpUID, @JobPositionUID, @RequiredByDate, @Status, @Remarks, @StockType, @SS,
  @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @RouteUID, @OrgUID, @CombinedWarehouseUID, @YearMonth
);";

// Use combinedWarehouseUID instead of WareHouseUID in parameters
```

---

## 🎯 Recommended Immediate Fix

**Change the frontend payload to use combined warehouse UID:**

### Current Payload (WRONG):
```json
{
  "WHStockRequest": {
    "OrgUID": "LBCL",
    "WareHouseUID": "WH001",  ← WRONG: Just warehouse UID
    "YearMonth": 202510
  }
}
```

### Fixed Payload (CORRECT):
```json
{
  "WHStockRequest": {
    "OrgUID": "LBCL",
    "WareHouseUID": "LBCL_WH001",  ← CORRECT: Combined org_warehouse
    "YearMonth": 2510  ← Also fix: should be YYMM not YYYYMM
  }
}
```

### Frontend Fix Location:

**File:** `/web-app/src/app/administration/warehouse-management/stock-requests/create/page.tsx`

**Line ~710** in `handleSubmit`:

```typescript
// BEFORE:
OrgUID: formData.OrgUID || formData.TargetOrgUID,
WareHouseUID: formData.WareHouseUID || formData.TargetWHUID,

// AFTER:
OrgUID: formData.TargetOrgUID,
WareHouseUID: `${formData.TargetOrgUID}_${formData.TargetWHUID}`,  // Combine org + warehouse
```

And for `WHStockRequestLines` (line ~680):

```typescript
// BEFORE:
OrgUID: formData.OrgUID || formData.TargetOrgUID,
WareHouseUID: formData.WareHouseUID || formData.TargetWHUID,

// AFTER:
OrgUID: formData.TargetOrgUID,
WareHouseUID: `${formData.TargetOrgUID}_${formData.TargetWHUID}`,  // Combine org + warehouse
```

---

## 📊 Existing Partitions

### LBCL Organization:
- `wh_stock_request_LBCL_TEST12` (year_month: 2510, 2511, 2512, 2601, 2602, 2603)

### DIST1759603974795 Organization:
- `wh_stock_request_DIST1759603974795_WH001` (year_month: 2510, 2511, 2512, 2601, 2602, 2603)

### Available Warehouses:
- `WH001` (Parent: DIST1759603974795, Type: FRWH)

---

## ⚠️ Important Notes

1. **YearMonth Format**: Should be `YYMM` (e.g., 2510 for Oct 2025), NOT `YYYYMM`

2. **WareHouseUID Must Be Combined**:
   - Format: `{org_uid}_{warehouse_uid}`
   - Example: `LBCL_WH001`, `DIST1759603974795_WH001`

3. **Both Request and Lines Need Fix**:
   - `WHStockRequest.WareHouseUID`
   - Each `WHStockRequestLine[].WareHouseUID`

4. **Don't Create Partitions Manually**:
   - Use existing partitions
   - OR implement auto-partition creation in backend

---

## 🔧 Quick Test

After fixing, test with this payload:

```json
{
  "WHStockRequest": {
    "UID": "WH-TEST-001",
    "SourceOrgUID": "LBCL",
    "SourceWHUID": "TEST12",
    "TargetOrgUID": "DIST1759603974795",
    "TargetWHUID": "WH001",
    "Code": "SR-TEST-001",
    "RequestType": "StockIn",
    "RequestByEmpUID": "ADMIN",
    "Status": "Pending",
    "StockType": "Saleable",
    "OrgUID": "DIST1759603974795",
    "WareHouseUID": "DIST1759603974795_WH001",  ← Combined value
    "YearMonth": 2510  ← YYMM format
  },
  "WHStockRequestLines": [
    {
      "UID": "LINE-001",
      "SKUUID": "EMBT3001",
      "RequestedQty": 100,
      "OrgUID": "DIST1759603974795",
      "WareHouseUID": "DIST1759603974795_WH001",  ← Combined value
      "YearMonth": 2510
    }
  ]
}
```

---

**Status:** ⏳ Awaiting frontend fix
**Priority:** P0 (Blocking stock request creation)
**Solution:** Update frontend to combine org_uid + warehouse_uid
