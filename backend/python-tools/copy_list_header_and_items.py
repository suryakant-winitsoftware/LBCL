#!/usr/bin/env python3
"""
Copy list_header and list_item from multiplexdev170725FM to multiplexdev15072025
Update org_uid to LBCL (Principal org)
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

TARGET_ORG_UID = 'LBCL'  # Principal organization in target database

print("=" * 70)
print("Copying list_header and list_item Tables")
print("=" * 70)

conn_src = psycopg2.connect(**SOURCE_DB)
cursor_src = conn_src.cursor(cursor_factory=RealDictCursor)

conn_tgt = psycopg2.connect(**TARGET_DB)
cursor_tgt = conn_tgt.cursor(cursor_factory=RealDictCursor)

try:
    # ========== STEP 1: Copy list_header ==========
    print("\n1. Copying list_header:")
    print("-" * 70)

    # Get all list headers from source
    cursor_src.execute("""
        SELECT * FROM list_header
        ORDER BY created_time
    """)
    headers = cursor_src.fetchall()

    print(f"Found {len(headers)} list headers in source database")

    # Check what exists in target
    cursor_tgt.execute("SELECT uid FROM list_header")
    existing_headers = {row['uid'] for row in cursor_tgt.fetchall()}
    print(f"Found {len(existing_headers)} existing headers in target database")

    inserted_headers = 0
    updated_headers = 0

    for header in headers:
        # Update org_uid to LBCL
        org_uid = TARGET_ORG_UID if header.get('org_uid') else None

        if header['uid'] in existing_headers:
            # Update existing
            cursor_tgt.execute("""
                UPDATE list_header
                SET modified_by = %s,
                    modified_time = NOW(),
                    org_uid = %s,
                    code = %s,
                    name = %s,
                    is_editable = %s,
                    is_visible_in_ui = %s
                WHERE uid = %s
            """, (
                header['modified_by'],
                org_uid,
                header['code'],
                header['name'],
                header['is_editable'],
                header['is_visible_in_ui'],
                header['uid']
            ))
            updated_headers += 1
            print(f"  ↻ Updated: {header['code']} - {header['name']}")
        else:
            # Insert new
            cursor_tgt.execute("""
                INSERT INTO list_header (
                    uid, created_by, created_time, modified_by, modified_time,
                    server_add_time, server_modified_time,
                    company_uid, org_uid, code, name,
                    is_editable, is_visible_in_ui
                ) VALUES (
                    %s, %s, %s, %s, %s,
                    NOW(), NOW(),
                    %s, %s, %s, %s,
                    %s, %s
                )
            """, (
                header['uid'],
                'ADMIN',  # Use ADMIN instead of original created_by
                header['created_time'],
                'ADMIN',  # Use ADMIN instead of original modified_by
                header['modified_time'],
                header.get('company_uid'),
                org_uid,
                header['code'],
                header['name'],
                header['is_editable'],
                header['is_visible_in_ui']
            ))
            inserted_headers += 1
            print(f"  + Inserted: {header['code']} - {header['name']}")

    conn_tgt.commit()

    print(f"\n✅ list_header: Inserted {inserted_headers}, Updated {updated_headers}")

    # ========== STEP 2: Copy list_item ==========
    print("\n2. Copying list_item:")
    print("-" * 70)

    # Get all list items from source
    cursor_src.execute("""
        SELECT * FROM list_item
        ORDER BY list_header_uid, serial_no
    """)
    items = cursor_src.fetchall()

    print(f"Found {len(items)} list items in source database")

    # Check what exists in target
    cursor_tgt.execute("SELECT uid FROM list_item")
    existing_items = {row['uid'] for row in cursor_tgt.fetchall()}
    print(f"Found {len(existing_items)} existing items in target database")

    inserted_items = 0
    updated_items = 0
    skipped_items = 0

    for item in items:
        # Check if the list_header_uid exists in target
        cursor_tgt.execute("""
            SELECT uid FROM list_header WHERE uid = %s
        """, (item['list_header_uid'],))

        if not cursor_tgt.fetchone():
            print(f"  ⚠ Skipped: {item['code']} (header {item['list_header_uid']} not found)")
            skipped_items += 1
            continue

        if item['uid'] in existing_items:
            # Update existing
            cursor_tgt.execute("""
                UPDATE list_item
                SET modified_by = %s,
                    modified_time = NOW(),
                    code = %s,
                    name = %s,
                    is_editable = %s,
                    serial_no = %s,
                    list_header_uid = %s
                WHERE uid = %s
            """, (
                item['modified_by'],
                item['code'],
                item['name'],
                item['is_editable'],
                item['serial_no'],
                item['list_header_uid'],
                item['uid']
            ))
            updated_items += 1
            if updated_items <= 5:  # Only print first 5
                print(f"  ↻ Updated: {item['code']} - {item['name']}")
        else:
            # Insert new
            cursor_tgt.execute("""
                INSERT INTO list_item (
                    uid, created_by, created_time, modified_by, modified_time,
                    server_add_time, server_modified_time,
                    code, name, is_editable, serial_no, list_header_uid
                ) VALUES (
                    %s, %s, %s, %s, %s,
                    NOW(), NOW(),
                    %s, %s, %s, %s, %s
                )
            """, (
                item['uid'],
                'ADMIN',  # Use ADMIN instead of original created_by
                item['created_time'],
                'ADMIN',  # Use ADMIN instead of original modified_by
                item['modified_time'],
                item['code'],
                item['name'],
                item['is_editable'],
                item['serial_no'],
                item['list_header_uid']
            ))
            inserted_items += 1
            if inserted_items <= 5:  # Only print first 5
                print(f"  + Inserted: {item['code']} - {item['name']}")

    conn_tgt.commit()

    print(f"\n✅ list_item: Inserted {inserted_items}, Updated {updated_items}, Skipped {skipped_items}")

    # ========== STEP 3: Verify ==========
    print("\n3. Verification:")
    print("-" * 70)

    cursor_tgt.execute("""
        SELECT lh.code, lh.name, lh.org_uid, COUNT(li.uid) as item_count
        FROM list_header lh
        LEFT JOIN list_item li ON lh.uid = li.list_header_uid
        GROUP BY lh.uid, lh.code, lh.name, lh.org_uid
        ORDER BY lh.code
    """)

    verification = cursor_tgt.fetchall()
    print(f"\nTarget database now has {len(verification)} list headers:")
    for i, row in enumerate(verification[:15], 1):  # Show first 15
        print(f"  {i:2}. {row['code']:30} - {row['item_count']:2} items (Org: {row['org_uid'] or 'NULL'})")

    if len(verification) > 15:
        print(f"  ... and {len(verification) - 15} more")

    print("\n" + "=" * 70)
    print("✅ COPY COMPLETE!")
    print("=" * 70)
    print(f"Summary:")
    print(f"  Headers: +{inserted_headers} inserted, ~{updated_headers} updated")
    print(f"  Items:   +{inserted_items} inserted, ~{updated_items} updated, !{skipped_items} skipped")
    print(f"  All headers set to org: {TARGET_ORG_UID}")
    print("=" * 70)

except Exception as e:
    print(f"\n❌ Error: {e}")
    conn_tgt.rollback()
    import traceback
    traceback.print_exc()
finally:
    cursor_src.close()
    cursor_tgt.close()
    conn_src.close()
    conn_tgt.close()

print("\nDone!")
