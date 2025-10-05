#!/usr/bin/env python3
"""
Verify Warehouse Modules in both databases
Compare multiplexdev170725FM and multiplexdev15072025
"""

import psycopg2
from psycopg2.extras import RealDictCursor

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


def get_warehouse_modules(db_params, db_name):
    """Get all warehouse-related modules from a database."""
    conn = psycopg2.connect(**db_params)
    cursor = conn.cursor(cursor_factory=RealDictCursor)

    print(f"\n{'='*70}")
    print(f"Database: {db_name}")
    print(f"{'='*70}")

    # Get warehouse sub-modules
    cursor.execute("""
        SELECT submodule_name_en, uid, module_uid, serial_no
        FROM sub_modules
        WHERE LOWER(submodule_name_en) LIKE '%warehouse%'
        OR LOWER(submodule_name_en) LIKE '%stock%'
        OR LOWER(submodule_name_en) LIKE '%inventory%'
        ORDER BY serial_no
    """)
    sub_modules = cursor.fetchall()

    if sub_modules:
        print("\n📦 Warehouse/Stock/Inventory Sub-Modules:")
        for row in sub_modules:
            print(f"  • {row['submodule_name_en']}")
            print(f"    UID: {row['uid']}")
            print(f"    Serial No: {row['serial_no']}")
    else:
        print("\n⚠ No warehouse sub-modules found!")

    # Get warehouse sub-sub-modules
    cursor.execute("""
        SELECT sub_sub_module_name_en, uid, sub_module_uid, serial_no
        FROM sub_sub_modules
        WHERE LOWER(sub_sub_module_name_en) LIKE '%warehouse%'
        OR LOWER(sub_sub_module_name_en) LIKE '%stock%'
        OR sub_module_uid IN (
            SELECT uid FROM sub_modules
            WHERE LOWER(submodule_name_en) LIKE '%warehouse%'
            OR LOWER(submodule_name_en) LIKE '%stock%'
            OR LOWER(submodule_name_en) LIKE '%inventory%'
        )
        ORDER BY serial_no
    """)
    sub_sub_modules = cursor.fetchall()

    if sub_sub_modules:
        print(f"\n📋 Warehouse-Related Sub-Sub-Modules ({len(sub_sub_modules)} total):")
        for row in sub_sub_modules:
            print(f"  • {row['sub_sub_module_name_en']}")
            print(f"    UID: {row['uid']}")
            print(f"    Serial No: {row['serial_no']}")
    else:
        print("\n⚠ No warehouse sub-sub-modules found!")

    cursor.close()
    conn.close()

    return {
        'sub_modules': [dict(row) for row in sub_modules],
        'sub_sub_modules': [dict(row) for row in sub_sub_modules]
    }


def main():
    """Main function to verify warehouse modules."""

    print("="*70)
    print("Warehouse Module Verification")
    print("Comparing multiplexdev170725FM and multiplexdev15072025")
    print("="*70)

    # Get warehouse modules from source
    source_data = get_warehouse_modules(SOURCE_DB, 'multiplexdev170725FM (SOURCE)')

    # Get warehouse modules from target
    target_data = get_warehouse_modules(TARGET_DB, 'multiplexdev15072025 (TARGET)')

    # Compare
    print(f"\n{'='*70}")
    print("📊 Comparison Summary")
    print(f"{'='*70}")

    source_sub_uids = {m['uid'] for m in source_data['sub_modules']}
    target_sub_uids = {m['uid'] for m in target_data['sub_modules']}

    source_subsub_uids = {m['uid'] for m in source_data['sub_sub_modules']}
    target_subsub_uids = {m['uid'] for m in target_data['sub_sub_modules']}

    missing_subs = source_sub_uids - target_sub_uids
    missing_subsubs = source_subsub_uids - target_subsub_uids

    print(f"\nSub-Modules:")
    print(f"  Source: {len(source_data['sub_modules'])}")
    print(f"  Target: {len(target_data['sub_modules'])}")
    print(f"  Missing: {len(missing_subs)}")

    if missing_subs:
        print("\n  ⚠ Missing Sub-Modules in Target:")
        for uid in missing_subs:
            module = next((m for m in source_data['sub_modules'] if m['uid'] == uid), None)
            if module:
                print(f"    • {module['submodule_name_en']} ({uid})")

    print(f"\nSub-Sub-Modules:")
    print(f"  Source: {len(source_data['sub_sub_modules'])}")
    print(f"  Target: {len(target_data['sub_sub_modules'])}")
    print(f"  Missing: {len(missing_subsubs)}")

    if missing_subsubs:
        print("\n  ⚠ Missing Sub-Sub-Modules in Target:")
        for uid in missing_subsubs:
            module = next((m for m in source_data['sub_sub_modules'] if m['uid'] == uid), None)
            if module:
                print(f"    • {module['sub_sub_module_name_en']} ({uid})")

    if not missing_subs and not missing_subsubs:
        print("\n✅ All warehouse modules are present in target database!")
    else:
        print(f"\n⚠ {len(missing_subs) + len(missing_subsubs)} modules are missing in target database")


if __name__ == "__main__":
    main()
