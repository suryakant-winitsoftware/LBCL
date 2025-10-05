#!/usr/bin/env python3
"""
Delete the SKUs we just inserted so we can re-insert with correct org and UIDs
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

SKU_CODES = [
    'EMBT3001', 'EMBT6001', 'EMBT6005', 'EMCT6001', 'EMCT6003',
    'FGLL000130L', 'FGLL0001325', 'FGLL0001625', 'FGLL0001EA', 'FGLL0001TR5',
    'LL10KL01', 'LL19KL01', 'LL325BL03', 'LL330BL01', 'LL330CL02',
    'LL500CL04', 'LL500CL05', 'LL500CL06'
]

print("=" * 70)
print("Delete Previously Inserted SKUs")
print("=" * 70)

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

try:
    print(f"\nDeleting {len(SKU_CODES)} SKUs and their related data...")
    print("-" * 70)

    # Get SKU UIDs first
    cursor.execute("""
        SELECT uid, code FROM sku
        WHERE code IN %s
    """, (tuple(SKU_CODES),))
    skus = cursor.fetchall()

    sku_uids = [sku['uid'] for sku in skus]

    print(f"Found {len(skus)} SKUs to delete:")
    for sku in skus:
        print(f"  • {sku['code']} (UID: {sku['uid']})")

    if sku_uids:
        # Delete from sku_attributes
        cursor.execute("""
            DELETE FROM sku_attributes
            WHERE sku_uid IN %s
        """, (tuple(sku_uids),))
        print(f"\n✓ Deleted {cursor.rowcount} attribute records")

        # Delete from sku_uom
        cursor.execute("""
            DELETE FROM sku_uom
            WHERE sku_uid IN %s
        """, (tuple(sku_uids),))
        print(f"✓ Deleted {cursor.rowcount} UOM records")

        # Delete from sku
        cursor.execute("""
            DELETE FROM sku
            WHERE uid IN %s
        """, (tuple(sku_uids),))
        print(f"✓ Deleted {cursor.rowcount} SKU records")

        conn.commit()
        print("\n" + "=" * 70)
        print("✅ Successfully deleted all SKU data")
        print("=" * 70)
    else:
        print("\nNo SKUs found to delete")

except Exception as e:
    print(f"\n❌ Error: {e}")
    conn.rollback()
    import traceback
    traceback.print_exc()
finally:
    cursor.close()
    conn.close()

print("\nDone!")
