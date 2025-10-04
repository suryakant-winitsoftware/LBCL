# API Request Field Mapping Documentation
*Generated: 2025-09-04*
*Purpose: Complete documentation for all table entities in AppRequest system*

## Overview
This document provides comprehensive field mapping and requirements for all entities that can be uploaded through the AppRequest API. Based on analysis of the successful `sales_order` implementation, all entities require specific common fields for proper processing by worker services.

## Common Pattern Analysis

### Universal Required Fields (ALL Tables)
Every entity record MUST include these fields:

```json
{
    "Id": 1,                    // Database ID field (integer)
    "SS": 1,                    // System Status field (integer) 
    "CreatedBy": "EMP_UID",     // Employee UID who created (string)
    "ModifiedBy": "EMP_UID",    // Employee UID who modified (string)
    "CreatedTime": "ISO8601",   // Creation timestamp (ISO 8601 format)
    "ModifiedTime": "ISO8601",  // Modification timestamp (ISO 8601 format)
    "Tablename": "table_name"   // Exact table name (string)
}
```

### Table Group to LinkedItemType Mapping

Based on troubleshooting documentation and database analysis:

| Table Group | LinkedItemType | Queue Pattern | Suffix |
|-------------|---------------|---------------|---------|
| sales | `sales` | `sales_queue_ramana` | `_ramana` |
| master | `master` | `master_queue_ramana` | `_ramana` |
| Merchandiser | `store_check` | `store_check_queue_ramana` | `_ramana` |
| return_order | `return_order_line` | `return_queue_ramana` | `_ramana` |
| collection | `collection` | `collection_queue_ramana` | `_ramana` |
| stock_request | `wh_stock_request_stock` | `stock_request_queue_ramana` | `_ramana` |
| collection_deposit | `collection_deposit` | `collection_deposit_queue_ramana` | `_ramana` |

## Complete Entity Documentation

### SALES GROUP

#### 1. sales_order
- **LinkedItemType**: `sales`
- **Queue**: `sales_queue_ramana`
- **Required Database Fields**:
  - `id` (auto-increment)
  - `uid` (unique identifier)
  - `created_by` (employee UID)
  - `modified_by` (employee UID)

**Minimal Request Structure**:
```json
{
    "IsNewOrder": true,
    "SalesOrder": {
        "Id": 1,
        "UID": "unique_sales_order_id",
        "SS": 1,
        "CompanyUID": "EPIC01",           // org.uid reference
        "OrgUID": "EPIC01",              // org.uid reference
        "StoreUID": "S1518",             // store.uid reference
        "CurrencyUid": "USD",            // currency.uid reference
        "EmpUID": "TB3227",              // emp.uid reference
        "JobPositionUID": "TB3227",      // emp.uid reference
        "CreatedBy": "TB3227",           // emp.uid reference
        "ModifiedBy": "TB3227",          // emp.uid reference
        "CreatedTime": "2025-09-04T10:30:00.000Z",
        "ModifiedTime": "2025-09-04T10:30:00.000Z",
        "OrderDate": "2025-09-04T10:30:00.000Z",
        "SalesOrderNumber": "SO-12345",
        "Status": "DELIVERED",
        "OrderType": "REGULAR",
        "PaymentType": "Credit",
        "Source": "Mobile App",
        "TotalAmount": 100.00,
        "NetAmount": 100.00,
        "LineCount": 1,
        "QtyCount": 1,
        "Tablename": "sales_order"
    },
    "SalesOrderLines": [...], // See sales_order_line below
    "RequestUIDDictionary": {
        "SalesOrder": ["unique_sales_order_id"],
        "SalesOrderLine": ["unique_line_id_1"]
    }
}
```

#### 2. sales_order_line
- **LinkedItemType**: `sales` (same as parent)
- **Required for parent sales_order processing**

