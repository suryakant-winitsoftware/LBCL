#!/usr/bin/env python3
"""
Add Warehouse sub-sub-modules to match the structure
Similar to Distributor Management structure
"""

import psycopg2
from psycopg2.extras import RealDictCursor
import uuid

TARGET_DB = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev15072025',
    'user': 'multiplex',
    'password': 'multiplex'
}

# Warehouse sub-sub-modules to create
WAREHOUSE_SUBSUBMODULES = [
    {
        'name_en': 'Warehouses',
        'name_other': 'Warehouses',
        'relative_path': '/administration/warehouse-management/warehouses',
        'serial_no': 166,
        'show_in_menu': True,
        'is_for_distributor': True,
        'is_for_principal': True,
    },
    {
        'name_en': 'Stock Requests',
        'name_other': 'Stock Requests',
        'relative_path': '/administration/warehouse-management/stock-requests',
        'serial_no': 167,
        'show_in_menu': True,
        'is_for_distributor': True,
        'is_for_principal': True,
    },
    {
        'name_en': 'Stock Transfers',
        'name_other': 'Stock Transfers',
        'relative_path': '/administration/warehouse-management/stock-transfers',
        'serial_no': 168,
        'show_in_menu': True,
        'is_for_distributor': True,
        'is_for_principal': True,
    },
    {
        'name_en': 'Stock Adjustments',
        'name_other': 'Stock Adjustments',
        'relative_path': '/administration/warehouse-management/stock-adjustments',
        'serial_no': 169,
        'show_in_menu': True,
        'is_for_distributor': True,
        'is_for_principal': True,
    },
    {
        'name_en': 'Stock Summary',
        'name_other': 'Stock Summary',
        'relative_path': '/administration/warehouse-management/stock-summary',
        'serial_no': 170,
        'show_in_menu': True,
        'is_for_distributor': True,
        'is_for_principal': True,
    },
]

# Standard CRUD permissions for each sub-sub-module
STANDARD_PERMISSIONS = ['View', 'Create', 'Update', 'Delete', 'Export']


def main():
    conn = psycopg2.connect(**TARGET_DB)
    cursor = conn.cursor(cursor_factory=RealDictCursor)

    print("="*70)
    print("Adding Warehouse Sub-Sub-Modules and Permissions")
    print("="*70)

    try:
        # Get Warehouse Management sub-module UID
        cursor.execute("""
            SELECT uid FROM sub_modules
            WHERE uid = 'SystemAdministration_WarehouseManagement'
        """)
        warehouse_sub = cursor.fetchone()

        if not warehouse_sub:
            print("✗ Warehouse Management sub-module not found!")
            return

        warehouse_uid = warehouse_sub['uid']
        print(f"\n✓ Found Warehouse Management: {warehouse_uid}")

        # Get the highest current serial number for sub-sub-modules
        cursor.execute("SELECT MAX(serial_no) as max_serial FROM sub_sub_modules")
        max_serial = cursor.fetchone()['max_serial'] or 165

        print(f"\n📋 Creating Sub-Sub-Modules...")
        modules_created = 0

        for i, module_data in enumerate(WAREHOUSE_SUBSUBMODULES, 1):
            # Generate UID
            module_uid = f"SystemAdministration_WarehouseManagement_{module_data['name_en'].replace(' ', '')}"

            # Check if already exists
            cursor.execute("""
                SELECT uid FROM sub_sub_modules WHERE uid = %s
            """, (module_uid,))

            if cursor.fetchone():
                print(f"\n  ⊙ {module_data['name_en']} (already exists)")
                continue

            # Use the specified serial number or next available
            serial_no = module_data['serial_no']

            # Insert sub-sub-module
            cursor.execute("""
                INSERT INTO sub_sub_modules (
                    uid, sub_sub_module_name_en, sub_sub_module_name_other,
                    relative_path, serial_no, sub_module_uid,
                    show_in_menu, is_for_distributor, is_for_principal,
                    ss, created_by, modified_by, created_time, modified_time
                ) VALUES (
                    %s, %s, %s, %s, %s, %s, %s, %s, %s,
                    1, 'SYSTEM', 'SYSTEM', NOW(), NOW()
                )
            """, (
                module_uid,
                module_data['name_en'],
                module_data['name_other'],
                module_data['relative_path'],
                serial_no,
                warehouse_uid,
                module_data['show_in_menu'],
                module_data['is_for_distributor'],
                module_data['is_for_principal']
            ))

            print(f"\n  ✓ {module_data['name_en']}")
            print(f"    UID: {module_uid}")
            print(f"    Serial: {serial_no}")
            modules_created += 1

        # Commit changes
        conn.commit()

        # Display summary
        print("\n" + "="*70)
        print("📊 Summary")
        print("="*70)
        print(f"  Sub-Sub-Modules Created: {modules_created}")

        # Verify the structure
        print("\n🔍 Verification - Warehouse Management Structure:")
        print("-"*70)

        cursor.execute("""
            SELECT
                sub_sub_module_name_en,
                uid,
                serial_no,
                show_in_menu,
                relative_path
            FROM sub_sub_modules
            WHERE sub_module_uid = %s
            ORDER BY serial_no
        """, (warehouse_uid,))

        warehouse_modules = cursor.fetchall()
        if warehouse_modules:
            for row in warehouse_modules:
                print(f"\n  • {row['sub_sub_module_name_en']}")
                print(f"    Path: {row['relative_path']}")
                print(f"    Show in menu: {row['show_in_menu']}")
        else:
            print("  ⚠ No sub-sub-modules found!")

        print("\n✅ Warehouse sub-sub-modules added successfully!")
        print("\nYou should now see these modules in the permission matrix.")

    except Exception as e:
        conn.rollback()
        print(f"\n✗ Error: {e}")
        import traceback
        traceback.print_exc()

    finally:
        cursor.close()
        conn.close()


if __name__ == "__main__":
    main()
