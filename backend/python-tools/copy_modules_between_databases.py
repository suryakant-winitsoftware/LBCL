#!/usr/bin/env python3
"""
Copy Modules, Sub-Modules, and Sub-Sub-Modules between PostgreSQL databases
Source: multiplexdev170725FM
Target: multiplexdev15072025
"""

import psycopg2
from psycopg2.extras import RealDictCursor
import sys

# Database connection parameters
SOURCE_DB = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev170725FM',
    'user': 'multiplex',
    'password': 'multiplex'
}

TARGET_DB = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev15072025',
    'user': 'multiplex',
    'password': 'multiplex'
}


def connect_db(db_params):
    """Connect to PostgreSQL database."""
    try:
        conn = psycopg2.connect(**db_params)
        print(f"✓ Connected to {db_params['database']}")
        return conn
    except Exception as e:
        print(f"✗ Error connecting to {db_params['database']}: {e}")
        sys.exit(1)


def get_all_modules(conn):
    """Get all modules from source database."""
    cursor = conn.cursor(cursor_factory=RealDictCursor)
    cursor.execute("SELECT * FROM modules ORDER BY serial_no")
    modules = [dict(row) for row in cursor.fetchall()]
    cursor.close()
    return modules


def get_all_sub_modules(conn):
    """Get all sub-modules from source database."""
    cursor = conn.cursor(cursor_factory=RealDictCursor)
    cursor.execute("SELECT * FROM sub_modules ORDER BY serial_no")
    sub_modules = [dict(row) for row in cursor.fetchall()]
    cursor.close()
    return sub_modules


def get_all_sub_sub_modules(conn):
    """Get all sub-sub-modules from source database."""
    cursor = conn.cursor(cursor_factory=RealDictCursor)
    cursor.execute("SELECT * FROM sub_sub_modules ORDER BY serial_no")
    sub_sub_modules = [dict(row) for row in cursor.fetchall()]
    cursor.close()
    return sub_sub_modules


def get_permissions(conn, table_name):
    """Get all permissions from specified table."""
    cursor = conn.cursor(cursor_factory=RealDictCursor)
    cursor.execute(f"SELECT * FROM {table_name}")
    permissions = [dict(row) for row in cursor.fetchall()]
    cursor.close()
    return permissions


def record_exists(conn, table_name, uid):
    """Check if record exists in target database."""
    cursor = conn.cursor()
    cursor.execute(f"SELECT 1 FROM {table_name} WHERE uid = %s", (uid,))
    exists = cursor.fetchone() is not None
    cursor.close()
    return exists


def insert_module(conn, table_name, record):
    """Insert record into target database."""
    cursor = conn.cursor()

    # Get column names
    columns = list(record.keys())
    # Remove 'id' if it exists (auto-increment)
    if 'id' in columns:
        columns.remove('id')

    # Build insert query
    placeholders = ', '.join(['%s'] * len(columns))
    columns_str = ', '.join(columns)
    query = f"INSERT INTO {table_name} ({columns_str}) VALUES ({placeholders})"

    # Get values in the same order as columns
    values = tuple(record[col] for col in columns)

    cursor.execute(query, values)
    cursor.close()