**Minimal Line Structure**:
```json
{
    "Id": 1,
    "UID": "unique_line_id",
    "SS": 1,
    "SKUUID": "ECO8040002",          // sku.uid reference
    "ItemCode": "ECO8040002",        // Usually same as SKUUID
    "SalesOrderUID": "parent_sales_order_uid",
    "Qty": 1.0,
    "UnitPrice": 100.00,
    "TotalAmount": 100.00,
    "NetAmount": 100.00,
    "CreatedBy": "TB3227",
    "ModifiedBy": "TB3227",
    "CreatedTime": "2025-09-04T10:30:00.000Z",
    "ModifiedTime": "2025-09-04T10:30:00.000Z",
    "Tablename": "sales_order_line"
}
```

### MASTER GROUP

#### 3. address
- **LinkedItemType**: `master`
- **Queue**: `master_queue_ramana`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

**Minimal Structure**:
```json
{
    "Id": 1,
    "UID": "unique_address_id",
    "SS": 1,
    "CreatedBy": "EMP_UID",
    "ModifiedBy": "EMP_UID", 
    "CreatedTime": "2025-09-04T10:30:00.000Z",
    "ModifiedTime": "2025-09-04T10:30:00.000Z",
    "Tablename": "address",
    // Address-specific fields
    "AddressLine1": "Street Address",
    "City": "City Name",
    "State": "State Name",
    "Country": "Country Name",
    "PostalCode": "12345"
}
```

#### 4. store_history
- **LinkedItemType**: `master`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 5. beat_history  
- **LinkedItemType**: `master`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 6. emp_productivity
- **LinkedItemType**: `master` 
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 7. store_activity
- **LinkedItemType**: `master`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 8. store_activity_history
- **LinkedItemType**: `master`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 9. store_activity_role_mapping
- **LinkedItemType**: `master`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 10. store_history_stats
- **LinkedItemType**: `master`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 11. survey
- **LinkedItemType**: `master`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 12. user_journey
- **LinkedItemType**: `master`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

### MERCHANDISER GROUP

#### 13. store_check_history
- **LinkedItemType**: `store_check`
- **Queue**: `store_check_queue_ramana`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 14. store_check_item_history
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 15. store_check_group_history
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 16. store_check_item_expiry_der_history
- **LinkedItemType**: `store_check` 
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 17. store_check_item_uom_qty
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 18. planogram_execution_header
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 19. planogram_execution_detail
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 20. planogram_execution_v1
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 21. po_execution
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 22. po_execution_line
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 23. expiry_check_execution
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 24. expiry_check_execution_line
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 25. broadcast_initiative
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 26. capture_competitor
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 27. category_brand_mapping
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 28. category_brand_competitor_mapping
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 29. product_feedback
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

#### 30. product_sampling
- **LinkedItemType**: `store_check`
- **Required Database Fields**: `id`, `uid`, `created_by`, `modified_by`

## AppRequest Structure Template

### Standard AppRequest Format:
```json
[
    {
        "Id": 0,
        "UID": "unique_request_id",
        "OrgUID": "EPIC01",
        "LinkedItemType": "sales|master|store_check|collection|etc",
        "YearMonth": 2509,
        "LinkedItemUID": "primary_entity_uid", 
        "EmpUID": "TB3227",
        "JobPositionUID": "TB3227",
        "RequestCreatedTime": "2025-09-04T10:30:00.000Z",
        "RequestBody": "{...JSON_STRING_OF_ENTITY_DATA...}",
        "RequestUIDs": "{...JSON_STRING_OF_UIDS_DICTIONARY...}"
    }
]
```

### RequestBody Structure Pattern:
```json
{
    "IsNewOrder": true,                    // For new records
    "EntityName": { /* entity data */ },  // Main entity  
    "EntityNameLines": [ /* line items */ ], // If applicable
    "WHStockLedgerList": [],              // Usually empty
    "StoreHistory": null,                 // Usually null
    "AccPayable": null,                   // Usually null
    "ActionType": 1,                      // Action type
    "RequestUIDDictionary": {
        "EntityName": ["uid1", "uid2"],
        "EntityNameLine": ["line_uid1"]
    }
}
```

