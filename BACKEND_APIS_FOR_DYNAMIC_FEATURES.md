# Backend APIs for Dynamic Delivery & Manager Features

## Overview
This document explains all the backend APIs needed to make the delivery and manager features work dynamically. The APIs are organized by functionality and include request/response examples.

---

## Table of Contents
1. [Delivery Module APIs](#delivery-module-apis)
2. [Manager/Warehouse Module APIs](#managerwarehouse-module-apis)
3. [Distributor Management APIs](#distributor-management-apis)
4. [Common/Shared APIs](#commonshared-apis)
5. [API Integration Examples](#api-integration-examples)
6. [Complete API List Summary](#complete-api-list-summary)

---

## Delivery Module APIs

### 1. Route Management APIs
**Base URL:** `/api/Route`

These APIs handle delivery routes, schedules, and route configurations.

#### 1.1 Get All Routes (with Pagination)
```http
POST /api/Route/SelectAllRouteDetails
```

**Request Body:**
```json
{
  "sortCriterias": [
    { "sortParameter": "Name", "direction": "Asc" }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "filterCriterias": [
    { "name": "OrgUID", "value": "org-123", "filterType": "Equal" },
    { "name": "IsActive", "value": true, "filterType": "Equal" }
  ],
  "isCountRequired": true
}
```

**Response:**
```json
{
  "pagedData": [
    {
      "uid": "route-001",
      "name": "North Zone Route 1",
      "code": "NZ-R1",
      "orgUID": "org-123",
      "warehouseUID": "wh-001",
      "vehicleUID": "vh-001",
      "isActive": true
    }
  ],
  "totalCount": 45
}
```

**Usage:** Display all routes in delivery dashboard with filtering and sorting.

---

#### 1.2 Get Route Details by UID
```http
GET /api/Route/SelectRouteDetailByUID?UID={routeUID}
```

**Response:**
```json
{
  "uid": "route-001",
  "name": "North Zone Route 1",
  "code": "NZ-R1",
  "description": "Main route for north zone",
  "orgUID": "org-123",
  "warehouseUID": "wh-001",
  "vehicleUID": "vh-001",
  "isActive": true,
  "createdDate": "2024-01-15T10:00:00Z"
}
```

**Usage:** Show detailed route information when user clicks on a specific route.

---

#### 1.3 Get Complete Route Master (with Customers & Schedule)
```http
GET /api/Route/SelectRouteMasterViewByUID?UID={routeUID}
```

**Response:**
```json
{
  "route": { /* route details */ },
  "routeSchedule": {
    "uid": "schedule-001",
    "routeUID": "route-001",
    "scheduleType": "Weekly",
    "frequency": "Mon,Wed,Fri"
  },
  "routeCustomers": [
    {
      "uid": "rc-001",
      "routeUID": "route-001",
      "storeUID": "store-001",
      "storeName": "ABC Store",
      "sequenceNo": 1,
      "frequency": "Monday"
    }
  ],
  "routeUsers": [
    {
      "uid": "ru-001",
      "routeUID": "route-001",
      "empUID": "emp-001",
      "empName": "John Doe",
      "role": "Driver"
    }
  ]
}
```

**Usage:** Complete route view with all related data (customers, users, schedule).

---

#### 1.4 Create Route Master
```http
POST /api/Route/CreateRouteMaster
```

**Request Body:**
```json
{
  "route": {
    "uid": "route-new",
    "name": "South Zone Route 3",
    "code": "SZ-R3",
    "orgUID": "org-123",
    "warehouseUID": "wh-002"
  },
  "routeSchedule": {
    "uid": "schedule-new",
    "routeUID": "route-new",
    "scheduleType": "Daily"
  },
  "routeCustomersList": [
    {
      "routeUID": "route-new",
      "storeUID": "store-101",
      "frequency": "Daily",
      "sequenceNo": 1
    }
  ],
  "routeScheduleCustomerMappings": [
    {
      "routeScheduleUID": "schedule-new",
      "customerUID": "store-101",
      "routeScheduleConfigUID": "config-001"
    }
  ]
}
```

**Usage:** Create a new route with customers and schedule in one API call.

---

#### 1.5 Update Route Master
```http
PUT /api/Route/UpdateRouteMaster
```

**Usage:** Update existing route configuration.

---

#### 1.6 Delete Route
```http
DELETE /api/Route/DeleteRouteDetail?UID={routeUID}
```

**Usage:** Remove a route from the system.

---

#### 1.7 Get Routes by Store/Customer
```http
GET /api/Route/GetRoutesByStoreUID?OrgUID={orgUID}&StoreUID={storeUID}
```

**Response:**
```json
[
  {
    "value": "route-001",
    "text": "North Zone Route 1"
  },
  {
    "value": "route-002",
    "text": "North Zone Route 2"
  }
]
```

**Usage:** Show which routes serve a particular customer/store.

---

#### 1.8 Get Vehicle Dropdown
```http
GET /api/Route/GetVehicleDDL?orgUID={orgUID}
```

**Response:**
```json
[
  { "value": "vh-001", "text": "Truck - MH01AB1234" },
  { "value": "vh-002", "text": "Van - MH01CD5678" }
]
```

**Usage:** Populate vehicle selection dropdown when creating/editing routes.

---

#### 1.9 Get Warehouse Dropdown
```http
GET /api/Route/GetWareHouseDDL?OrgTypeUID={orgTypeUID}&ParentUID={parentUID}
```

**Response:**
```json
[
  { "value": "wh-001", "text": "Main Warehouse" },
  { "value": "wh-002", "text": "Regional Warehouse" }
]
```

**Usage:** Populate warehouse selection dropdown.

---

#### 1.10 Get User Dropdown
```http
GET /api/Route/GetUserDDL?OrgUID={orgUID}
```

**Response:**
```json
[
  { "value": "emp-001", "text": "John Doe - Driver" },
  { "value": "emp-002", "text": "Jane Smith - Helper" }
]
```

**Usage:** Populate delivery agent/driver selection dropdown.

---

#### 1.11 Get Route Schedule Configurations
```http
GET /api/Route/GetAllRouteScheduleConfigs
```

**Response:**
```json
[
  {
    "uid": "config-001",
    "name": "Daily Delivery",
    "type": "Daily"
  },
  {
    "uid": "config-002",
    "name": "Weekly - Mon/Wed/Fri",
    "type": "Weekly",
    "days": "Monday,Wednesday,Friday"
  }
]
```

**Usage:** Get available schedule templates for route planning.

---

### 2. Route Customer APIs
**Base URL:** `/api/RouteCustomer`

These APIs manage customer-route mappings.

#### 2.1 Get All Route Customers
```http
POST /api/RouteCustomer/SelectAllRouteCustomerDetails
```

**Request Body:**
```json
{
  "sortCriterias": [],
  "pageNumber": 1,
  "pageSize": 50,
  "filterCriterias": [
    { "name": "RouteUID", "value": "route-001", "filterType": "Equal" }
  ],
  "isCountRequired": true
}
```

**Response:**
```json
{
  "pagedData": [
    {
      "uid": "rc-001",
      "routeUID": "route-001",
      "storeUID": "store-001",
      "storeName": "ABC Store",
      "storeCode": "ST001",
      "frequency": "Daily",
      "sequenceNo": 1,
      "isActive": true
    }
  ],
  "totalCount": 25
}
```

**Usage:** Show all customers mapped to a route, with their sequence and frequency.

---

#### 2.2 Get Route Customer by UID
```http
GET /api/RouteCustomer/SelectRouteCustomerDetailByUID?UID={uid}
```

**Usage:** Get specific route-customer mapping details.

---

#### 2.3 Get Routes by Store UID
```http
GET /api/RouteCustomer/GetRouteByStoreUID?storeUID={storeUID}
```

**Response:**
```json
[
  { "value": "route-001", "text": "North Zone Route 1" },
  { "value": "route-003", "text": "Express Route" }
]
```

**Usage:** Find which routes serve a specific customer.

---

#### 2.4 Create Route Customer Mapping
```http
POST /api/RouteCustomer/CreateRouteCustomerDetails
```

**Request Body:**
```json
{
  "uid": "rc-new",
  "routeUID": "route-001",
  "storeUID": "store-105",
  "frequency": "Monday,Wednesday,Friday",
  "sequenceNo": 12,
  "isActive": true
}
```

**Usage:** Add a new customer to a route.

---

#### 2.5 Update Route Customer Mapping
```http
PUT /api/RouteCustomer/UpdateRouteCustomerDetails
```

**Usage:** Change customer's position, frequency, or other route settings.

---

#### 2.6 Delete Route Customer Mappings
```http
DELETE /api/RouteCustomer/DeleteRouteCustomerDetails
```

**Request Body:**
```json
["rc-001", "rc-002", "rc-003"]
```

**Usage:** Remove multiple customers from a route.

---

### 3. Route User APIs
**Base URL:** `/api/RouteUser`

These APIs manage delivery agent/driver assignments to routes.

#### 3.1 Get All Route Users
```http
POST /api/RouteUser/SelectAllRouteUserDetails
```

**Request Body:**
```json
{
  "filterCriterias": [
    { "name": "RouteUID", "value": "route-001", "filterType": "Equal" }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "isCountRequired": true
}
```

**Response:**
```json
{
  "pagedData": [
    {
      "uid": "ru-001",
      "routeUID": "route-001",
      "empUID": "emp-001",
      "empName": "John Doe",
      "role": "Driver",
      "isActive": true
    }
  ],
  "totalCount": 2
}
```

**Usage:** Show which delivery agents are assigned to a route.

---

#### 3.2 Get Route Users by UID
```http
GET /api/RouteUser/SelectRouteUserByUID
```

**Request Body:**
```json
["ru-001", "ru-002"]
```

**Usage:** Get details of specific route-user assignments.

---

#### 3.3 Create Route User Assignment
```http
POST /api/RouteUser/CreateRouteUser
```

**Request Body:**
```json
[
  {
    "uid": "ru-new-1",
    "routeUID": "route-001",
    "empUID": "emp-005",
    "role": "Driver",
    "isActive": true
  },
  {
    "uid": "ru-new-2",
    "routeUID": "route-001",
    "empUID": "emp-006",
    "role": "Helper",
    "isActive": true
  }
]
```

**Usage:** Assign delivery agents to a route.

---

#### 3.4 Update Route User Assignment
```http
PUT /api/RouteUser/UpdateRouteUser
```

**Usage:** Update agent assignments or roles.

---

#### 3.5 Delete Route User Assignments
```http
DELETE /api/RouteUser/DeleteRouteUser
```

**Request Body:**
```json
["ru-001", "ru-002"]
```

**Usage:** Remove agents from a route.

---

### 4. Journey Plan APIs
**Base URL:** `/api/UserJourney`

These APIs manage daily journey plans and activity tracking for delivery agents.

#### 4.1 Get All User Journeys
```http
POST /api/UserJourney/SelectAlUserJourneyDetails
```

**Request Body:**
```json
{
  "filterCriterias": [
    { "name": "EmpUID", "value": "emp-001", "filterType": "Equal" },
    { "name": "Date", "value": "2024-01-15", "filterType": "Equal" }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "isCountRequired": true
}
```

**Response:**
```json
{
  "pagedData": [
    {
      "uid": "journey-001",
      "empUID": "emp-001",
      "empName": "John Doe",
      "date": "2024-01-15",
      "routeUID": "route-001",
      "routeName": "North Zone Route 1",
      "status": "In Progress",
      "startTime": "09:00:00",
      "endTime": null,
      "totalStores": 15,
      "visitedStores": 8
    }
  ],
  "totalCount": 1
}
```

**Usage:** Display journey plans in delivery dashboard.

---

#### 4.2 Get Today's Journey Plan
```http
POST /api/UserJourney/SelectTodayJourneyPlanDetails
```

**Request Body:**
```json
{
  "filterCriterias": [],
  "pageNumber": 1,
  "pageSize": 50,
  "isCountRequired": true
}
```

**Query Parameters:**
- `Type` (string): "Delivery" or "Sales"
- `VisitDate` (DateTime): "2024-01-15"
- `JobPositionUID` (string): Filter by job position
- `OrgUID` (string): Organization UID

**Response:**
```json
{
  "pagedData": [
    {
      "uid": "plan-001",
      "empUID": "emp-001",
      "empName": "John Doe",
      "routeUID": "route-001",
      "routeName": "North Zone Route 1",
      "plannedStores": 15,
      "visitedStores": 8,
      "pendingStores": 7,
      "currentLocation": "ABC Store",
      "lastUpdateTime": "2024-01-15T14:30:00Z"
    }
  ],
  "totalCount": 10
}
```

**Usage:** Show live status of today's deliveries for all agents.

---

#### 4.3 Get Journey Plan Details by UID
```http
GET /api/UserJourney/GetUserJourneyDetailsByUID?UID={journeyUID}
```

**Response:**
```json
{
  "uid": "journey-001",
  "empUID": "emp-001",
  "empName": "John Doe",
  "date": "2024-01-15",
  "routeUID": "route-001",
  "routeName": "North Zone Route 1",
  "vehicleUID": "vh-001",
  "vehicleNumber": "MH01AB1234",
  "startTime": "09:00:00",
  "endTime": "17:30:00",
  "startOdometer": 12500,
  "endOdometer": 12680,
  "totalDistance": 180,
  "storeVisits": [
    {
      "storeUID": "store-001",
      "storeName": "ABC Store",
      "visitTime": "09:30:00",
      "checkInTime": "09:30:00",
      "checkOutTime": "10:15:00",
      "orderValue": 15000,
      "status": "Delivered"
    }
  ]
}
```

**Usage:** Show detailed journey activity log with all store visits.

---

#### 4.4 Get User Journey Grid
```http
POST /api/UserJourney/GetUserJourneyGridDetails
```

**Request Body:**
```json
{
  "filterCriterias": [
    { "name": "StartDate", "value": "2024-01-01", "filterType": "GreaterThanOrEqual" },
    { "name": "EndDate", "value": "2024-01-31", "filterType": "LessThanOrEqual" }
  ],
  "pageNumber": 1,
  "pageSize": 50,
  "isCountRequired": true
}
```

**Usage:** Display journey history in a grid/table format.

---

#### 4.5 Get Beat History Inner Grid
```http
POST /api/UserJourney/SelecteatHistoryInnerGridDetails?BeatHistoryUID={beatHistoryUID}
```

**Response:**
```json
[
  {
    "storeUID": "store-001",
    "storeName": "ABC Store",
    "visitDate": "2024-01-15",
    "checkInTime": "09:30:00",
    "checkOutTime": "10:15:00",
    "orderNumber": "SO-001",
    "orderValue": 15000,
    "paymentReceived": 10000,
    "remarks": "Partial payment"
  }
]
```

**Usage:** Show detailed activity for a specific beat/route execution.

---

### 5. Sales Order APIs
**Base URL:** `/api/SalesOrder`

These APIs handle orders that need to be delivered.

#### 5.1 Get All Sales Orders
```http
GET /api/SalesOrder/SelectSalesOrderDetailsAll
```

**Query Parameters:**
- `sortCriterias`: Sorting criteria
- `pageNumber`: Page number
- `pageSize`: Page size
- `filterCriterias`: Filter criteria

**Response:**
```json
[
  {
    "uid": "so-001",
    "orderNumber": "SO-2024-001",
    "storeUID": "store-001",
    "storeName": "ABC Store",
    "orderDate": "2024-01-15",
    "deliveryDate": "2024-01-16",
    "totalAmount": 25000,
    "status": "Pending Delivery",
    "routeUID": "route-001",
    "deliveryAgentUID": "emp-001"
  }
]
```

**Usage:** Show orders pending delivery or completed deliveries.

---

#### 5.2 Get Sales Order by UID
```http
GET /api/SalesOrder/SelectSalesOrderByUID?SalesOrderUID={orderUID}
```

**Response:**
```json
{
  "uid": "so-001",
  "orderNumber": "SO-2024-001",
  "storeUID": "store-001",
  "storeName": "ABC Store",
  "orderDate": "2024-01-15",
  "deliveryDate": "2024-01-16",
  "totalAmount": 25000,
  "status": "Pending Delivery",
  "lines": [
    {
      "uid": "sol-001",
      "skuUID": "sku-001",
      "skuName": "Product A",
      "quantity": 100,
      "unitPrice": 200,
      "lineTotal": 20000
    }
  ]
}
```

**Usage:** Show order details when preparing for delivery.

---

#### 5.3 Create Sales Order
```http
POST /api/SalesOrder/CreateSalesOrder
```

**Request Body:**
```json
{
  "salesOrder": {
    "uid": "so-new",
    "orderNumber": "SO-2024-NEW",
    "storeUID": "store-001",
    "orderDate": "2024-01-15",
    "deliveryDate": "2024-01-16",
    "routeUID": "route-001"
  },
  "salesOrderLines": [
    {
      "uid": "sol-new-1",
      "salesOrderUID": "so-new",
      "skuUID": "sku-001",
      "quantity": 50,
      "unitPrice": 200
    }
  ]
}
```

**Usage:** Create order that will be assigned to delivery route.

---

#### 5.4 Get Sales Order Print View
```http
GET /api/SalesOrder/GetSalesOrderPrintView?SalesOrderUID={orderUID}
```

**Usage:** Generate delivery note/invoice for printing.

---

#### 5.5 Get Delivered Pre-Sales Orders
```http
POST /api/SalesOrder/SelectDeliveredPreSales
```

**Request Body:**
```json
{
  "filterCriterias": [],
  "pageNumber": 1,
  "pageSize": 50,
  "isCountRequired": true
}
```

**Query Parameters:**
- `startDate` (DateTime): Start date filter
- `endDate` (DateTime): End date filter
- `Status` (string): Order status

**Response:**
```json
{
  "pagedData": [
    {
      "orderUID": "so-001",
      "orderNumber": "SO-2024-001",
      "deliveryDate": "2024-01-15",
      "storeName": "ABC Store",
      "deliveryAgent": "John Doe",
      "status": "Delivered",
      "amount": 25000
    }
  ],
  "totalCount": 150
}
```

**Usage:** Show delivered orders in manager dashboard for tracking.

---

## Manager/Warehouse Module APIs

### 6. Warehouse Stock APIs
**Base URL:** `/api/WHStock`

These APIs handle warehouse stock requests and load management.

#### 6.1 Create/Update/Delete Warehouse Stock
```http
POST /api/WHStock/CUDWHStock
```

**Request Body:**
```json
{
  "whStockRequest": {
    "uid": "wh-req-001",
    "requestNumber": "WHR-2024-001",
    "requestByEmpUID": "emp-001",
    "sourceOrgUID": "org-001",
    "destinationOrgUID": "wh-001",
    "requestDate": "2024-01-15",
    "status": "Pending"
  },
  "whStockRequestLines": [
    {
      "uid": "whrl-001",
      "whStockRequestUID": "wh-req-001",
      "skuUID": "sku-001",
      "requestedQuantity": 500,
      "approvedQuantity": 0
    }
  ]
}
```

**Usage:** Create stock transfer requests between warehouses.

---

#### 6.2 Create Stock Request via Queue
```http
POST /api/WHStock/CreateWHStockFromQueue
```

**Request Body:**
```json
[
  {
    "whStockRequest": { /* request details */ },
    "whStockRequestLines": [ /* line items */ ]
  }
]
```

**Usage:** Submit multiple stock requests for background processing (better for large volumes).

---

#### 6.3 Get Load Requests
```http
POST /api/WHStock/SelectLoadRequestData
```

**Request Body:**
```json
{
  "filterCriterias": [
    { "name": "Status", "value": "Pending", "filterType": "Equal" },
    { "name": "WarehouseUID", "value": "wh-001", "filterType": "Equal" }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "isCountRequired": true
}
```

**Query Parameters:**
- `StockType` (string): "LoadRequest" or "StockTransfer"

**Response:**
```json
{
  "pagedData": [
    {
      "uid": "wh-req-001",
      "requestNumber": "WHR-2024-001",
      "requestDate": "2024-01-15",
      "requestByEmp": "John Doe",
      "sourceWarehouse": "Main Warehouse",
      "destinationWarehouse": "Regional WH",
      "status": "Pending",
      "totalItems": 5,
      "totalQuantity": 1500
    }
  ],
  "totalCount": 25
}
```

**Usage:** Display pending stock requests in manager dashboard.

---

#### 6.4 Get Load Request by UID
```http
GET /api/WHStock/SelectLoadRequestDataByUID?UID={requestUID}
```

**Response:**
```json
{
  "request": {
    "uid": "wh-req-001",
    "requestNumber": "WHR-2024-001",
    "requestDate": "2024-01-15",
    "status": "Pending"
  },
  "lines": [
    {
      "uid": "whrl-001",
      "skuUID": "sku-001",
      "skuName": "Product A",
      "skuCode": "PA-001",
      "requestedQuantity": 500,
      "approvedQuantity": 450,
      "remarks": "Stock limited"
    }
  ]
}
```

**Usage:** Show detailed stock request for approval/processing.

---

#### 6.5 Create/Update Stock Request Lines
```http
POST /api/WHStock/CUDWHStockRequestLine
```

**Request Body:**
```json
[
  {
    "uid": "whrl-001",
    "whStockRequestUID": "wh-req-001",
    "skuUID": "sku-001",
    "requestedQuantity": 500,
    "approvedQuantity": 450
  }
]
```

**Usage:** Update individual line items in stock request.

---

### 7. Stock Updater APIs
**Base URL:** `/api/StockUpdater`

These APIs handle real-time stock updates and summaries.

#### 7.1 Update Stock
```http
POST /api/StockUpdater/UpdateStockAsync
```

**Request Body:**
```json
[
  {
    "uid": "stock-ledger-001",
    "orgUID": "wh-001",
    "skuUID": "sku-001",
    "quantity": 500,
    "transactionType": "StockIn",
    "referenceUID": "po-001",
    "referenceType": "PurchaseOrder",
    "transactionDate": "2024-01-15"
  }
]
```

**Usage:** Record stock movements (in/out) in warehouse.

---

#### 7.2 Get Warehouse Stock Summary
```http
GET /api/StockUpdater/GetWHStockSummary?orgUID={orgUID}&wareHouseUID={warehouseUID}
```

**Response:**
```json
[
  {
    "skuUID": "sku-001",
    "skuName": "Product A",
    "skuCode": "PA-001",
    "currentStock": 5000,
    "committedStock": 1200,
    "availableStock": 3800,
    "reorderLevel": 1000,
    "lastStockInDate": "2024-01-15",
    "warehouseUID": "wh-001"
  }
]
```

**Usage:** Display current stock levels in warehouse dashboard.

---

### 8. Purchase Order APIs
**Base URL:** `/api/PurchaseOrder`

These APIs manage purchase orders for incoming stock.

#### 8.1 Get Purchase Order Headers
```http
POST /api/PurchaseOrder/GetPurchaseOrderHeaders
```

**Request Body:**
```json
{
  "filterCriterias": [
    { "name": "Status", "value": "Approved", "filterType": "Equal" },
    { "name": "WarehouseUID", "value": "wh-001", "filterType": "Equal" }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "isCountRequired": true
}
```

**Response:**
```json
{
  "pagedData": [
    {
      "uid": "po-001",
      "poNumber": "PO-2024-001",
      "poDate": "2024-01-10",
      "supplierUID": "sup-001",
      "supplierName": "ABC Supplier",
      "warehouseUID": "wh-001",
      "totalAmount": 150000,
      "status": "Approved",
      "expectedDeliveryDate": "2024-01-20"
    }
  ],
  "totalCount": 45
}
```

**Usage:** Show purchase orders in manager dashboard for tracking.

---

#### 8.2 Create/Update/Delete Purchase Order
```http
POST /api/PurchaseOrder/CUD_PurchaseOrder
```

**Request Body:**
```json
[
  {
    "purchaseOrderHeader": {
      "uid": "po-new",
      "poNumber": "PO-2024-NEW",
      "poDate": "2024-01-15",
      "supplierUID": "sup-001",
      "warehouseUID": "wh-001"
    },
    "purchaseOrderLines": [
      {
        "uid": "pol-new-1",
        "purchaseOrderUID": "po-new",
        "skuUID": "sku-001",
        "quantity": 1000,
        "unitPrice": 150
      }
    ],
    "purchaseOrderLineProvisions": []
  }
]
```

**Usage:** Create/update purchase orders for stock replenishment.

---

## Distributor Management APIs

### 9. Distributor APIs
**Base URL:** `/api/Distributor`

These APIs manage distributor (wholesaler/partner) information.

#### 9.1 Get All Distributors
```http
POST /api/Distributor/SelectAllDistributors
```

**Request Body:**
```json
{
  "filterCriterias": [
    { "name": "OrgUID", "value": "org-001", "filterType": "Equal" },
    { "name": "IsActive", "value": true, "filterType": "Equal" }
  ],
  "pageNumber": 1,
  "pageSize": 50,
  "isCountRequired": true
}
```

**Response:**
```json
{
  "pagedData": [
    {
      "uid": "dist-001",
      "code": "DIST-001",
      "name": "ABC Distributors Pvt Ltd",
      "contactPerson": "Mr. Sharma",
      "phone": "+91-9876543210",
      "email": "contact@abcdist.com",
      "orgUID": "org-001",
      "territoryUID": "territory-001",
      "isActive": true
    }
  ],
  "totalCount": 15
}
```

**Usage:** Display distributor list for selection in routes, warehouses, etc.

---

#### 9.2 Get Distributor Details by UID
```http
GET /api/Distributor/GetDistributorDetailsByUID?UID={distributorUID}
```

**Response:**
```json
{
  "distributor": {
    "uid": "dist-001",
    "code": "DIST-001",
    "name": "ABC Distributors Pvt Ltd",
    "contactPerson": "Mr. Sharma",
    "phone": "+91-9876543210",
    "email": "contact@abcdist.com"
  },
  "addresses": [
    {
      "type": "Warehouse",
      "address": "Plot 123, Industrial Area",
      "city": "Mumbai",
      "state": "Maharashtra",
      "pincode": "400001"
    }
  ],
  "warehouses": [
    {
      "uid": "wh-001",
      "name": "Main Warehouse",
      "capacity": 10000
    }
  ]
}
```

**Usage:** Show complete distributor information with warehouses and addresses.

---

#### 9.3 Create Distributor
```http
POST /api/Distributor/CreateDistributor
```

**Request Body:**
```json
{
  "distributor": {
    "uid": "dist-new",
    "code": "DIST-NEW",
    "name": "New Distributor Ltd",
    "contactPerson": "Mr. Kumar",
    "phone": "+91-9876543211",
    "email": "contact@newdist.com",
    "orgUID": "org-001"
  },
  "addresses": [
    {
      "type": "Office",
      "address": "Office Address",
      "city": "Delhi",
      "state": "Delhi",
      "pincode": "110001"
    }
  ]
}
```

**Usage:** Onboard new distributor in the system.

---

#### 9.4 Manage Distributor Admin Users
```http
POST /api/Distributor/CUDDistributorAdmin
```

**Request Body:**
```json
{
  "emp": {
    "uid": "emp-new",
    "firstName": "Admin",
    "lastName": "User",
    "userName": "admin@newdist.com",
    "encryptedPassword": "password123",
    "orgUID": "dist-new"
  },
  "jobPosition": {
    "uid": "jp-new",
    "empUID": "emp-new",
    "roleUID": "role-distributor-admin"
  },
  "actionType": "Add"
}
```

**Usage:** Create/update distributor admin user accounts.

---

#### 9.5 Get Distributor Admins by Org
```http
GET /api/Distributor/SelectAllDistributorAdminDetailsByOrgUID?OrgUID={distributorOrgUID}
```

**Response:**
```json
[
  {
    "empUID": "emp-001",
    "userName": "admin@abcdist.com",
    "firstName": "Admin",
    "lastName": "User",
    "role": "Distributor Admin",
    "isActive": true
  }
]
```

**Usage:** Show distributor staff/admin users.

---

## Common/Shared APIs

### 10. Store/Customer APIs
**Base URL:** `/api/Store`

These APIs manage store/customer master data.

#### 10.1 Get All Stores
```http
POST /api/Store/SelectAllStore
```

**Request Body:**
```json
{
  "filterCriterias": [
    { "name": "Type", "value": "Retailer", "filterType": "Equal" },
    { "name": "Name", "value": "ABC", "filterType": "Contains" }
  ],
  "sortCriterias": [
    { "sortParameter": "Name", "direction": "Asc" }
  ],
  "pageNumber": 1,
  "pageSize": 50,
  "isCountRequired": true
}
```

**Response:**
```json
{
  "pagedData": [
    {
      "uid": "store-001",
      "code": "ST001",
      "name": "ABC Store",
      "type": "Retailer",
      "contactPerson": "Mr. Patel",
      "phone": "+91-9876543210",
      "address": "Shop 1, Main Street",
      "city": "Mumbai",
      "isActive": true
    }
  ],
  "totalCount": 250
}
```

**Usage:**
- Show customer list for route mapping
- Select customers for delivery
- Display store information in dashboards

---

#### 10.2 Get All Stores (Fast/Basic)
```http
POST /api/Store/SelectAllStoreBasic
```

**Note:** This is an optimized version (10x-20x faster) that returns basic store info without expensive JOINs.

**Usage:** Use when you only need basic store information for dropdowns or lists.

---

### 11. Customer APIs
**Base URL:** `/api/Customer`

APIs for detailed customer management (if different from Store APIs).

**Note:** Check if this controller exists separately or if customer management is handled via Store APIs.

---

## API Integration Examples

### Example 1: Building Delivery Dashboard

**Step 1: Get Today's Deliveries**
```typescript
const getTodaysDeliveries = async (orgUID: string) => {
  const response = await fetch('/api/UserJourney/SelectTodayJourneyPlanDetails', {
    method: 'POST',
    body: JSON.stringify({
      filterCriterias: [
        { name: 'OrgUID', value: orgUID, filterType: 'Equal' }
      ],
      pageNumber: 1,
      pageSize: 100,
      isCountRequired: true
    })
  });

  return response.json();
};
```

**Step 2: Get Route Details**
```typescript
const getRouteDetails = async (routeUID: string) => {
  const response = await fetch(
    `/api/Route/SelectRouteMasterViewByUID?UID=${routeUID}`
  );

  return response.json();
};
```

**Step 3: Get Pending Orders**
```typescript
const getPendingOrders = async (routeUID: string) => {
  const response = await fetch('/api/SalesOrder/SelectSalesOrderDetailsAll', {
    method: 'GET',
    params: {
      filterCriterias: [
        { name: 'RouteUID', value: routeUID, filterType: 'Equal' },
        { name: 'Status', value: 'Pending Delivery', filterType: 'Equal' }
      ]
    }
  });

  return response.json();
};
```

---

### Example 2: Building Manager Dashboard

**Step 1: Get Stock Summary**
```typescript
const getStockSummary = async (warehouseUID: string, orgUID: string) => {
  const response = await fetch(
    `/api/StockUpdater/GetWHStockSummary?orgUID=${orgUID}&wareHouseUID=${warehouseUID}`
  );

  return response.json();
};
```

**Step 2: Get Pending Stock Requests**
```typescript
const getPendingStockRequests = async (warehouseUID: string) => {
  const response = await fetch('/api/WHStock/SelectLoadRequestData', {
    method: 'POST',
    body: JSON.stringify({
      filterCriterias: [
        { name: 'DestinationOrgUID', value: warehouseUID, filterType: 'Equal' },
        { name: 'Status', value: 'Pending', filterType: 'Equal' }
      ],
      pageNumber: 1,
      pageSize: 50,
      isCountRequired: true
    })
  });

  return response.json();
};
```

**Step 3: Get Approved Purchase Orders**
```typescript
const getApprovedPOs = async (warehouseUID: string) => {
  const response = await fetch('/api/PurchaseOrder/GetPurchaseOrderHeaders', {
    method: 'POST',
    body: JSON.stringify({
      filterCriterias: [
        { name: 'WarehouseUID', value: warehouseUID, filterType: 'Equal' },
        { name: 'Status', value: 'Approved', filterType: 'Equal' }
      ],
      pageNumber: 1,
      pageSize: 20,
      isCountRequired: true
    })
  });

  return response.json();
};
```

---

### Example 3: Creating Dynamic Route with Customers

```typescript
const createRoute = async (routeData: any) => {
  const response = await fetch('/api/Route/CreateRouteMaster', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      route: {
        uid: generateUID(),
        name: routeData.name,
        code: routeData.code,
        orgUID: routeData.orgUID,
        warehouseUID: routeData.warehouseUID,
        vehicleUID: routeData.vehicleUID
      },
      routeSchedule: {
        uid: generateUID(),
        routeUID: routeData.routeUID,
        scheduleType: routeData.scheduleType,
        frequency: routeData.frequency
      },
      routeCustomersList: routeData.customers.map((customer: any, index: number) => ({
        routeUID: routeData.routeUID,
        storeUID: customer.storeUID,
        frequency: customer.frequency,
        sequenceNo: index + 1
      })),
      routeScheduleCustomerMappings: routeData.customers.map((customer: any) => ({
        routeScheduleUID: routeData.scheduleUID,
        customerUID: customer.storeUID,
        routeScheduleConfigUID: routeData.configUID
      }))
    })
  });

  return response.json();
};
```

---

## Complete API List Summary

### Delivery Module (11 Controllers, ~40 APIs)
| API Group | Base URL | Key Endpoints | Count |
|-----------|----------|---------------|-------|
| Route Management | `/api/Route` | SelectAll, Create, Update, Delete, GetByUID, GetMasterView, Dropdowns | 11 |
| Route Customer | `/api/RouteCustomer` | SelectAll, Create, Update, Delete, GetByStore | 6 |
| Route User | `/api/RouteUser` | SelectAll, Create, Update, Delete | 5 |
| Journey Plan | `/api/UserJourney` | SelectAll, GetToday, GetDetails, GetGrid, BeatHistory | 5 |
| Sales Order | `/api/SalesOrder` | SelectAll, GetByUID, Create, PrintView, DeliveredPreSales | 5 |

**Total Delivery APIs: ~32**

---

### Manager/Warehouse Module (8 Controllers, ~15 APIs)
| API Group | Base URL | Key Endpoints | Count |
|-----------|----------|---------------|-------|
| Warehouse Stock | `/api/WHStock` | CUD, CreateFromQueue, SelectLoadRequest, GetByUID, CUDLines | 5 |
| Stock Updater | `/api/StockUpdater` | UpdateStock, GetSummary | 2 |
| Purchase Order | `/api/PurchaseOrder` | GetHeaders, CUD_PurchaseOrder | 2+ |

**Total Manager APIs: ~15**

---

### Distributor Module (1 Controller, 5 APIs)
| API Group | Base URL | Key Endpoints | Count |
|-----------|----------|---------------|-------|
| Distributor | `/api/Distributor` | SelectAll, GetByUID, Create, CUDAdmin, GetAdmins | 5 |

**Total Distributor APIs: 5**

---

### Common/Shared Module (2+ Controllers, 10+ APIs)
| API Group | Base URL | Key Endpoints | Count |
|-----------|----------|---------------|-------|
| Store/Customer | `/api/Store` | SelectAll, SelectBasic, GetByUID, Create, Update | 5+ |
| Customer | `/api/Customer` | (if separate controller exists) | TBD |

**Total Common APIs: ~10**

---

## Grand Total: ~62 APIs

### Breakdown:
- **Delivery Module:** 32 APIs
- **Manager/Warehouse Module:** 15 APIs
- **Distributor Module:** 5 APIs
- **Common/Shared Module:** 10 APIs

---

## Key Patterns in All APIs

### 1. Pagination Request Format
All list APIs use this format:
```json
{
  "sortCriterias": [
    { "sortParameter": "FieldName", "direction": "Asc|Desc" }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "filterCriterias": [
    { "name": "FieldName", "value": "value", "filterType": "Equal|Contains|GreaterThan|LessThan" }
  ],
  "isCountRequired": true
}
```

### 2. Pagination Response Format
```json
{
  "pagedData": [ /* array of data */ ],
  "totalCount": 100
}
```

### 3. Common Filter Types
- `Equal` - Exact match
- `Contains` - Partial match (for text search)
- `GreaterThan`, `GreaterThanOrEqual` - Numeric/date comparison
- `LessThan`, `LessThanOrEqual` - Numeric/date comparison
- `In` - Multiple values

### 4. Dropdown/Selection Item Format
```json
[
  { "value": "uid-001", "text": "Display Name" }
]
```

### 5. Standard Response Format
```json
{
  "success": true,
  "data": { /* response data */ },
  "message": "Success message",
  "errors": []
}
```

---

## Next Steps for Frontend Integration

### 1. Create Service Layer
Create TypeScript services for each API group:
- `route.service.ts`
- `route-customer.service.ts`
- `route-user.service.ts`
- `journey.service.ts`
- `sales-order.service.ts`
- `warehouse-stock.service.ts`
- `stock-updater.service.ts`
- `purchase-order.service.ts`
- `distributor.service.ts`
- `store.service.ts`

### 2. Create Type Definitions
Define TypeScript interfaces for all request/response objects:
- `route.types.ts`
- `warehouse.types.ts`
- `distributor.types.ts`
- etc.

### 3. Update Frontend Components
Replace hardcoded data with API calls in:
- Delivery dashboard pages
- Manager dashboard pages
- Route management pages
- Stock management pages

### 4. Implement Caching Strategy
- Cache frequently accessed data (routes, stores, distributors)
- Use React Query or SWR for data fetching
- Implement optimistic updates

### 5. Add Real-time Updates
- Use SignalR/WebSockets for live delivery tracking
- Update dashboard automatically when data changes
- Show notifications for important events

---

## API Authentication & Authorization

All APIs require:
1. **JWT Token** in Authorization header: `Bearer {token}`
2. **Proper user permissions** based on role
3. **Organization context** (OrgUID) in requests

Example request:
```typescript
fetch('/api/Route/SelectAllRouteDetails', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${authToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(requestData)
});
```

---

## Error Handling

All APIs return errors in this format:
```json
{
  "success": false,
  "message": "Error occurred while processing the request.",
  "errors": [
    {
      "field": "FieldName",
      "message": "Validation error message"
    }
  ]
}
```

Common HTTP Status Codes:
- `200` - Success
- `201` - Created
- `400` - Bad Request (validation error)
- `401` - Unauthorized (missing/invalid token)
- `403` - Forbidden (no permission)
- `404` - Not Found
- `500` - Internal Server Error

---

## Performance Considerations

### 1. Use Pagination
Always paginate large datasets. Don't fetch all records at once.

### 2. Filter at API Level
Apply filters in API request rather than client-side filtering.

### 3. Use Basic APIs When Possible
Use `SelectAllStoreBasic` instead of `SelectAllStore` when you don't need full details.

### 4. Batch Operations
Use bulk APIs (like `CreateRouteUser` with array) instead of multiple single requests.

### 5. Optimize Page Size
Start with small page size (20-50) and increase based on needs.

---

## Testing Checklist

- [ ] Test pagination on all list APIs
- [ ] Test filtering with various criteria
- [ ] Test sorting (ascending/descending)
- [ ] Test create/update/delete operations
- [ ] Test error scenarios (invalid data, missing required fields)
- [ ] Test with different user roles/permissions
- [ ] Test dropdown APIs return correct data
- [ ] Test nested data (route with customers, journey with visits)
- [ ] Load test with large datasets
- [ ] Test queue-based APIs (WHStock CreateFromQueue)

---

## Conclusion

This document covers **62+ backend APIs** across **4 major modules** that are needed to make the delivery and manager features work dynamically.

**Key Takeaways:**
1. All APIs support **pagination, filtering, and sorting**
2. Use **batch operations** where available for better performance
3. **Dropdown APIs** provide data for form selections
4. **Master View APIs** return complete nested data
5. **Queue-based APIs** handle background processing for heavy operations

**Next Action:**
Create service layer in frontend to consume these APIs and replace all hardcoded data with dynamic API calls.
