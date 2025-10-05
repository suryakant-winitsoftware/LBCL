#!/usr/bin/env python3
"""
Check Permission Matrix configuration for warehouse modules
"""

import psycopg2
from psycopg2.extras import RealDictCursor

DB_PARAMS = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev15072025',
    'user': 'multiplex',
    'password': 'multiplex'
}


def main():
    conn = psycopg2.connect(**DB_PARAMS)
    cursor = conn.cursor(cursor_factory=RealDictCursor)

    print("="*70)
    print("Permission Matrix Configuration Check")
    print("="*70)

    # Check warehouse sub-modules
    print("\n📦 Warehouse Sub-Modules Configuration:")
    cursor.execute("""
        SELECT
            submodule_name_en,
            uid,
            show_in_menu,
            is_for_distributor,
            is_for_principal,
            module_uid
        FROM sub_modules
        WHERE LOWER(submodule_name_en) LIKE '%warehouse%'
        ORDER BY serial_no
    """)

    warehouse_subs = cursor.fetchall()
    for row in warehouse_subs:
        print(f"\n  • {row['submodule_name_en']}")
        print(f"    UID: {row['uid']}")
        print(f"    show_in_menu: {row['show_in_menu']}")
        print(f"    is_for_distributor: {row['is_for_distributor']}")
        print(f"    is_for_principal: {row['is_for_principal']}")
        print(f"    module_uid: {row['module_uid']}")

    # Check warehouse sub-sub-modules
    print("\n📋 Warehouse Sub-Sub-Modules Configuration:")
    cursor.execute("""
        SELECT
            sub_sub_module_name_en,
            uid,
            show_in_menu,
            is_for_distributor,
            is_for_principal,
            sub_module_uid
        FROM sub_sub_modules
        WHERE sub_module_uid = 'SystemAdministration_WarehouseManagement'
        ORDER BY serial_no
    """)

    warehouse_subsubs = cursor.fetchall()
    if warehouse_subsubs:
        for row in warehouse_subsubs:
            print(f"\n  • {row['sub_sub_module_name_en']}")
            print(f"    UID: {row['uid']}")
            print(f"    show_in_menu: {row['show_in_menu']}")
            print(f"    is_for_distributor: {row['is_for_distributor']}")
            print(f"    is_for_principal: {row['is_for_principal']}")
    else:
        print("  ⚠ No sub-sub-modules found for Warehouse Management!")

    # Check permissions for warehouse modules
    print("\n🔐 Permissions for Warehouse Management:")
    cursor.execute("""
        SELECT p.*, sm.submodule_name_en
        FROM normal_permissions p
        JOIN sub_modules sm ON p.sub_module_uid = sm.uid
        WHERE sm.uid = 'SystemAdministration_WarehouseManagement'
        ORDER BY p.serial_no
    """)

    permissions = cursor.fetchall()
    if permissions:
        print(f"  Found {len(permissions)} permissions")
        for row in permissions:
            print(f"    • {row.get('permission_name_en', 'N/A')}")
    else:
        print("  ⚠ No permissions found!")

    # Compare with another working module (e.g., Distributor Management)
    print("\n\n📊 Comparison with Distributor Management (Working Module):")
    cursor.execute("""
        SELECT
            submodule_name_en,
            uid,
            show_in_menu,
            is_for_distributor,
            is_for_principal,
            module_uid
        FROM sub_modules
        WHERE LOWER(submodule_name_en) LIKE '%distributor%'
        ORDER BY serial_no
        LIMIT 1
    """)

    working_module = cursor.fetchone()
    if working_module:
        print(f"\n  • {working_module['submodule_name_en']}")
        print(f"    UID: {working_module['uid']}")
        print(f"    show_in_menu: {working_module['show_in_menu']}")
        print(f"    is_for_distributor: {working_module['is_for_distributor']}")
        print(f"    is_for_principal: {working_module['is_for_principal']}")

        # Check its sub-sub-modules
        cursor.execute("""
            SELECT
                sub_sub_module_name_en,
                uid,
                show_in_menu,
                is_for_distributor,
                is_for_principal
            FROM sub_sub_modules
            WHERE sub_module_uid = %s
            ORDER BY serial_no
            LIMIT 3
        """, (working_module['uid'],))

        print(f"\n  Sub-Sub-Modules of {working_module['submodule_name_en']}:")
        for row in cursor.fetchall():
            print(f"    • {row['sub_sub_module_name_en']}")
            print(f"      show_in_menu: {row['show_in_menu']}")
            print(f"      is_for_distributor: {row['is_for_distributor']}")

    # Check Administration module
    print("\n\n🏢 Administration Module (Parent):")
    cursor.execute("""
        SELECT
            module_name_en,
            uid,
            show_in_menu
        FROM modules
        WHERE LOWER(module_name_en) = 'administration'
    """)

    admin_module = cursor.fetchone()
    if admin_module:
        print(f"  • {admin_module['module_name_en']}")
        print(f"    UID: {admin_module['uid']}")
        print(f"    show_in_menu: {admin_module['show_in_menu']}")

    cursor.close()
    conn.close()

    print("\n" + "="*70)


if __name__ == "__main__":
    main()
