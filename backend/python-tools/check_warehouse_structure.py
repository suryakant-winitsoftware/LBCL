#!/usr/bin/env python3
"""
Check the complete structure of warehouse modules
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


def check_db_structure(db_params, db_name):
    conn = psycopg2.connect(**db_params)
    cursor = conn.cursor(cursor_factory=RealDictCursor)

    print(f"\n{'='*70}")
    print(f"{db_name}")
    print(f"{'='*70}")

    # Find Administration module
    cursor.execute("""
        SELECT uid, module_name_en
        FROM modules
        WHERE LOWER(module_name_en) = 'administration'
    """)
    admin = cursor.fetchone()

    if admin:
        print(f"\n✓ Administration Module: {admin['uid']}")

        # Find all sub-modules under Administration
        cursor.execute("""
            SELECT uid, submodule_name_en, serial_no, show_in_menu
            FROM sub_modules
            WHERE module_uid = %s
            ORDER BY serial_no
        """, (admin['uid'],))

        sub_modules = cursor.fetchall()
        print(f"\n📁 Sub-Modules under Administration ({len(sub_modules)} total):")

        for sub in sub_modules:
            print(f"\n  {sub['serial_no']}. {sub['submodule_name_en']}")
            print(f"     UID: {sub['uid']}")
            print(f"     show_in_menu: {sub['show_in_menu']}")

            # Check if this is warehouse management
            if 'warehouse' in sub['submodule_name_en'].lower():
                # Get sub-sub-modules
                cursor.execute("""
                    SELECT uid, sub_sub_module_name_en, serial_no, show_in_menu
                    FROM sub_sub_modules
                    WHERE sub_module_uid = %s
                    ORDER BY serial_no
                """, (sub['uid'],))

                sub_subs = cursor.fetchall()
                if sub_subs:
                    print(f"     📄 Sub-Sub-Modules ({len(sub_subs)}):")
                    for ss in sub_subs:
                        print(f"        • {ss['sub_sub_module_name_en']} (show: {ss['show_in_menu']})")
                else:
                    print(f"     ⚠ NO SUB-SUB-MODULES!")

    cursor.close()
    conn.close()


def main():
    print("="*70)
    print("Warehouse Module Structure Analysis")
    print("="*70)

    # Check source database
    check_db_structure(SOURCE_DB, "SOURCE: multiplexdev170725FM")

    # Check target database
    check_db_structure(TARGET_DB, "TARGET: multiplexdev15072025")


if __name__ == "__main__":
    main()
