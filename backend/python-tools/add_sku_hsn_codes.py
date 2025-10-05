#!/usr/bin/env python3
"""
Add HSN codes to sku_ext_data for all our SKUs
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

# SKU HSN code mapping
# Using common HSN codes for beer and packaging materials
SKU_HSN_CODES = {
    # Empty bottles and crates - Packaging material
    'EMBT3001': '70109010',  # Glass bottles
    'EMBT6001': '70109010',  # Glass bottles
    'EMBT6005': '70109010',  # Glass bottles
    'EMCT6001': '39231000',  # Plastic crates
    'EMCT6003': '39231000',  # Plastic crates

    # Lion Lager products - Beer
    'FGLL000130L': '22030010',   # Beer in kegs
    'FGLL0001325': '22030010',   # Beer in bottles
    'FGLL0001625': '22030010',   # Beer in bottles
    'FGLL0001EA': '22030010',    # Beer in cans
    'FGLL0001TR5': '22030010',   # Beer in cans
    'LL10KL01': '22030010',      # Beer in kegs
    'LL19KL01': '22030010',      # Beer in kegs
    'LL325BL03': '22030010',     # Beer in bottles
    'LL330BL01': '22030010',     # Beer in bottles
    'LL330CL02': '22030010',     # Beer in cans
    'LL500CL04': '22030010',     # Beer in cans
    'LL500CL05': '22030010',     # Beer in cans
    'LL500CL06': '22030010',     # Beer in cans
}

print("=" * 70)
print("Adding HSN Codes to SKU Extended Data")
print("=" * 70)

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

try:
    inserted_count = 0
    updated_count = 0

    for sku_code, hsn_code in SKU_HSN_CODES.items():
        # Check if ext_data exists
        cursor.execute("""
            SELECT uid FROM sku_ext_data
            WHERE sku_uid = %s
        """, (sku_code,))

        existing = cursor.fetchone()

        if existing:
            # Update existing record
            cursor.execute("""
                UPDATE sku_ext_data
                SET hsn_code = %s,
                    modified_by = 'ADMIN',
                    modified_time = NOW()
                WHERE sku_uid = %s
            """, (hsn_code, sku_code))
            print(f"  ✓ Updated: {sku_code} - HSN: {hsn_code}")
            updated_count += 1
        else:
            # Insert new record
            ext_uid = f"{sku_code}_EXT"
            cursor.execute("""
                INSERT INTO sku_ext_data (
                    uid, created_by, created_time, modified_by, modified_time,
                    sku_uid, hsn_code, ss
                ) VALUES (
                    %s, 'ADMIN', NOW(), 'ADMIN', NOW(),
                    %s, %s, 1
                )
            """, (ext_uid, sku_code, hsn_code))
            print(f"  ✓ Inserted: {sku_code} - HSN: {hsn_code}")
            inserted_count += 1

    conn.commit()

    print("\n" + "=" * 70)
    print(f"✅ Successfully added HSN codes")
    print(f"   Inserted: {inserted_count}")
    print(f"   Updated: {updated_count}")
    print("=" * 70)

    # Verify
    print("\nVerifying HSN codes:")
    print("-" * 70)
    cursor.execute("""
        SELECT e.sku_uid, s.name, e.hsn_code
        FROM sku_ext_data e
        JOIN sku s ON e.sku_uid = s.uid
        WHERE e.sku_uid IN %s
        ORDER BY e.sku_uid
    """, (tuple(SKU_HSN_CODES.keys()),))

    ext_data = cursor.fetchall()
    print(f"Found {len(ext_data)} SKUs with HSN codes:")
    for data in ext_data:
        print(f"  • {data['sku_uid']}: {data['name'][:40]:40} - HSN: {data['hsn_code']}")

except Exception as e:
    print(f"\n❌ Error: {e}")
    conn.rollback()
    import traceback
    traceback.print_exc()
finally:
    cursor.close()
    conn.close()

print("\nDone!")
