#!/usr/bin/env python3
"""
Script to copy location_type data from PostgreSQL to MSSQL.
"""

import psycopg2
import pymssql
from datetime import datetime

# PostgreSQL configuration
PG_CONFIG = {
    'host': '10.20.53.130',
    'port': 5432,
    'database': 'multiplexdev170725FM',
    'user': 'multiplex',
    'password': 'multiplex'
}

# MSSQL configuration
MSSQL_CONFIG = {
    'server': '10.20.53.175',
    'user': 'lbssfadev',
    'password': 'lbssfadev',
    'database': 'LBSSFADev'
}

def fetch_location_types_from_pg():
    """Fetch all location types from PostgreSQL."""
    conn = psycopg2.connect(**PG_CONFIG)
    cursor = conn.cursor()

    query = """
    SELECT
        id, uid, created_by, created_time, modified_by, modified_time,
        server_add_time, server_modified_time, company_uid, name, parent_uid,
        level_no, code, show_in_ui, show_in_template
    FROM location_type
    ORDER BY id
    """

    cursor.execute(query)
    rows = cursor.fetchall()

    # Get column names
    columns = [desc[0] for desc in cursor.description]

    # Convert to list of dictionaries
    location_types = []
    for row in rows:
        location_type = dict(zip(columns, row))
        location_types.append(location_type)

    cursor.close()
    conn.close()

    return location_types

def check_mssql_table_structure():
    """Check if location_type table exists in MSSQL and show its structure."""
    conn = pymssql.connect(**MSSQL_CONFIG)
    cursor = conn.cursor()

    # Check if table exists
    cursor.execute("""
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_NAME = 'location_type'
    """)

    table_exists = cursor.fetchone()[0] > 0

    if table_exists:
        # Get existing record count
        cursor.execute("SELECT COUNT(*) FROM location_type")
        count = cursor.fetchone()[0]

        # Get column structure
        cursor.execute("""
            SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'location_type'
            ORDER BY ORDINAL_POSITION
        """)

        columns = cursor.fetchall()

        cursor.close()
        conn.close()

        return {
            'exists': True,
            'count': count,
            'columns': columns
        }
    else:
        cursor.close()
        conn.close()
        return {'exists': False}

def truncate_mssql_location_type():
    """Truncate location_type table in MSSQL."""
    conn = pymssql.connect(**MSSQL_CONFIG)
    cursor = conn.cursor()

    try:
        cursor.execute("DELETE FROM location_type")
        conn.commit()
        print("  Truncated location_type table in MSSQL")
    except Exception as e:
        print(f"  Error truncating table: {e}")
        conn.rollback()
    finally:
        cursor.close()
        conn.close()

def insert_location_types_to_mssql(location_types):
    """Insert location types into MSSQL."""
    conn = pymssql.connect(**MSSQL_CONFIG)
    cursor = conn.cursor()

    inserted = 0
    failed = 0

    for lt in location_types:
        try:
            # Check if IDENTITY_INSERT needs to be enabled
            cursor.execute("SET IDENTITY_INSERT location_type ON")

            sql = """
            INSERT INTO location_type
            (id, uid, created_by, created_time, modified_by, modified_time,
             server_add_time, server_modified_time, company_uid, name, parent_uid,
             level_no, code, show_in_ui, show_in_template)
            VALUES
            (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
            """

            cursor.execute(sql, (
                lt['id'],
                lt['uid'],
                lt['created_by'],
                lt['created_time'],
                lt['modified_by'],
                lt['modified_time'],
                lt['server_add_time'],
                lt['server_modified_time'],
                lt['company_uid'],
                lt['name'],
                lt['parent_uid'],
                lt['level_no'],
                lt['code'],
                lt.get('show_in_ui', True),
                lt.get('show_in_template', True)
            ))

            cursor.execute("SET IDENTITY_INSERT location_type OFF")
            conn.commit()
            inserted += 1
            print(f"  ✓ Inserted: {lt['name']} (UID: {lt['uid']}, Level: {lt['level_no']})")

        except Exception as e:
            print(f"  ✗ Failed to insert {lt.get('name', 'unknown')}: {e}")
            conn.rollback()
            failed += 1

    cursor.close()
    conn.close()

    return inserted, failed

def main():
    print("=" * 80)
    print("Copying location_type data from PostgreSQL to MSSQL")
    print("=" * 80)

    # Check MSSQL table structure
    print("\n1. Checking MSSQL table structure...")
    mssql_info = check_mssql_table_structure()

    if not mssql_info['exists']:
        print("  ✗ Table 'location_type' does not exist in MSSQL!")
        return

    print(f"  ✓ Table exists with {mssql_info['count']} existing records")
    print(f"  Columns: {len(mssql_info['columns'])}")

    # Fetch data from PostgreSQL
    print("\n2. Fetching location types from PostgreSQL...")
    location_types = fetch_location_types_from_pg()
    print(f"  ✓ Fetched {len(location_types)} location types")

    if not location_types:
        print("  No data to copy!")
        return

    # Show preview
    print("\n3. Preview of data to copy:")
    for lt in location_types[:5]:
        print(f"  - {lt['name']} (UID: {lt['uid']}, Level: {lt['level_no']}, Parent: {lt.get('parent_uid', 'None')})")
    if len(location_types) > 5:
        print(f"  ... and {len(location_types) - 5} more")

    # Ask for confirmation
    print(f"\n4. Current MSSQL has {mssql_info['count']} records.")
    if mssql_info['count'] > 0:
        response = input("   Do you want to DELETE existing records and copy fresh data? (yes/no): ")
    else:
        response = input(f"   Do you want to copy {len(location_types)} location types? (yes/no): ")

    if response.lower() not in ['yes', 'y']:
        print("Operation cancelled.")
        return

    # Truncate if has existing data
    if mssql_info['count'] > 0:
        print("\n5. Truncating existing data...")
        truncate_mssql_location_type()

    # Insert data
    print(f"\n6. Inserting {len(location_types)} location types into MSSQL...")
    inserted, failed = insert_location_types_to_mssql(location_types)

    # Summary
    print("\n" + "=" * 80)
    print("Copy Complete!")
    print(f"  Successfully inserted: {inserted}")
    print(f"  Failed: {failed}")
    print("=" * 80)

if __name__ == "__main__":
    main()
