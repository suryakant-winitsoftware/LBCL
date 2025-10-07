# Load Service Request (LSR) Database Schema

## Overview
The Load Service Request system uses two main tables in the database to store load request data:

1. **`wh_stock_request`** - Main table for stock/load requests (header level)
2. **`wh_stock_request_line`** - Detail lines for each request (line items/SKUs)

## Table: `wh_stock_request`

### Description
Main table storing warehouse stock load requests created by Van Sales Reps (LSR Portal).

### Table Name (Database)
```sql
wh_stock_request
```

### Columns

| Column Name | Data Type | Description |
|------------|-----------|-------------|
| `id` | INT (PK) | Auto-increment primary key |
| `uid` | VARCHAR | Unique identifier (GUID) |
| `company_uid` | VARCHAR | Company reference |
| `source_org_uid` | VARCHAR | Source organization UID |
| `source_wh_uid` | VARCHAR | Source warehouse UID |
| `target_org_uid` | VARCHAR | Target organization UID |
| `target_wh_uid` | VARCHAR | Target warehouse UID |
| `code` | VARCHAR | Request code/number |
| `request_type` | VARCHAR | Type of request (e.g., "Load Request", "Transfer") |
| `request_by_emp_uid` | VARCHAR | Employee who created the request |
| `job_position_uid` | VARCHAR | Job position of requester |
| `required_by_date` | DATETIME | Date when stock is required |
| `status` | VARCHAR | Request status (Pending, Approved, Rejected, etc.) |
| `remarks` | VARCHAR | Additional notes/comments |
| `stock_type` | VARCHAR | Type of stock (e.g., "Regular", "Promotional") |
| `route_uid` | VARCHAR | Associated route UID |
| `org_uid` | VARCHAR | Organization UID |
| `warehouse_uid` | VARCHAR | Warehouse UID |
| `year_month` | INT | Period identifier (YYYYMM format) |
| `ss` | INT | Sync status (0=Not Synced, 1=Synced, 2=Modified) |
| `created_time` | DATETIME | Record creation timestamp |
| `modified_time` | DATETIME | Last modification timestamp |
| `server_add_time` | DATETIME | Server-side creation time |
| `server_modified_time` | DATETIME | Server-side modification time |

### Status Values
Common status values used in the system:
- `Pending` - Newly created request awaiting approval
- `Awaiting Approval` - Submitted for approval
- `Under Review` - Being reviewed by approver
- `Approved` - Request approved
- `Rejected` - Request rejected
- `Load Sheet Generated` - Approved and load sheet created
- `In Transit` - Stock being delivered
- `Delivered` - Delivery completed
- `Cancelled` - Request cancelled

## Table: `wh_stock_request_line`

### Description
Detail table storing individual SKU line items for each stock request.

### Table Name (Database)
```sql
wh_stock_request_line
```

### Columns

| Column Name | Data Type | Description |
|------------|-----------|-------------|
| `id` | INT (PK) | Auto-increment primary key |
| `uid` | VARCHAR | Unique identifier (GUID) |
| `wh_stock_request_uid` | VARCHAR (FK) | Foreign key to wh_stock_request |
| `stock_sub_type` | VARCHAR | Sub-type of stock |
| `sku_uid` | VARCHAR | SKU reference |
| `uom1` | VARCHAR | Primary unit of measure |
| `uom2` | VARCHAR | Secondary unit of measure |
| `uom` | VARCHAR | Base unit of measure |
| `uom1_cnf` | DECIMAL | UOM1 conversion factor |
| `uom2_cnf` | DECIMAL | UOM2 conversion factor |
| `requested_qty1` | DECIMAL | Requested quantity in UOM1 |
| `requested_qty2` | DECIMAL | Requested quantity in UOM2 |
| `requested_qty` | DECIMAL | Requested quantity in base UOM |
| `cpe_approved_qty1` | DECIMAL | CPE approved quantity in UOM1 |
| `cpe_approved_qty2` | DECIMAL | CPE approved quantity in UOM2 |
| `cpe_approved_qty` | DECIMAL | CPE approved quantity in base UOM |
| `approved_qty1` | DECIMAL | Final approved quantity in UOM1 |
| `approved_qty2` | DECIMAL | Final approved quantity in UOM2 |
| `approved_qty` | DECIMAL | Final approved quantity in base UOM |
| `forward_qty1` | DECIMAL | Forwarded quantity in UOM1 |
| `forward_qty2` | DECIMAL | Forwarded quantity in UOM2 |
| `forward_qty` | DECIMAL | Forwarded quantity in base UOM |
| `collected_qty1` | DECIMAL | Collected quantity in UOM1 |
| `collected_qty2` | DECIMAL | Collected quantity in UOM2 |
| `collected_qty` | DECIMAL | Collected quantity in base UOM |
| `wh_qty` | DECIMAL | Warehouse quantity |
| `template_qty1` | DECIMAL | Template quantity in UOM1 |
| `template_qty2` | DECIMAL | Template quantity in UOM2 |
| `sku_code` | VARCHAR | SKU code |
| `line_number` | INT | Line sequence number |
| `org_uid` | VARCHAR | Organization UID |
| `warehouse_uid` | VARCHAR | Warehouse UID |
| `year_month` | INT | Period identifier (YYYYMM format) |
| `ss` | INT | Sync status |
| `created_time` | DATETIME | Record creation timestamp |
| `modified_time` | DATETIME | Last modification timestamp |
| `server_add_time` | DATETIME | Server-side creation time |
| `server_modified_time` | DATETIME | Server-side modification time |

