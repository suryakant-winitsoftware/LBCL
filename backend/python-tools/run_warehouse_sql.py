#!/usr/bin/env python3
import pyodbc
import sys

# MSSQL Connection details
SERVER = '10.20.53.175'
DATABASE = 'LBSSFADev'
USERNAME = 'lbssfadev'
PASSWORD = 'lbssfadev'

try:
    # Build connection string
    conn_str = (
        f"DRIVER={{ODBC Driver 17 for SQL Server}};"
        f"SERVER={SERVER};"
        f"DATABASE={DATABASE};"
        f"UID={USERNAME};"
        f"PWD={PASSWORD};"
        "TrustServerCertificate=yes;"
    )

    print("🔌 Connecting to MSSQL database...")
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    print("✓ Connected successfully\n")

    # Read the SQL file
    sql_file = '/Users/suryakantkumar/Desktop/Multiplex/backend/python-tools/add_warehouses_submodule.sql'

    with open(sql_file, 'r', encoding='utf-8') as f:
        sql_script = f.read()

    print("📝 Executing SQL script...\n")

    # Execute the entire script
    cursor.execute(sql_script)

    # Commit the transaction
    conn.commit()

    print("\n" + "="*60)
    print("✅ WAREHOUSE SUBMODULE ADDED SUCCESSFULLY!")
    print("="*60)

    # Verify the result
    print("\n📊 Warehouse Management Submodules:")
    cursor.execute("""
        SELECT
            m.ModuleName,
            m.NavigationRoute,
            m.OrderNumber,
            COUNT(p.UID) as PermissionCount
        FROM Modules m
        LEFT JOIN Permissions p ON p.ModuleUID = m.UID
        WHERE m.ParentUID IN (
            SELECT UID FROM Modules
            WHERE ModuleName = 'Warehouse Management'
        )
        GROUP BY m.ModuleName, m.NavigationRoute, m.OrderNumber
        ORDER BY m.OrderNumber
    """)

    print(f"\n{'Module':<25} {'Route':<55} {'Order':<7} {'Perms'}")
    print("-" * 100)

    for row in cursor.fetchall():
        print(f"{row[0]:<25} {row[1]:<55} {row[2]:<7} {row[3]}")

    print("\n🎉 The 'Warehouses' submodule should now appear in your sidebar!")

    cursor.close()
    conn.close()

except pyodbc.Error as e:
    print(f"\n❌ Database Error:")
    print(f"   {e}")
    print("\n💡 Troubleshooting:")
    print("   • Is SQL Server running?")
    print("   • Check server name, database, username, password")
    print("   • Verify ODBC Driver 17 for SQL Server is installed")
    sys.exit(1)

except FileNotFoundError as e:
    print(f"\n❌ File not found: {e}")
    sys.exit(1)

except Exception as e:
    print(f"\n❌ Unexpected Error:")
    print(f"   {e}")
    import traceback
    traceback.print_exc()
    sys.exit(1)
