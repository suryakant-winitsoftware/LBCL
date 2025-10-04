import pyodbc
import uuid
from datetime import datetime

# Connection string
conn_str = (
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=localhost;"
    "DATABASE=WINITDB;"
    "UID=SA;"
    "PWD=YourStrong@Passw0rd"
)

try:
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()

    # Get Administration module UID
    cursor.execute("SELECT UID FROM Modules WHERE ModuleName = 'Administration'")
    admin_row = cursor.fetchone()

    if not admin_row:
        print("❌ Administration module not found")
        exit(1)

    admin_uid = admin_row[0]
    print(f"✓ Found Administration module: {admin_uid}")

    # Get or verify Warehouse Management module
    cursor.execute(
        "SELECT UID FROM Modules WHERE ModuleName = 'Warehouse Management' AND ParentUID = ?",
        admin_uid
    )
    wh_mgmt_row = cursor.fetchone()

    if not wh_mgmt_row:
        print("❌ Warehouse Management module not found")
        exit(1)

    wh_mgmt_uid = wh_mgmt_row[0]
    print(f"✓ Found Warehouse Management module: {wh_mgmt_uid}")

    # Check if Warehouses submodule already exists
    cursor.execute(
        "SELECT UID FROM Modules WHERE ModuleName = 'Warehouses' AND ParentUID = ?",
        wh_mgmt_uid
    )

    if cursor.fetchone():
        print("ℹ Warehouses submodule already exists")
    else:
        # Add Warehouses submodule
        new_uid = str(uuid.uuid4())
        now = datetime.now()

        cursor.execute("""
            INSERT INTO Modules (UID, ModuleName, ParentUID, NavigationRoute, OrderNumber, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
        """, new_uid, 'Warehouses', wh_mgmt_uid, '/administration/warehouse-management/warehouses', 1, 'SYSTEM', now, 'SYSTEM', now)

        conn.commit()
        print(f"✓ Warehouses submodule added successfully: {new_uid}")

    # Display the hierarchy
    print("\n📋 Warehouse Management Hierarchy:")
    cursor.execute("""
        SELECT
            m3.ModuleName AS 'Submodule',
            m3.NavigationRoute AS 'Route',
            m3.OrderNumber AS 'Order'
        FROM Modules m3
        JOIN Modules m2 ON m3.ParentUID = m2.UID
        JOIN Modules m1 ON m2.ParentUID = m1.UID
        WHERE m1.ModuleName = 'Administration'
        AND m2.ModuleName = 'Warehouse Management'
        ORDER BY m3.OrderNumber
    """)

    for row in cursor.fetchall():
        print(f"  - {row[0]:25} → {row[1]:50} (Order: {row[2]})")

    cursor.close()
    conn.close()
    print("\n✅ Done!")

except Exception as e:
    print(f"❌ Error: {e}")
    import traceback
    traceback.print_exc()