def main():
    """Main function to copy modules and permissions."""

    print("=" * 70)
    print("Copying Modules, Sub-Modules, and Sub-Sub-Modules")
    print("Source: multiplexdev170725FM -> Target: multiplexdev15072025")
    print("=" * 70)
    print()

    # Connect to both databases
    source_conn = connect_db(SOURCE_DB)
    target_conn = connect_db(TARGET_DB)

    try:
        stats = {
            'modules_added': 0,
            'modules_skipped': 0,
            'sub_modules_added': 0,
            'sub_modules_skipped': 0,
            'sub_sub_modules_added': 0,
            'sub_sub_modules_skipped': 0,
            'normal_permissions_added': 0,
            'normal_permissions_skipped': 0,
            'principal_permissions_added': 0,
            'principal_permissions_skipped': 0,
        }

        # 1. Copy Modules
        print("\n📋 Copying Modules...")
        modules = get_all_modules(source_conn)
        print(f"   Found {len(modules)} modules in source")

        for module in modules:
            module_name = module.get('module_name_en', 'Unknown')
            uid = module['uid']

            if record_exists(target_conn, 'modules', uid):
                print(f"   ⊙ {module_name} (already exists)")
                stats['modules_skipped'] += 1
            else:
                insert_module(target_conn, 'modules', module)
                print(f"   ✓ {module_name} (added)")
                stats['modules_added'] += 1

        # 2. Copy Sub-Modules
        print("\n📋 Copying Sub-Modules...")
        sub_modules = get_all_sub_modules(source_conn)
        print(f"   Found {len(sub_modules)} sub-modules in source")

        for sub_module in sub_modules:
            sub_module_name = sub_module.get('sub_module_name_en', 'Unknown')
            uid = sub_module['uid']

            if record_exists(target_conn, 'sub_modules', uid):
                print(f"   ⊙ {sub_module_name} (already exists)")
                stats['sub_modules_skipped'] += 1
            else:
                insert_module(target_conn, 'sub_modules', sub_module)
                print(f"   ✓ {sub_module_name} (added)")
                stats['sub_modules_added'] += 1

        # 3. Copy Sub-Sub-Modules
        print("\n📋 Copying Sub-Sub-Modules...")
        sub_sub_modules = get_all_sub_sub_modules(source_conn)
        print(f"   Found {len(sub_sub_modules)} sub-sub-modules in source")

        for sub_sub_module in sub_sub_modules:
            sub_sub_module_name = sub_sub_module.get('sub_sub_module_name_en', 'Unknown')
            uid = sub_sub_module['uid']

            if record_exists(target_conn, 'sub_sub_modules', uid):
                print(f"   ⊙ {sub_sub_module_name} (already exists)")
                stats['sub_sub_modules_skipped'] += 1
            else:
                insert_module(target_conn, 'sub_sub_modules', sub_sub_module)
                print(f"   ✓ {sub_sub_module_name} (added)")
                stats['sub_sub_modules_added'] += 1

        # 4. Copy Normal Permissions
        print("\n📋 Copying Normal Permissions...")
        normal_permissions = get_permissions(source_conn, 'normal_permissions')
        print(f"   Found {len(normal_permissions)} permissions in source")

        for permission in normal_permissions:
            uid = permission['uid']
            if record_exists(target_conn, 'normal_permissions', uid):
                stats['normal_permissions_skipped'] += 1
            else:
                insert_module(target_conn, 'normal_permissions', permission)
                stats['normal_permissions_added'] += 1

        print(f"   ✓ Added {stats['normal_permissions_added']}, Skipped {stats['normal_permissions_skipped']}")

        # 5. Copy Principal Permissions
        print("\n📋 Copying Principal Permissions...")
        principal_permissions = get_permissions(source_conn, 'principal_permissions')
        print(f"   Found {len(principal_permissions)} permissions in source")

        for permission in principal_permissions:
            uid = permission['uid']
            if record_exists(target_conn, 'principal_permissions', uid):
                stats['principal_permissions_skipped'] += 1
            else:
                insert_module(target_conn, 'principal_permissions', permission)
                stats['principal_permissions_added'] += 1

        print(f"   ✓ Added {stats['principal_permissions_added']}, Skipped {stats['principal_permissions_skipped']}")

        # Commit changes
        target_conn.commit()

        # Display summary
        print("\n" + "=" * 70)
        print("📊 Summary")
        print("=" * 70)
        print(f"  Modules:")
        print(f"    • Added:   {stats['modules_added']}")
        print(f"    • Skipped: {stats['modules_skipped']}")
        print(f"  Sub-Modules:")
        print(f"    • Added:   {stats['sub_modules_added']}")
        print(f"    • Skipped: {stats['sub_modules_skipped']}")
        print(f"  Sub-Sub-Modules:")
        print(f"    • Added:   {stats['sub_sub_modules_added']}")
        print(f"    • Skipped: {stats['sub_sub_modules_skipped']}")
        print(f"  Normal Permissions:")
        print(f"    • Added:   {stats['normal_permissions_added']}")
        print(f"    • Skipped: {stats['normal_permissions_skipped']}")
        print(f"  Principal Permissions:")
        print(f"    • Added:   {stats['principal_permissions_added']}")
        print(f"    • Skipped: {stats['principal_permissions_skipped']}")
        print()

        # Show warehouse modules specifically
        print("🏢 Warehouse-Related Modules in Target Database:")
        print("-" * 70)
        cursor = target_conn.cursor(cursor_factory=RealDictCursor)

        # Check sub-modules for warehouse
        cursor.execute("""
            SELECT sub_module_name_en, uid, module_uid
            FROM sub_modules
            WHERE LOWER(sub_module_name_en) LIKE '%warehouse%'
            ORDER BY serial_no
        """)
        warehouse_sub_modules = cursor.fetchall()

        if warehouse_sub_modules:
            print("\nWarehouse Sub-Modules:")
            for row in warehouse_sub_modules:
                print(f"  • {row['sub_module_name_en']} (UID: {row['uid']})")

        # Check sub-sub-modules for warehouse
        cursor.execute("""
            SELECT sub_sub_module_name_en, uid, sub_module_uid
            FROM sub_sub_modules
            WHERE LOWER(sub_sub_module_name_en) LIKE '%warehouse%'
            OR sub_module_uid IN (
                SELECT uid FROM sub_modules WHERE LOWER(sub_module_name_en) LIKE '%warehouse%'
            )
            ORDER BY serial_no
        """)
        warehouse_sub_sub_modules = cursor.fetchall()

        if warehouse_sub_sub_modules:
            print("\nWarehouse Sub-Sub-Modules:")
            for row in warehouse_sub_sub_modules:
                print(f"  • {row['sub_sub_module_name_en']} (UID: {row['uid']})")

        if not warehouse_sub_modules and not warehouse_sub_sub_modules:
            print("  ⚠ No warehouse-related modules found!")

        cursor.close()
        print()
        print("✅ Module copy completed successfully!")

    except Exception as e:
        target_conn.rollback()
        print(f"\n✗ Error: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)

    finally:
        source_conn.close()
        target_conn.close()
        print("\n✓ Database connections closed")


if __name__ == "__main__":
    main()
