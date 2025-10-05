#!/usr/bin/env python3
"""
Verify that warehouse modules will appear in permission matrix
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
    print("Permission Matrix Visibility Verification")
    print("="*70)

    # Get complete hierarchy for Administration -> Warehouse Management
    print("\n📁 Complete Module Hierarchy:")
    print("-"*70)

    cursor.execute("""
        SELECT
            m.module_name_en as module,
            sm.submodule_name_en as sub_module,
            sm.show_in_menu as sm_show,
            ssm.sub_sub_module_name_en as sub_sub_module,
            ssm.show_in_menu as ssm_show,
            ssm.is_for_distributor,
            ssm.is_for_principal,
            ssm.uid as ssm_uid
        FROM modules m
        JOIN sub_modules sm ON sm.module_uid = m.uid
        JOIN sub_sub_modules ssm ON ssm.sub_module_uid = sm.uid
        WHERE m.module_name_en = 'Administration'
        AND sm.submodule_name_en = 'Warehouse Management'
        ORDER BY ssm.serial_no
    """)

    warehouse_hierarchy = cursor.fetchall()

    if warehouse_hierarchy:
        print(f"\n✓ Found {len(warehouse_hierarchy)} warehouse sub-sub-modules:")
        for row in warehouse_hierarchy:
            print(f"\n  {row['module']} > {row['sub_module']} > {row['sub_sub_module']}")
            print(f"    UID: {row['ssm_uid']}")
            print(f"    Show in Menu: {row['ssm_show']}")
            print(f"    For Distributor: {row['is_for_distributor']}")
            print(f"    For Principal: {row['is_for_principal']}")
    else:
        print("\n✗ No warehouse sub-sub-modules found in hierarchy!")
        return

    # Check if similar structure exists for comparison
    print("\n\n📊 Comparison with Distributor Management:")
    print("-"*70)

    cursor.execute("""
        SELECT
            sm.submodule_name_en as sub_module,
            COUNT(ssm.uid) as sub_sub_count
        FROM modules m
        JOIN sub_modules sm ON sm.module_uid = m.uid
        LEFT JOIN sub_sub_modules ssm ON ssm.sub_module_uid = sm.uid
        WHERE m.module_name_en = 'Administration'
        AND sm.submodule_name_en IN ('Warehouse Management', 'Distributor Management')
        GROUP BY sm.submodule_name_en
        ORDER BY sm.submodule_name_en
    """)

    comparison = cursor.fetchall()
    for row in comparison:
        print(f"  {row['sub_module']}: {row['sub_sub_count']} sub-sub-modules")

    # Show how it will appear in permission matrix API response
    print("\n\n🔍 Expected Permission Matrix Structure:")
    print("-"*70)
    print("In /administration/access-control/permission-matrix, you should see:")
    print("\n  └─ Administration")
    print("     └─ Warehouse Management")

    for row in warehouse_hierarchy:
        print(f"        ├─ {row['sub_sub_module']}")
        print(f"        │  └─ Permissions: View, Create, Update, Delete, Export, etc.")

    # Check if there are any existing role permissions
    print("\n\n🔐 Existing Role Permissions:")
    print("-"*70)

    cursor.execute("""
        SELECT DISTINCT
            r.role_name_en,
            COUNT(DISTINCT np.sub_sub_module_uid) as modules_with_permissions
        FROM roles r
        LEFT JOIN normal_permissions np ON np.role_uid = r.uid
        WHERE np.sub_sub_module_uid LIKE 'SystemAdministration_WarehouseManagement_%'
        GROUP BY r.role_name_en
    """)

    role_perms = cursor.fetchall()
    if role_perms:
        print("\nRoles with warehouse permissions already configured:")
        for row in role_perms:
            print(f"  • {row['role_name_en']}: {row['modules_with_permissions']} modules")
    else:
        print("\nℹ No roles have warehouse permissions configured yet.")
        print("  You will need to configure permissions for each role in the permission matrix.")

    cursor.close()
    conn.close()

    print("\n" + "="*70)
    print("✅ Verification Complete!")
    print("="*70)
    print("\nNext Steps:")
    print("1. Refresh the permission matrix page in your browser")
    print("2. Navigate to Administration > Access Control > Permission Matrix")
    print("3. Find 'Warehouse Management' under Administration")
    print("4. Configure permissions for each role as needed")
    print("="*70)


if __name__ == "__main__":
    main()