## Foreign Key References

### Common Reference Tables:
- **org.uid**: Organization references (`EPIC01`, `MAIN`, `Supplier`, `RO`)
- **store.uid**: Store references (e.g., `S1518`)
- **emp.uid**: Employee references (e.g., `TB3227`)  
- **currency.uid**: Currency references (`USD`, `INR`)
- **sku.uid**: Product SKU references (e.g., `ECO8040002`)

### Validation Queries:
```sql
-- Verify organization exists
SELECT uid FROM org WHERE uid = 'EPIC01';

-- Verify store exists  
SELECT uid FROM store WHERE uid = 'S1518';

-- Verify employee exists
SELECT uid FROM emp WHERE uid = 'TB3227';

-- Verify currency exists
SELECT uid FROM currency WHERE uid = 'USD';

-- Verify SKU exists
SELECT uid FROM sku WHERE uid = 'ECO8040002';
```

## Worker Service Architecture

### Queue Processing Flow:
1. **API Receives Request** → Saves to `app_request` table
2. **RabbitMQ Message Posted** → Queue name based on LinkedItemType + `_queue_ramana`
3. **Worker Service Consumes** → Processes message from queue
4. **Database Insert** → Creates records in target tables
5. **Firebase Notification** → Optional success notification

### Queue Name Pattern:
- Format: `{LinkedItemType}_queue{suffix}`
- Suffix: `_ramana` (from database table group configuration)
- Examples:
  - `sales` → `sales_queue_ramana`
  - `master` → `master_queue_ramana` 
  - `store_check` → `store_check_queue_ramana`

## Error Prevention Checklist

### Before Submitting Any Request:
1. ✅ **Verify Foreign Keys**: All referenced UIDs exist in database
2. ✅ **Include Required Fields**: All universal fields (`Id`, `SS`, timestamps, etc.)
3. ✅ **Correct LinkedItemType**: Matches table group mapping
4. ✅ **Valid Timestamps**: Use ISO 8601 format
5. ✅ **Array Format**: Request must be wrapped in array `[{...}]`
6. ✅ **JSON String Format**: RequestBody must be escaped JSON string
7. ✅ **Tablename Field**: Must match exact database table name

## Testing Template

### Generic Test Request:
```bash
curl -X POST "https://multiplex-promotions-api.winitsoftware.com/api/AppRequest/PostAppRequest" \
-H "Content-Type: application/json" \
-d '[{
    "Id": 0,
    "UID": "test-{ENTITY}-{TIMESTAMP}",
    "OrgUID": "EPIC01", 
    "LinkedItemType": "{LINKED_ITEM_TYPE}",
    "YearMonth": 2509,
    "LinkedItemUID": "test_{ENTITY}_{TIMESTAMP}",
    "EmpUID": "TB3227",
    "JobPositionUID": "TB3227", 
    "RequestCreatedTime": "{ISO_TIMESTAMP}",
    "RequestBody": "{ESCAPED_JSON_ENTITY_DATA}",
    "RequestUIDs": "{ESCAPED_JSON_UIDS}"
}]'
```

## Success Verification

### Verify Processing:
1. **Check app_request table**: Request saved successfully
2. **Check RabbitMQ queue**: Messages consumed (0 in queue)
3. **Check target table**: Records created successfully
4. **Check foreign keys**: All references valid

### Database Verification Queries:
```sql
-- Check if request was saved
SELECT uid, linked_item_type, linked_item_uid 
FROM app_request 
WHERE uid = 'your_test_uid';

-- Check if entity was created
SELECT uid FROM {target_table} 
WHERE uid = 'your_entity_uid';
```

---
*This documentation ensures consistent, error-free processing across all AppRequest entities.*