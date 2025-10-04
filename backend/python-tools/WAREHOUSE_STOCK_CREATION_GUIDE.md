# Warehouse Stock Creation Guide

## Overview
To create warehouse stock in the system, you need to use the **Stock Request** mechanism which creates stock entries in the warehouse.

## API Endpoints

### 1. Create Warehouse Stock Request
**Endpoint:** `POST /api/WHStock/CUDWHStock`

**Endpoint (Queue-based):** `POST /api/WHStock/CreateWHStockFromQueue`

## Data Structure

### Main Request Model: WHRequestTempleteModel

```json
{
  "WHStockRequest": {
    // Stock Request Header
  },
  "WHStockRequestLines": [
    // Stock Request Line Items (SKUs with quantities)
  ],
  "WHStockLedgerList": [
    // Optional: Stock Ledger entries
  ]
}
```

### WHStockRequest (Header) - Required Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `UID` | string | Yes | Unique identifier for the request (generate new GUID) |
| `CompanyUID` | string | Optional | Company identifier |
| `SourceOrgUID` | string | Yes | Source organization UID |
| `SourceWHUID` | string | Yes | Source warehouse UID (where stock comes from) |
| `TargetOrgUID` | string | Yes | Target organization UID |
| `TargetWHUID` | string | Yes | Target warehouse UID (where stock goes to) |
| `Code` | string | Yes | Request code/number |
| `RequestType` | string | Yes | Type of request (e.g., "StockIn", "Transfer", "Adjustment") |
| `RequestByEmpUID` | string | Yes | Employee UID who is making the request |
| `JobPositionUID` | string | Yes | Job position UID of requester |
| `RequiredByDate` | datetime | Yes | Date when stock is required |
| `Status` | string | Yes | Status (e.g., "Pending", "Approved", "Completed") |
| `Remarks` | string | Optional | Additional remarks/notes |
| `StockType` | string | Yes | Type of stock (e.g., "Saleable", "FOC", "Damaged") |
| `RouteUID` | string | Optional | Route UID if applicable |
| `OrgUID` | string | Yes | Organization UID |
| `WareHouseUID` | string | Yes | Warehouse UID |
| `YearMonth` | int | Yes | Year-month in format YYYYMM (e.g., 202510) |
| `ActionType` | enum | Yes | ActionType.Insert (1) for new records |
| `CreatedBy` | string | Yes | Username who created |
| `ModifiedBy` | string | Yes | Username who modified |
| `CreatedTime` | datetime | Yes | Creation timestamp |
| `ModifiedTime` | datetime | Yes | Modification timestamp |
| `ServerAddTime` | datetime | Yes | Server add timestamp |
| `ServerModifiedTime` | datetime | Yes | Server modified timestamp |

### WHStockRequestLine (Line Items) - Required Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `UID` | string | Yes | Unique identifier for the line (generate new GUID) |
| `CompanyUID` | string | Optional | Company identifier |
| `WHStockRequestUID` | string | Yes | Parent request UID (matches WHStockRequest.UID) |
| `StockSubType` | string | Optional | Stock sub-type |
| `SKUUID` | string | Yes | SKU unique identifier |
| `UOM` | string | Yes | Unit of Measure (e.g., "EA", "CS") |
| `UOM1` | string | Optional | Primary UOM |
| `UOM2` | string | Optional | Secondary UOM |
| `UOM1CNF` | decimal | Optional | UOM1 conversion factor |
| `UOM2CNF` | decimal | Optional | UOM2 conversion factor |
| `RequestedQty` | decimal | Yes | Requested quantity in base UOM |
| `RequestedQty1` | decimal | Optional | Requested quantity in UOM1 |
| `RequestedQty2` | decimal | Optional | Requested quantity in UOM2 |
| `ApprovedQty` | decimal | Optional | Approved quantity |
| `ApprovedQty1` | decimal | Optional | Approved quantity in UOM1 |
| `ApprovedQty2` | decimal | Optional | Approved quantity in UOM2 |
| `CollectedQty` | decimal | Optional | Collected/received quantity |
| `CollectedQty1` | decimal | Optional | Collected quantity in UOM1 |
| `CollectedQty2` | decimal | Optional | Collected quantity in UOM2 |
| `SKUCode` | string | Yes | SKU code |
| `LineNumber` | int | Yes | Line sequence number (1, 2, 3, ...) |
| `OrgUID` | string | Yes | Organization UID |
| `WareHouseUID` | string | Yes | Warehouse UID |
| `YearMonth` | int | Yes | Year-month in format YYYYMM |
| `ActionType` | enum | Yes | ActionType.Insert (1) for new records |
| `CreatedBy` | string | Yes | Username who created |
| `ModifiedBy` | string | Yes | Username who modified |
| `CreatedTime` | datetime | Yes | Creation timestamp |
| `ModifiedTime` | datetime | Yes | Modification timestamp |
| `ServerAddTime` | datetime | Yes | Server add timestamp |
| `ServerModifiedTime` | datetime | Yes | Server modified timestamp |

