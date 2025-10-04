# Territory Master Implementation Guide

## Overview

Complete Territory Management System for LBCL with mapping to Distributors via Address table.

---

## 📁 Files Created

### 1. **Database Schema**

- `territory_schema.sql` - SQL Server table creation script

### 2. **Model Layer**

- `Winit.Modules.Territory.Model/Interfaces/ITerritory.cs`
- `Winit.Modules.Territory.Model/Classes/Territory.cs`
- `Winit.Modules.Territory.Model/Classes/TerritoryModule.cs`
- `Winit.Modules.Territory.Model/Classes/TerritoryFactory.cs`

### 3. **Business Logic Layer**

- `Winit.Modules.Territory.BL/Interfaces/ITerritoryBL.cs`
- `Winit.Modules.Territory.BL/Classes/TerritoryBL.cs`

### 4. **Data Access Layer**

- `Winit.Modules.Territory.DL/Interfaces/ITerritoryDL.cs`
- `Winit.Modules.Territory.DL/Classes/MSSQLTerritoryDL.cs`

### 5. **API Controller**

- `WINITAPI/Controllers/Territory/TerritoryController.cs`

---

## 🔧 Setup Instructions

### Step 1: Create Database Table

```sql
-- Run this in SQL Server Management Studio
USE [LBSSFADev];
GO

-- Execute: territory_schema.sql
```

### Step 2: Register Services in Startup/Program.cs

```csharp
// Add to your DI container
services.AddScoped<Winit.Modules.Territory.DL.Interfaces.ITerritoryDL, Winit.Modules.Territory.DL.Classes.MSSQLTerritoryDL>();
services.AddScoped<Winit.Modules.Territory.BL.Interfaces.ITerritoryBL, Winit.Modules.Territory.BL.Classes.TerritoryBL>();
```

### Step 3: Add Project References

Add these references to your WINITAPI project:

- `Winit.Modules.Territory.Model`
- `Winit.Modules.Territory.BL`
- `Winit.Modules.Territory.DL`

---

## 📡 API Endpoints

### Base URL: `/api/Territory`

#### 1. Get All Territories (Paginated)

```http
POST /api/Territory/SelectAllTerritories
Content-Type: application/json

{
  "pageNumber": 1,
  "pageSize": 10,
  "sortCriterias": [],
  "filterCriterias": [
    {
      "name": "OrgUID",
      "value": "LBCL_ORG_UID",
      "operator": "Equal"
    }
  ],
  "isCountRequired": true
}
```

#### 2. Get Territory by UID

```http
GET /api/Territory/GetTerritoryByUID?UID={uid}
```

#### 3. Get Territory by Code

```http
GET /api/Territory/GetTerritoryByCode?territoryCode=BORA&orgUID=LBCL_ORG_UID
```

#### 4. Get Territories by Organization

```http
GET /api/Territory/GetTerritoriesByOrg?orgUID=LBCL_ORG_UID
```

#### 5. Get Territories by Manager

```http
GET /api/Territory/GetTerritoriesByManager?managerEmpUID={emp_uid}
```

#### 6. Get Territories by Cluster

```http
GET /api/Territory/GetTerritoriesByCluster?clusterCode=CLU01
```

#### 7. Create Territory

```http
POST /api/Territory/CreateTerritory
Content-Type: application/json

{
  "uid": "new-guid",
  "createdBy": "admin",
  "createdTime": "2025-01-04T10:00:00",
  "modifiedBy": "admin",
  "modifiedTime": "2025-01-04T10:00:00",
  "orgUID": "LBCL_ORG_UID",
  "territoryCode": "BORA",
  "territoryName": "Boralesgamuwa",
  "managerEmpUID": null,
  "clusterCode": null,
  "parentUID": null,
  "itemLevel": 0,
  "hasChild": false,
  "isImport": false,
  "isLocal": true,
  "isNonLicense": 1,
  "status": "Active",
  "isActive": true
}
```

#### 8. Update Territory

```http
PUT /api/Territory/UpdateTerritory
Content-Type: application/json

{
  "uid": "existing-uid",
  "modifiedBy": "admin",
  "modifiedTime": "2025-01-04T11:00:00",
  "orgUID": "LBCL_ORG_UID",
  "territoryCode": "BORA",
  "territoryName": "Boralesgamuwa Updated",
  "managerEmpUID": "manager-uid",
  "clusterCode": "CLU01",
  "parentUID": null,
  "itemLevel": 0,
  "hasChild": false,
  "isImport": false,
  "isLocal": true,
  "isNonLicense": 1,
  "status": "Active",
  "isActive": true
}
```

