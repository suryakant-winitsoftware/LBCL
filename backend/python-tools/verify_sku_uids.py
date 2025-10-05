#!/usr/bin/env python3
"""
Verify SKU UIDs and organization
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

SKU_CODES = ['EMBT3001', 'FGLL0001325', 'LL10KL01']  # Sample codes

print("=" * 70)
print("Verify SKU UIDs and Organization")
print("=" * 70)

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

try:
    print("\nSample SKU Records:")
    print("-" * 70)
    cursor.execute("""
        SELECT uid, code, name, org_uid, base_uom
        FROM sku
        WHERE code IN %s
        ORDER BY code
    """, (tuple(SKU_CODES),))

    skus = cursor.fetchall()
    for sku in skus:
        print(f"\nCode: {sku['code']}")
        print(f"  UID: {sku['uid']}")
        print(f"  Name: {sku['name']}")
        print(f"  Org UID: {sku['org_uid']}")
        print(f"  Base UOM: {sku['base_uom']}")
        print(f"  ✓ UID matches Code: {sku['uid'] == sku['code']}")

    print("\n" + "-" * 70)
    print("\nSample SKU UOM Records:")
    print("-" * 70)
    cursor.execute("""
        SELECT uid, sku_uid, code, liter
        FROM sku_uom
        WHERE sku_uid IN %s
        ORDER BY sku_uid
    """, (tuple(SKU_CODES),))

    uoms = cursor.fetchall()
    for uom in uoms:
        print(f"\nSKU UID: {uom['sku_uid']}")
        print(f"  UOM UID: {uom['uid']}")
        print(f"  UOM Code: {uom['code']}")
        print(f"  Liter: {uom['liter']}")

    print("\n" + "-" * 70)
    print("\nSample SKU Attributes:")
    print("-" * 70)
    cursor.execute("""
        SELECT uid, sku_uid, type, value
        FROM sku_attributes
        WHERE sku_uid IN %s
        ORDER BY sku_uid, type
        LIMIT 10
    """, (tuple(SKU_CODES),))

    attrs = cursor.fetchall()
    for attr in attrs:
        print(f"\nSKU UID: {attr['sku_uid']}")
        print(f"  Attr UID: {attr['uid']}")
        print(f"  Type: {attr['type']}")
        print(f"  Value: {attr['value']}")

except Exception as e:
    print(f"\n❌ Error: {e}")
    import traceback
    traceback.print_exc()
finally:
    cursor.close()
    conn.close()

print("\n" + "=" * 70)
print("Done!")
print("=" * 70)