## Example JSON Request

```json
{
  "WHStockRequest": {
    "UID": "550e8400-e29b-41d4-a716-446655440000",
    "CompanyUID": "LBSS",
    "SourceOrgUID": "MainWarehouse",
    "SourceWHUID": "WH001",
    "TargetOrgUID": "Branch001",
    "TargetWHUID": "WH002",
    "Code": "SR-2025-001",
    "RequestType": "StockIn",
    "RequestByEmpUID": "EMP001",
    "JobPositionUID": "JP001",
    "RequiredByDate": "2025-10-15T00:00:00",
    "Status": "Approved",
    "Remarks": "Initial stock for new warehouse",
    "StockType": "Saleable",
    "RouteUID": "",
    "OrgUID": "Branch001",
    "WareHouseUID": "WH002",
    "YearMonth": 202510,
    "ActionType": 1,
    "CreatedBy": "ADMIN",
    "ModifiedBy": "ADMIN",
    "CreatedTime": "2025-10-04T10:00:00",
    "ModifiedTime": "2025-10-04T10:00:00",
    "ServerAddTime": "2025-10-04T10:00:00",
    "ServerModifiedTime": "2025-10-04T10:00:00"
  },
  "WHStockRequestLines": [
    {
      "UID": "660e8400-e29b-41d4-a716-446655440001",
      "CompanyUID": "LBSS",
      "WHStockRequestUID": "550e8400-e29b-41d4-a716-446655440000",
      "StockSubType": "",
      "SKUUID": "SKU001",
      "UOM": "EA",
      "UOM1": "CS",
      "UOM2": "EA",
      "UOM1CNF": 24,
      "UOM2CNF": 1,
      "RequestedQty": 100,
      "RequestedQty1": 4,
      "RequestedQty2": 4,
      "ApprovedQty": 100,
      "ApprovedQty1": 4,
      "ApprovedQty2": 4,
      "CollectedQty": 100,
      "CollectedQty1": 4,
      "CollectedQty2": 4,
      "CPEApprovedQty": 0,
      "CPEApprovedQty1": 0,
      "CPEApprovedQty2": 0,
      "ForwardQty": 0,
      "ForwardQty1": 0,
      "ForwardQty2": 0,
      "WHQty": 0,
      "TemplateQty1": 0,
      "TemplateQty2": 0,
      "SKUCode": "PROD001",
      "LineNumber": 1,
      "OrgUID": "Branch001",
      "WareHouseUID": "WH002",
      "YearMonth": 202510,
      "ActionType": 1,
      "CreatedBy": "ADMIN",
      "ModifiedBy": "ADMIN",
      "CreatedTime": "2025-10-04T10:00:00",
      "ModifiedTime": "2025-10-04T10:00:00",
      "ServerAddTime": "2025-10-04T10:00:00",
      "ServerModifiedTime": "2025-10-04T10:00:00"
    }
  ],
  "WHStockLedgerList": null
}
```

## Request Types

Common request types you can use:

1. **StockIn** - Initial stock entry into warehouse
2. **Transfer** - Transfer stock between warehouses
3. **Adjustment** - Stock adjustment (increase/decrease)
4. **Return** - Return stock to warehouse
5. **Damage** - Mark stock as damaged

## Stock Types

Common stock types:

1. **Saleable** - Regular sellable stock
2. **FOC** - Free of charge stock
3. **Damaged** - Damaged/unsellable stock
4. **Sample** - Sample stock
5. **Promotional** - Promotional stock

