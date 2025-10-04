# Territory Module Setup

## Quick Start - Run Python Script

### Prerequisites
1. Python 3.7 or higher
2. ODBC Driver 17 for SQL Server

### Install ODBC Driver (if not already installed)

**macOS:**
```bash
brew install unixodbc
brew tap microsoft/mssql-release https://github.com/Microsoft/homebrew-mssql-release
brew install msodbcsql17 mssql-tools
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt-get update
sudo apt-get install -y unixodbc unixodbc-dev
curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list
sudo apt-get update
sudo ACCEPT_EULA=Y apt-get install -y msodbcsql17
```

**Windows:**
Download and install from: https://learn.microsoft.com/en-us/sql/connect/odbc/download-odbc-driver-for-sql-server

### Setup and Run

1. **Install Python dependencies:**
```bash
cd backend/Modules/Territory
pip install -r requirements.txt
```

2. **Run the setup script:**
```bash
python3 create_territory_table.py
```

The script will:
- ✓ Connect to the SQL Server database
- ✓ Read the territory_schema.sql file
- ✓ Execute SQL commands to create the table
- ✓ Create all indexes
- ✓ Verify table creation
- ✓ Display table structure

### Expected Output

```
================================================================================
Territory Table Creation Script
================================================================================

Connecting to SQL Server: 10.20.53.175
Database: LBSSFADev
✓ Successfully connected to database
✓ Read SQL file: territory_schema.sql

Executing SQL commands...
✓ Command 1 executed successfully
✓ Command 2 executed successfully

✓ SQL script execution completed

✓ Territory table created successfully
✓ Table has 21 columns
✓ Table has 6 indexes

Table Structure:
--------------------------------------------------------------------------------
Column Name                    Data Type            Nullable
--------------------------------------------------------------------------------
id                             int                  NO
uid                            nvarchar(250)        NO
created_by                     nvarchar(250)        NO
created_time                   datetime2            YES
...
--------------------------------------------------------------------------------

✓ Database connection closed

================================================================================
Territory table setup completed successfully!
================================================================================

Next steps:
1. Restart the backend API server
2. Test the API endpoints at: http://localhost:8000/api/Territory
3. Access the UI at: /administration/territory-management/territories
```

## Manual Setup (Alternative)

If you prefer to run SQL manually, see [SETUP_INSTRUCTIONS.md](./SETUP_INSTRUCTIONS.md)

## Troubleshooting

### Error: "pyodbc.InterfaceError: ('IM002', ...)"
**Solution:** Install ODBC Driver 17 for SQL Server (see prerequisites above)

### Error: "Connection failed"
**Solution:**
- Verify database server is accessible: `ping 10.20.53.175`
- Check credentials in the script
- Ensure firewall allows SQL Server connections (port 1433)

### Error: "Table already exists"
**Solution:** The table is already created. You can verify by connecting to the database:
```sql
SELECT * FROM territory
```

## Verification

After setup, verify the table exists:

```bash
# Using Python
python3 -c "
import pyodbc
conn = pyodbc.connect('DRIVER={ODBC Driver 17 for SQL Server};SERVER=10.20.53.175;DATABASE=LBSSFADev;UID=lbssfadev;PWD=lbssfadev;TrustServerCertificate=yes')
cursor = conn.cursor()
cursor.execute('SELECT COUNT(*) FROM territory')
print(f'Territory table exists with {cursor.fetchone()[0]} records')
conn.close()
"
```

## API Endpoints

Once the table is created and backend is restarted, these endpoints will be available:

- `POST /api/Territory/SelectAllTerritories` - Get all territories
- `GET /api/Territory/GetTerritoryByUID?UID={uid}` - Get by UID
- `POST /api/Territory/CreateTerritory` - Create new territory
- `PUT /api/Territory/UpdateTerritory` - Update territory
- `DELETE /api/Territory/DeleteTerritory?UID={uid}` - Delete territory

## Support

For issues, check:
1. Database connection string in `appsettings.Development.json`
2. Territory services registered in `Startup.cs` (✓ Already added)
3. Backend server logs for any errors
