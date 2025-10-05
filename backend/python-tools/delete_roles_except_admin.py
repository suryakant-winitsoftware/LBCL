#!/usr/bin/env python3
"""
Delete all roles except Admin from the database
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

print("=" * 70)
print("Delete Roles Except Admin")
print("=" * 70)

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

try:
    # 1. First, check all roles in the database
    print("\n1. Current roles in database:")
    print("-" * 70)
    cursor.execute("""
        SELECT uid, role_name_en, code, is_active
        FROM roles
        WHERE (ss = 1 OR ss IS NULL)
        ORDER BY role_name_en
    """)
    all_roles = cursor.fetchall()

    print(f"Found {len(all_roles)} roles:")
    for role in all_roles:
        print(f"  • {role['role_name_en'] or role['uid']} (UID: {role['uid']}, Active: {role['is_active']})")

    # 2. Identify roles to delete (all except Admin)
    print("\n2. Roles to DELETE:")
    print("-" * 70)
    roles_to_delete = [r for r in all_roles if r['uid'] != 'Admin']

    if len(roles_to_delete) == 0:
        print("  No roles to delete (only Admin exists)")
        cursor.close()
        conn.close()
        exit(0)

    print(f"Will delete {len(roles_to_delete)} roles:")
    for role in roles_to_delete:
        print(f"  • {role['role_name_en'] or role['uid']} (UID: {role['uid']})")

    # 3. Confirm deletion
    print("\n" + "=" * 70)
    print("⚠️  WARNING: This will DELETE the roles listed above!")
    print("=" * 70)
    confirm = input("\nType 'DELETE' to confirm deletion: ")

    if confirm != 'DELETE':
        print("\n❌ Deletion cancelled")
        conn.close()
        exit(0)

    # 4. Delete roles (soft delete by setting ss = 0)
    print("\n3. Deleting roles...")
    print("-" * 70)

    deleted_count = 0
    for role in roles_to_delete:
        try:
            # Soft delete by setting ss = 0
            cursor.execute("""
                UPDATE roles
                SET ss = 0, modified_time = NOW()
                WHERE uid = %s AND (ss = 1 OR ss IS NULL)
            """, (role['uid'],))

            print(f"  ✓ Deleted: {role['role_name_en'] or role['uid']} (UID: {role['uid']})")
            deleted_count += 1
        except Exception as e:
            print(f"  ✗ Failed to delete {role['role_name_en'] or role['uid']}: {e}")

    # 5. Commit the changes
    conn.commit()

    print("\n" + "=" * 70)
    print(f"✅ Successfully deleted {deleted_count} roles")
    print("=" * 70)

    # 6. Verify remaining roles
    print("\n4. Remaining roles:")
    print("-" * 70)
    cursor.execute("""
        SELECT uid, role_name_en, code, is_active
        FROM roles
        WHERE (ss = 1 OR ss IS NULL)
        ORDER BY role_name_en
    """)
    remaining_roles = cursor.fetchall()

    print(f"Found {len(remaining_roles)} remaining roles:")
    for role in remaining_roles:
        print(f"  • {role['role_name_en'] or role['uid']} (UID: {role['uid']}, Active: {role['is_active']})")

except Exception as e:
    print(f"\n❌ Error: {e}")
    conn.rollback()
    import traceback
    traceback.print_exc()
finally:
    cursor.close()
    conn.close()

print("\n" + "=" * 70)
print("Done!")
print("=" * 70)
