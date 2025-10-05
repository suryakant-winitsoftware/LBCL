#!/usr/bin/env python3
"""
Check detailed partition information for wh_stock_request table
"""

import psycopg2

DB_PARAMS = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev15072025',
    'user': 'multiplex',
    'password': 'multiplex'
}

def main():
    conn = psycopg2.connect(**DB_PARAMS)
    cursor = conn.cursor()

    # Check Level 1 partitions (by org_uid)
    print("=" * 80)
    print("LEVEL 1 PARTITIONS (by org_uid)")
    print("=" * 80)
    cursor.execute("""
        SELECT
            nmsp_parent.nspname AS parent_schema,
            parent.relname AS parent_table,
            nmsp_child.nspname AS child_schema,
            child.relname AS child_table,
            pg_get_expr(child.relpartbound, child.oid) AS partition_expression
        FROM pg_inherits
        JOIN pg_class parent ON pg_inherits.inhparent = parent.oid
        JOIN pg_class child ON pg_inherits.inhrelid = child.oid
        JOIN pg_namespace nmsp_parent ON nmsp_parent.oid = parent.relnamespace
        JOIN pg_namespace nmsp_child ON nmsp_child.oid = child.relnamespace
        WHERE parent.relname = 'wh_stock_request'
        ORDER BY child.relname;
    """)

    level1_partitions = cursor.fetchall()
    for row in level1_partitions:
        print(f"  {row[3]}: {row[4]}")

    # Check Level 2 partitions for each Level 1
    print("\n" + "=" * 80)
    print("LEVEL 2 PARTITIONS (by warehouse_uid)")
    print("=" * 80)

    for partition in level1_partitions:
        parent_name = partition[3]
        print(f"\n  Parent: {parent_name}")

        cursor.execute("""
            SELECT
                child.relname AS child_table,
                pg_get_expr(child.relpartbound, child.oid) AS partition_expression
            FROM pg_inherits
            JOIN pg_class parent ON pg_inherits.inhparent = parent.oid
            JOIN pg_class child ON pg_inherits.inhrelid = child.oid
            WHERE parent.relname = %s
            ORDER BY child.relname;
        """, (parent_name,))

        level2_partitions = cursor.fetchall()
        for row in level2_partitions:
            print(f"    {row[0]}: {row[1]}")

    # Check actual org_uid values in the table
    print("\n" + "=" * 80)
    print("DISTINCT org_uid VALUES IN TABLE")
    print("=" * 80)
    cursor.execute("""
        SELECT DISTINCT org_uid, COUNT(*)
        FROM wh_stock_request
        GROUP BY org_uid
        ORDER BY org_uid;
    """)

    org_values = cursor.fetchall()
    for row in org_values:
        print(f"  '{row[0]}': {row[1]} records")

    # Check actual warehouse_uid values
    print("\n" + "=" * 80)
    print("DISTINCT warehouse_uid VALUES IN TABLE")
    print("=" * 80)
    cursor.execute("""
        SELECT DISTINCT org_uid, warehouse_uid, COUNT(*)
        FROM wh_stock_request
        GROUP BY org_uid, warehouse_uid
        ORDER BY org_uid, warehouse_uid;
    """)

    wh_values = cursor.fetchall()
    for row in wh_values:
        print(f"  org='{row[0]}', warehouse='{row[1]}': {row[2]} records")

    # Check year_month values
    print("\n" + "=" * 80)
    print("DISTINCT year_month VALUES IN TABLE")
    print("=" * 80)
    cursor.execute("""
        SELECT DISTINCT year_month, COUNT(*)
        FROM wh_stock_request
        GROUP BY year_month
        ORDER BY year_month;
    """)

    ym_values = cursor.fetchall()
    for row in ym_values:
        print(f"  {row[0]}: {row[1]} records")

    cursor.close()
    conn.close()

if __name__ == "__main__":
    main()
