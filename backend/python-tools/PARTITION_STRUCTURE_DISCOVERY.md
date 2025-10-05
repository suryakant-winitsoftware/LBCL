# Partition Structure Discovery

## Date: 2025-10-05

---

## 🔍 Critical Discovery

The `wh_stock_request` table has **INCONSISTENT partition structures** across different organizations!

### Partition Structure by Organization:

#### DIST001 (Combined Format):
```
Level 1: org_uid = 'DIST001'
Level 2: warehouse_uid = 'DIST001_1234', 'DIST001_FRWH', etc. (COMBINED)
```

#### DIST1759603974795 (Simple Format):
```
Level 1: org_uid = 'DIST1759603974795'
Level 2: warehouse_uid = 'WH001' (SIMPLE - just warehouse!)
```

#### LBCL (Simple Format):
```
Level 1: org_uid = 'LBCL'
Level 2: warehouse_uid = 'TEST12' (SIMPLE - just warehouse!)
```

#### Farmley (Simple Format):
```
Level 1: org_uid = 'Farmley'
Level 2: warehouse_uid = 'TB01003', 'TB01006', etc. (SIMPLE)
```

---

## ❌ The Problem

The partition expectation **varies by organization**:

1. **DIST001**: Expects `warehouse_uid = 'DIST001_WH001'` (combined)
2. **DIST1759603974795**: Expects `warehouse_uid = 'WH001'` (simple)
3. **LBCL**: Expects `warehouse_uid = 'TEST12'` (simple)

When we send:
```json
{
  "OrgUID": "DIST1759603974795",
  "WareHouseUID": "DIST1759603974795_WH001"
}
```

PostgreSQL looks for a partition with:
```
org_uid = 'DIST1759603974795' AND warehouse_uid = 'DIST1759603974795_WH001'
```

But the partition expects:
```
org_uid = 'DIST1759603974795' AND warehouse_uid = 'WH001'
```

**Mismatch = Partition not found error!**

---

## ✅ Solution

**DO NOT combine org_uid and warehouse_uid** - use the warehouse UID as-is!

The backend already has the correct values in `TargetWHUID`. We should use that directly.

### Frontend Fix:

```typescript
// WRONG (current):
WareHouseUID: `${formData.TargetOrgUID}_${formData.TargetWHUID}`

// CORRECT:
WareHouseUID: formData.TargetWHUID  // Use as-is, no combining!
```

### Payload Example:

```json
{
  "WHStockRequest": {
    "OrgUID": "DIST1759603974795",
    "WareHouseUID": "WH001",  ← Just the warehouse UID!
    "YearMonth": 2510
  }
}
```

---

## 📊 Partition Value Formats

| Org UID | Warehouse UID Format | Example |
|---------|---------------------|---------|
| DIST001 | Combined: `{org}_{wh}` | `DIST001_FRWH` |
| DIST1759603974795 | Simple: `{wh}` | `WH001` |
| LBCL | Simple: `{wh}` | `TEST12` |
| Farmley | Simple: `{wh}` | `TB01003` |
| SIPL | Combined: `{org}_{wh}` | `SIPL_23` |
| Switz | Simple: `{wh}` | `SIPL` |
| WINIT | Mixed | `WINIT`, `Test001`, etc. |

---

## 🎯 Action Required

1. **Revert frontend change** - stop combining org_uid + warehouse_uid
2. **Use TargetWHUID directly** for WareHouseUID field
3. **The partitions were created with inconsistent naming** - we must match what exists

---

## 💡 Why This Happened

The partition creation was done inconsistently:
- Some orgs used combined warehouse UIDs
- Some used simple warehouse UIDs
- The partition table name vs partition expression value are different things

**Example:**
```sql
-- Table name: wh_stock_request_DIST1759603974795_WH001
-- Partition expression: FOR VALUES IN ('WH001')
                                           ↑
                                    This is what matters!
```

---

**Resolution:** Use warehouse UID as-is, without combining with org UID.