## ActionType Enum Values

- `Insert` = 1 - Create new record
- `Update` = 2 - Update existing record
- `Delete` = 3 - Delete record

## Process Flow

1. **Create Stock Request**
   - Create WHStockRequest header with warehouse and org details
   - Add WHStockRequestLines with SKU and quantity details
   - Set Status = "Pending" or "Approved"

2. **Submit Request**
   - POST to `/api/WHStock/CUDWHStock` or
   - POST to `/api/WHStock/CreateWHStockFromQueue` (for queue-based processing)

3. **Stock Creation**
   - System processes the request
   - Creates entries in `WH_Stock_Available` table
   - Creates ledger entries in `WH_Stock_Ledger` table
   - Updates stock levels

## Database Tables Involved

1. **wh_stock_request** - Stock request headers
2. **wh_stock_request_line** - Stock request line items
3. **wh_stock_available** - Available stock per warehouse
4. **wh_stock_ledger** - Stock movement ledger/history

## Prerequisites

Before creating stock, ensure:

1. ✅ Warehouse (org with warehouse org_type) exists
2. ✅ SKUs (products) exist in the system
3. ✅ Organization hierarchy is set up
4. ✅ Employee and job position exist (for requester)
5. ✅ UOM (Unit of Measure) is defined for SKUs

## Testing with cURL

```bash
curl -X POST 'http://localhost:8000/api/WHStock/CUDWHStock' \
  -H 'Content-Type: application/json' \
  -H 'Authorization: Bearer YOUR_TOKEN_HERE' \
  -d '{
    "WHStockRequest": {
      "UID": "NEW-GUID-HERE",
      "SourceOrgUID": "WAREHOUSE_UID",
      "SourceWHUID": "WH_UID",
      "TargetOrgUID": "ORG_UID",
      "TargetWHUID": "WH_UID",
      "Code": "SR-001",
      "RequestType": "StockIn",
      "RequestByEmpUID": "ADMIN",
      "JobPositionUID": "JP001",
      "RequiredByDate": "2025-10-15T00:00:00",
      "Status": "Approved",
      "StockType": "Saleable",
      "OrgUID": "ORG_UID",
      "WareHouseUID": "WH_UID",
      "YearMonth": 202510,
      "ActionType": 1,
      "CreatedBy": "ADMIN",
      "ModifiedBy": "ADMIN",
      "CreatedTime": "2025-10-04T10:00:00",
      "ModifiedTime": "2025-10-04T10:00:00",
      "ServerAddTime": "2025-10-04T10:00:00",
      "ServerModifiedTime": "2025-10-04T10:00:00"
    },
    "WHStockRequestLines": [
      {
        "UID": "LINE-GUID-HERE",
        "WHStockRequestUID": "NEW-GUID-HERE",
        "SKUUID": "SKU_UID",
        "UOM": "EA",
        "RequestedQty": 100,
        "ApprovedQty": 100,
        "SKUCode": "PROD001",
        "LineNumber": 1,
        "OrgUID": "ORG_UID",
        "WareHouseUID": "WH_UID",
        "YearMonth": 202510,
        "ActionType": 1,
        "CreatedBy": "ADMIN",
        "ModifiedBy": "ADMIN",
        "CreatedTime": "2025-10-04T10:00:00",
        "ModifiedTime": "2025-10-04T10:00:00",
        "ServerAddTime": "2025-10-04T10:00:00",
        "ServerModifiedTime": "2025-10-04T10:00:00"
      }
    ]
  }'
```

## Common Issues

1. **NULL UID errors** - Make sure to generate unique GUIDs for UID fields
2. **Foreign key errors** - Ensure referenced UIDs (SKU, Warehouse, Org) exist
3. **Stock type mismatch** - Use consistent stock types throughout
4. **UOM errors** - Ensure UOM exists and is defined for the SKU

## Notes

- The system uses **Stock Requests** as the primary mechanism for stock creation
- Stock is not directly created in `wh_stock_available` - it goes through the request workflow
- Queue-based processing (`CreateWHStockFromQueue`) is recommended for bulk operations
- Always use **ActionType.Insert (1)** for new stock entries
