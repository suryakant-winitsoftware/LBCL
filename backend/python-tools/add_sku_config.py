#!/usr/bin/env python3
"""
Add SKU config entries for all our new SKUs
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
print("Adding SKU Config Entries")
print("=" * 70)

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

try:
    print("\nCreating sku_config entries for all SKUs...")
    print("-" * 70)

    inserted_count = 0

    for sku_code in SKU_CODES:
        # Check if config already exists
        cursor.execute("""
            SELECT uid FROM sku_config
            WHERE sku_uid = %s
        """, (sku_code,))

        existing = cursor.fetchone()

        if existing:
            print(f"  ⏭  Skipped (exists): {sku_code}")
            continue

        # Create config UID
        config_uid = f"{sku_code}_CONFIG"

        # Insert sku_config
        cursor.execute("""
            INSERT INTO sku_config (
                uid, created_by, created_time, modified_by, modified_time,
                org_uid, sku_uid,
                can_buy, can_sell,
                buying_uom, selling_uom,
                is_active
            ) VALUES (
                %s, 'ADMIN', NOW(), 'ADMIN', NOW(),
                'LBCL', %s,
                TRUE, TRUE,
                (SELECT base_uom FROM sku WHERE uid = %s),
                (SELECT base_uom FROM sku WHERE uid = %s),
                TRUE
            )
        """, (config_uid, sku_code, sku_code, sku_code))

        print(f"  ✓ Created config: {sku_code}")
        inserted_count += 1

    conn.commit()

    print("\n" + "=" * 70)
    print(f"✅ Successfully created {inserted_count} SKU config entries")
    print("=" * 70)

    # Verify
    print("\nVerifying config entries:")
    print("-" * 70)
    cursor.execute("""
        SELECT sc.sku_uid, sc.can_buy, sc.can_sell, sc.buying_uom, sc.selling_uom
        FROM sku_config sc
        WHERE sc.sku_uid IN %s
        ORDER BY sc.sku_uid
    """, (tuple(SKU_CODES),))

    configs = cursor.fetchall()
    print(f"Found {len(configs)} config entries:")
    for config in configs:
        print(f"  • {config['sku_uid']}: Buy={config['can_buy']}, Sell={config['can_sell']}, BuyUOM={config['buying_uom']}, SellUOM={config['selling_uom']}")

except Exception as e:
    print(f"\n❌ Error: {e}")
    conn.rollback()
    import traceback
    traceback.print_exc()
finally:
    cursor.close()
    conn.close()

print("\nDone!")
