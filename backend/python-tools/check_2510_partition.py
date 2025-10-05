#!/usr/bin/env python3
"""
Check if partition exists for year_month = 2510
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

    # Check Level 3 partitions for DIST1759603974795_WH001
    print("=" * 80)
    print("Level 3 Partitions for: wh_stock_request_DIST1759603974795_WH001")
    print("=" * 80)

    cursor.execute("""
        SELECT
            child.relname AS partition_name,
            pg_get_expr(child.relpartbound, child.oid) AS partition_expression
        FROM pg_inherits
        JOIN pg_class parent ON pg_inherits.inhparent = parent.oid
        JOIN pg_class child ON pg_inherits.inhrelid = child.oid
        WHERE parent.relname = 'wh_stock_request_DIST1759603974795_WH001'
        ORDER BY child.relname;
    """)

    partitions = cursor.fetchall()

    if not partitions:
        print("❌ NO LEVEL 3 PARTITIONS FOUND!")
        print("\nThis means wh_stock_request_DIST1759603974795_WH001 has no year_month partitions.")
        print("You need to create them first!")

        print("\n" + "=" * 80)
        print("SQL to create partition for year_month = 2510:")
        print("=" * 80)
        print("""
CREATE TABLE wh_stock_request_DIST1759603974795_WH001_2510
PARTITION OF wh_stock_request_DIST1759603974795_WH001
FOR VALUES IN (2510);
        """)
    else:
        print(f"\nFound {len(partitions)} partitions:\n")
        found_2510 = False
        for row in partitions:
            print(f"  {row[0]}: {row[1]}")
            if '2510' in row[1]:
                found_2510 = True

        if found_2510:
            print("\n✅ Partition for year_month = 2510 EXISTS!")
        else:
            print("\n❌ Partition for year_month = 2510 NOT FOUND!")
            print("\nYou need to create it:")
            print("""
CREATE TABLE wh_stock_request_DIST1759603974795_WH001_2510
PARTITION OF wh_stock_request_DIST1759603974795_WH001
FOR VALUES IN (2510);
            """)

    cursor.close()
    conn.close()

if __name__ == "__main__":
    main()
