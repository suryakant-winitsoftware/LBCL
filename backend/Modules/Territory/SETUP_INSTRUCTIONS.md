# Territory Module Setup Instructions

## Database Setup

### Option 1: Using SQL Server Management Studio (SSMS)
1. Open SQL Server Management Studio
2. Connect to server: `10.20.53.175`
3. Open the file: `territory_schema.sql`
4. Execute the script (F5)
5. Verify table creation by running:
   ```sql
   SELECT * FROM territory
   ```

### Option 2: Using sqlcmd (Command Line)
```bash
sqlcmd -S 10.20.53.175 -d LBSSFADev -U lbssfadev -P lbssfadev -i territory_schema.sql
```

### Option 3: Using Azure Data Studio
1. Open Azure Data Studio
2. Connect to server: `10.20.53.175`
3. Database: `LBSSFADev`
4. Open and run: `territory_schema.sql`

## Verify Installation

After creating the table, verify it exists:

```sql
-- Check if table exists
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'territory'

-- View table structure
EXEC sp_help 'territory'

-- Check indexes
EXEC sp_helpindex 'territory'
```

## Test Data (Optional)

To insert sample test data, you need to first get the actual ORG UID:

```sql
-- Get your organization UID
SELECT uid, name FROM org WHERE name LIKE '%LBCL%'

-- Then insert sample data (replace 'YOUR_ORG_UID' with actual value)
INSERT INTO territory (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                      org_uid, territory_code, territory_name, manager_emp_uid, cluster_code, parent_uid, item_level, has_child,
                      is_import, is_local, is_non_license, status, is_active)
VALUES
('TEST001', 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), GETDATE(), GETDATE(),
 'YOUR_ORG_UID', 'TEST001', 'Test Territory 1', NULL, NULL, NULL, 0, 0, 0, 1, 0, 'Active', 1);
```

## Backend Service Registration

✅ Already completed! The following has been added to `Startup.cs`:
- Territory Model Interface
- Territory BL (Business Logic)
- Territory DL (Data Layer) with MSSQL support

## API Endpoints

Once the table is created, the following endpoints will be available:

- `POST /api/Territory/SelectAllTerritories` - Get all territories with pagination
- `GET /api/Territory/GetTerritoryByUID?UID={uid}` - Get territory by UID
- `GET /api/Territory/GetTerritoryByCode?territoryCode={code}&orgUID={orgUID}` - Get territory by code
- `GET /api/Territory/GetTerritoriesByOrg?orgUID={orgUID}` - Get territories by organization
- `GET /api/Territory/GetTerritoriesByManager?managerEmpUID={empUID}` - Get territories by manager
- `GET /api/Territory/GetTerritoriesByCluster?clusterCode={code}` - Get territories by cluster
- `POST /api/Territory/CreateTerritory` - Create new territory
- `PUT /api/Territory/UpdateTerritory` - Update existing territory
- `DELETE /api/Territory/DeleteTerritory?UID={uid}` - Delete territory

## Next Steps

1. ✅ Create territory table in database (run territory_schema.sql)
2. ✅ Restart the backend API server
3. ✅ Test API endpoints using Swagger or Postman
4. ✅ Add navigation menu item in the frontend
5. ✅ Test the UI at `/administration/territory-management/territories`