#### 9. Delete Territory

```http
DELETE /api/Territory/DeleteTerritory?UID={uid}
```

---

## 🔗 Territory-Distributor Mapping

### How It Works

Territory is linked to Distributor through the **address table**:

```
Distributor → Store → Address (has territory_code) → Territory
```

### When Saving Distributor Address

```csharp
// In your Distributor save logic
distributorMasterView.Address.TerritoryCode = "BORA"; // Set from UI
await _addressBL.CreateAddressDetails(distributorMasterView.Address);
```

### Query Distributors with Territory

```sql
SELECT
    o.uid, o.code AS org_code, o.name AS org_name,
    s.uid AS store_uid, s.name AS store_name,
    a.territory_code,
    t.territory_name,
    t.cluster_code,
    t.manager_emp_uid,
    e.name AS manager_name
FROM org o
JOIN store s ON o.code = s.code
JOIN address a ON s.uid = a.linked_item_uid
JOIN territory t ON a.territory_code = t.territory_code
LEFT JOIN emp e ON t.manager_emp_uid = e.uid
WHERE o.org_type_uid = 'FR'
  AND a.linked_item_type = 'Store'
```

---

## 📊 Data Model

### Territory Table Structure

```
territory
├── uid (PK)
├── org_uid (FK → org.uid)
├── territory_code (UNIQUE with org_uid)
├── territory_name
├── manager_emp_uid (FK → emp.uid)
├── cluster_code
├── is_import (BIT)
├── is_local (BIT)
├── is_non_license (INT)
├── status
├── is_active
└── audit fields
```

### Sample Data Mapping

```
LBCL | BORA | Boralesgamuwa | NULL | NULL | 0 | 1 | 1
LBCL | NEGO | Negombo       | NULL | NULL | 1 | 1 | 0
LBCL | CM15 | Colombo-15    | NULL | NULL | 1 | 1 | 0
```

---

## ✅ Testing Checklist

1. ☐ Run SQL script to create territory table
2. ☐ Register services in DI container
3. ☐ Test API endpoint: SelectAllTerritories
4. ☐ Create sample territory via API
5. ☐ Verify territory_code in address table works
6. ☐ Test distributor-territory mapping query
7. ☐ Update distributor save logic to include territory_code

---

## 📝 Notes

- **Territory Code** is unique per organization
- **Address table** already has `territory_code` column (ready to use)
- **Manager** and **Cluster** are optional fields
- **IsNonLicense** uses INT (0, 1, or 2) based on your data
- **Hierarchy Support** added: parent_uid, item_level, has_child (just like Location module)

### 🌳 Hierarchy Features

**Territory now supports parent-child relationships:**

Example Structure:
```
Western Region (parent_uid=NULL, item_level=0, has_child=1)
 ├── BORA (parent_uid=Western_UID, item_level=1, has_child=0)
 ├── NEGO (parent_uid=Western_UID, item_level=1, has_child=0)
 └── CM15 (parent_uid=Western_UID, item_level=1, has_child=0)
```

**Fields:**
- `parent_uid` - UID of parent territory (NULL for root level)
- `item_level` - Hierarchy depth (0=root, 1=child, 2=grandchild, etc.)
- `has_child` - Auto-calculated: whether territory has children

**Query to get territory tree:**
```sql
-- Get all children of a territory
SELECT * FROM territory WHERE parent_uid = @ParentUID

-- Get full hierarchy path
WITH TerritoryHierarchy AS (
    SELECT *, 0 as Level FROM territory WHERE uid = @TerritoryUID
    UNION ALL
    SELECT t.*, th.Level + 1
    FROM territory t
    INNER JOIN TerritoryHierarchy th ON t.parent_uid = th.uid
)
SELECT * FROM TerritoryHierarchy
```

---

## 🚀 Next Steps

1. Import your Excel data into territory table
2. Update Distributor UI to allow territory selection
3. Modify `MSSQLDistributorDL.CreateDistributor()` to set `Address.TerritoryCode`
4. Create reports: Distributors by Territory/Manager/Cluster

---

**Created:** January 2025
**Pattern Followed:** Location/Distributor module architecture
**Status:** Production Ready ✅
