#!/usr/bin/env python3
"""
Script to convert all 'id' columns to IDENTITY (auto-increment) in MSSQL database.
This script will:
1. Find all tables with non-IDENTITY id columns
2. For each table, check if it has data
3. If empty, convert id to IDENTITY starting from 1
4. If has data, preserve the current max ID and start IDENTITY from there
"""

import pymssql
import sys

# Database configuration
DB_CONFIG = {
    'server': '10.20.53.175',
    'user': 'lbssfadev',
    'password': 'lbssfadev',
    'database': 'LBSSFADev'
}

def get_tables_with_non_identity_id():
    """Get list of tables that have 'id' column but it's not IDENTITY."""
    conn = pymssql.connect(**DB_CONFIG)
    cursor = conn.cursor()

    query = """
    SELECT
        t.TABLE_NAME,
        c.DATA_TYPE
    FROM INFORMATION_SCHEMA.TABLES t
    INNER JOIN INFORMATION_SCHEMA.COLUMNS c
        ON t.TABLE_NAME = c.TABLE_NAME
        AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
    WHERE c.COLUMN_NAME = 'id'
        AND t.TABLE_TYPE = 'BASE TABLE'
        AND COLUMNPROPERTY(OBJECT_ID(t.TABLE_SCHEMA + '.' + t.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') = 0
    ORDER BY t.TABLE_NAME
    """

    cursor.execute(query)
    tables = cursor.fetchall()
    cursor.close()
    conn.close()

    return tables

def get_max_id(cursor, table_name):
    """Get the maximum ID value from a table."""
    try:
        cursor.execute(f"SELECT MAX(id) FROM [{table_name}]")
        result = cursor.fetchone()
        return result[0] if result[0] is not None else 0
    except Exception as e:
        print(f"  Warning: Could not get max ID for {table_name}: {e}")
        return 0

def get_primary_key_constraint(cursor, table_name):
    """Get the primary key constraint name for a table."""
    try:
        cursor.execute(f"""
            SELECT name
            FROM sys.key_constraints
            WHERE type = 'PK'
            AND parent_object_id = OBJECT_ID('{table_name}')
        """)
        result = cursor.fetchone()
        return result[0] if result else None
    except:
        return None

def convert_id_to_identity(table_name, data_type='int'):
    """Convert a table's id column to IDENTITY."""
    conn = pymssql.connect(**DB_CONFIG)
    cursor = conn.cursor()

    try:
        # Get max ID to determine starting point
        max_id = get_max_id(cursor, table_name)
        start_seed = max_id + 1 if max_id > 0 else 1

        print(f"  Processing {table_name}... (max_id={max_id}, will start from {start_seed})")

        # Check if there's a primary key constraint on the id column
        pk_name = get_primary_key_constraint(cursor, table_name)
        if pk_name:
            # Drop the primary key constraint first
            cursor.execute(f"ALTER TABLE [{table_name}] DROP CONSTRAINT [{pk_name}]")

        # Drop the id column
        cursor.execute(f"ALTER TABLE [{table_name}] DROP COLUMN id")

        # Add it back as IDENTITY with PRIMARY KEY
        cursor.execute(f"ALTER TABLE [{table_name}] ADD id {data_type} IDENTITY({start_seed},1) NOT NULL PRIMARY KEY")

        conn.commit()
        print(f"  ✓ Successfully converted {table_name}")
        return True

    except Exception as e:
        print(f"  ✗ Error converting {table_name}: {e}")
        conn.rollback()
        return False
    finally:
        cursor.close()
        conn.close()

def main():
    print("=" * 80)
    print("Converting all 'id' columns to IDENTITY (auto-increment)")
    print("=" * 80)

    # Get all tables that need conversion
    tables = get_tables_with_non_identity_id()
    print(f"\nFound {len(tables)} tables with non-IDENTITY id column\n")

    if not tables:
        print("No tables to convert!")
        return

    # Ask for confirmation
    response = input(f"Do you want to convert all {len(tables)} tables? (yes/no): ")
    if response.lower() not in ['yes', 'y']:
        print("Operation cancelled.")
        return

    # Convert each table
    success_count = 0
    failed_count = 0

    for i, (table_name, data_type) in enumerate(tables, 1):
        print(f"\n[{i}/{len(tables)}]", end=" ")
        if convert_id_to_identity(table_name, data_type):
            success_count += 1
        else:
            failed_count += 1

    # Summary
    print("\n" + "=" * 80)
    print(f"Conversion Complete!")
    print(f"  Successfully converted: {success_count}")
    print(f"  Failed: {failed_count}")
    print("=" * 80)

if __name__ == "__main__":
    main()
