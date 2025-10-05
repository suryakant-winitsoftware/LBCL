#!/usr/bin/env python3
"""
Create missing partition for wh_stock_request table
"""

import psycopg2
import sys

DB_PARAMS = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev15072025',
    'user': 'multiplex',
    'password': 'multiplex'
}

def create_partition(org_uid, warehouse_uid=None, year_month=None):
    """
    Create partition for wh_stock_request table
    Partition structure: org_uid -> org_uid_warehouse_uid -> org_uid_warehouse_uid_yearmonth
    """
    conn = psycopg2.connect(**DB_PARAMS)
    cursor = conn.cursor()

    try:
        # Level 1: org_uid partition
        partition_name_l1 = f"wh_stock_request_{org_uid}"

        print(f"Creating Level 1 partition: {partition_name_l1}")
        cursor.execute(f"""
            CREATE TABLE IF NOT EXISTS {partition_name_l1}
            PARTITION OF wh_stock_request
            FOR VALUES IN ('{org_uid}')
            PARTITION BY LIST (warehouse_uid);
        """)
        conn.commit()
        print(f"✓ Created {partition_name_l1}")

        if warehouse_uid:
            # Level 2: org_uid_warehouse_uid partition
            partition_name_l2 = f"wh_stock_request_{org_uid}_{warehouse_uid}"

            print(f"Creating Level 2 partition: {partition_name_l2}")
            cursor.execute(f"""
                CREATE TABLE IF NOT EXISTS {partition_name_l2}
                PARTITION OF {partition_name_l1}
                FOR VALUES IN ('{org_uid}_{warehouse_uid}')
                PARTITION BY LIST (year_month);
            """)
            conn.commit()
            print(f"✓ Created {partition_name_l2}")

            if year_month:
                # Level 3: org_uid_warehouse_uid_yearmonth partition
                partition_name_l3 = f"wh_stock_request_{org_uid}_{warehouse_uid}_{year_month}"

                print(f"Creating Level 3 partition: {partition_name_l3}")
                cursor.execute(f"""
                    CREATE TABLE IF NOT EXISTS {partition_name_l3}
                    PARTITION OF {partition_name_l2}
                    FOR VALUES IN ({year_month});
                """)
                conn.commit()
                print(f"✓ Created {partition_name_l3}")

        print("\n✅ Partitions created successfully!")

    except Exception as e:
        print(f"❌ Error: {e}")
        conn.rollback()
        import traceback
        traceback.print_exc()
    finally:
        cursor.close()
        conn.close()

if __name__ == "__main__":
    # Create partitions for LBCL organization
    print("=" * 70)
    print("Creating partitions for LBCL organization")
    print("=" * 70)

    # LBCL org
    create_partition('LBCL', 'WH001', 2510)  # Oct 2025

    # DIST1759603974795 org with WH001
    print("\n" + "=" * 70)
    print("Creating partitions for DIST1759603974795_WH001")
    print("=" * 70)
    create_partition('DIST1759603974795', 'WH001', 2510)  # Oct 2025

    print("\nDone!")
