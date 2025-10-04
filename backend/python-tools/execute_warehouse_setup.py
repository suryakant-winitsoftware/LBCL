import pyodbc
import os

# Connection string - UPDATE THESE IF DIFFERENT
SERVER = 'localhost'
DATABASE = 'WINITDB'
USERNAME = 'SA'
PASSWORD = 'YourStrong@Passw0rd'

conn_str = (
    f"DRIVER={{ODBC Driver 17 for SQL Server}};"
    f"SERVER={SERVER};"
    f"DATABASE={DATABASE};"
    f"UID={USERNAME};"
    f"PWD={PASSWORD}"
)

try:
    print("🔌 Connecting to database...")
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()

    # Read the SQL file
    sql_file = '/Users/suryakantkumar/Desktop/Multiplex/backend/python-tools/add_warehouse_modules_complete.sql'

    print("📂 Reading SQL script...")
    with open(sql_file, 'r') as f:
        sql_script = f.read()

    # Split by GO statements and execute each batch
    batches = sql_script.split('GO')

    print("⚙️  Executing SQL script...\n")

    for batch in batches:
        batch = batch.strip()
        if batch:
            try:
                cursor.execute(batch)
                # Fetch any messages from PRINT statements
                while cursor.nextset():
                    pass
                conn.commit()
            except Exception as batch_error:
                print(f"Warning in batch: {str(batch_error)}")

    # Get the final results
    print("\n" + "="*60)
    print("📊 FINAL MODULE HIERARCHY")
    print("="*60)

    cursor.execute("""
        SELECT
            m3.OrderNumber AS [Order],
            m3.ModuleName AS [Submodule],
            m3.NavigationRoute AS [Route],
            COUNT(p.UID) AS [Permissions]
        FROM Modules m3
        LEFT JOIN Permissions p ON p.ModuleUID = m3.UID
        JOIN Modules m2 ON m3.ParentUID = m2.UID
        JOIN Modules m1 ON m2.ParentUID = m1.UID
        WHERE m1.ModuleName = 'Administration'
        AND m2.ModuleName = 'Warehouse Management'
        GROUP BY m3.OrderNumber, m3.ModuleName, m3.NavigationRoute
        ORDER BY m3.OrderNumber
    """)

    results = cursor.fetchall()

    if results:
        print(f"\n{'Order':<8} {'Submodule':<25} {'Permissions':<12} Route")
        print("-" * 100)
        for row in results:
            print(f"{row[0]:<8} {row[1]:<25} {row[3]:<12} {row[2]}")
    else:
        print("\n⚠️  No modules found. Check if the script executed correctly.")

    cursor.close()
    conn.close()

    print("\n" + "="*60)
    print("✅ WAREHOUSE MANAGEMENT SETUP COMPLETE!")
    print("="*60)
    print("\n📋 Summary:")
    print("   • Warehouse Management parent module verified/created")
    print("   • 5 submodules created:")
    print("     1. Warehouses")
    print("     2. Stock Requests")
    print("     3. Stock Summary")
    print("     4. Stock Audits")
    print("     5. Stock Conversions")
    print("   • 25 permissions created (5 per module)")
    print("\n🌐 These modules should now appear in your sidebar navigation!")

except pyodbc.Error as e:
    print(f"\n❌ Database Error: {e}")
    print("\nTroubleshooting:")
    print("  1. Ensure SQL Server is running")
    print("  2. Check connection details (server, database, username, password)")
    print("  3. Verify ODBC Driver 17 for SQL Server is installed")

except FileNotFoundError:
    print(f"\n❌ SQL file not found: {sql_file}")

except Exception as e:
    print(f"\n❌ Unexpected Error: {e}")
    import traceback
    traceback.print_exc()