## Relationships

```
wh_stock_request (1) ----< (N) wh_stock_request_line
       │
       │ (joins with)
       │
       ├──> route (via route_uid)
       ├──> org (via source_wh_uid - Source Warehouse)
       └──> org (via target_wh_uid - Target Warehouse)

wh_stock_request_line
       │
       └──> sku (via sku_uid)
```

## View: Load Request Item View

The system uses a view/query to display load requests with joined data:

### Query Structure
```sql
SELECT
    wsr.id AS Id,
    wsr.uid AS UID,
    wsr.code AS RequestCode,
    wsr.request_type AS RequestType,
    r.uid AS RouteUID,
    r.code AS RouteCode,
    r.name AS RouteName,
    s.code AS SourceCode,
    s.uid AS OrgUID,
    s.name AS SourceName,
    t.code AS TargetCode,
    t.name AS TargetName,
    wsr.modified_time AS ModifiedTime,
    wsr.created_time AS RequestedTime,
    wsr.required_by_date AS RequiredByDate,
    wsr.status AS Status,
    wsr.remarks AS Remarks
FROM
    wh_stock_request wsr
LEFT JOIN
    route r ON r.uid = wsr.route_uid
LEFT JOIN
    org s ON s.uid = wsr.source_wh_uid
LEFT JOIN
    org t ON t.uid = wsr.target_wh_uid
WHERE
    wsr.status = @StockType
```

## API Endpoints

### Available Endpoints

1. **Get Paginated Load Requests**
   ```
   POST /api/WHStock/SelectLoadRequestData?StockType={status}
   ```
   - Returns paginated list of load requests
   - Supports filtering, sorting, and pagination
   - StockType parameter filters by status (Pending, Approved, etc.)

2. **Get Load Request by UID**
   ```
   GET /api/WHStock/SelectLoadRequestDataByUID?UID={uid}
   ```
   - Returns complete load request with all line items

3. **Create/Update/Delete Load Request**
   ```
   POST /api/WHStock/CUDWHStock
   ```
   - Handles Create, Update, Delete operations
   - Accepts WHRequestTempleteModel with header and lines

4. **Update Request Lines**
   ```
   POST /api/WHStock/CUDWHStockRequestLine
   ```
   - Updates only the line items

5. **Create from Queue**
   ```
   POST /api/WHStock/CreateWHStockFromQueue
   ```
   - Queue-based creation using RabbitMQ

## Request/Response Models

### WHRequestTempleteModel
```csharp
{
    "WHStockRequest": { /* WHStockRequest object */ },
    "WHStockRequestLines": [ /* Array of WHStockRequestLine */ ],
    "WHStockLedgerList": [ /* Optional stock ledger updates */ ]
}
```

### WHStockRequestItemView (Response Model)
```csharp
{
    "UID": "string",
    "RequestCode": "string",
    "RequestType": "string",
    "RouteUID": "string",
    "RouteCode": "string",
    "RouteName": "string",
    "SourceCode": "string",
    "SourceName": "string",
    "TargetCode": "string",
    "TargetName": "string",
    "RequestedTime": "datetime",
    "RequiredByDate": "datetime",
    "ModifiedTime": "datetime",
    "Status": "string",
    "Remarks": "string",
    "OrgUID": "string",
    "SourceOrgUID": "string",
    "SourceWHUID": "string",
    "TargetOrgUID": "string",
    "TargetWHUID": "string"
}
```

## Data Flow

1. **Van Sales Rep (LSR Portal)**
   - Creates load request via frontend
   - Specifies route, required date, and SKU quantities
   - Submits request with status = "Pending"

2. **Database Storage**
   - Request header saved to `wh_stock_request`
   - Line items saved to `wh_stock_request_line`
   - Each record gets unique UID

3. **Approval Workflow**
   - Status changes: Pending → Under Review → Approved/Rejected
   - CPE approves quantities (updates `cpe_approved_qty` fields)
   - Final approval updates `approved_qty` fields

4. **Load Sheet Generation**
   - Status changes to "Load Sheet Generated"
   - Assignment to truck and driver
   - Creates delivery documentation

5. **Delivery Tracking**
   - Status: In Transit → Delivered
   - Updates `collected_qty` fields when collected

## Notes

- All UIDs are GUIDs (string format)
- Quantity fields support multiple UOMs for flexibility
- The `ss` (sync status) field tracks synchronization state
- `year_month` helps with data partitioning and period-based queries
- Base model includes audit fields: created_time, modified_time, server_add_time, server_modified_time
