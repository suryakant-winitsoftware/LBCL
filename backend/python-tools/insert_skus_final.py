#!/usr/bin/env python3
"""
Insert new SKU data into multiplexdev15072025 database
Based on Excel data provided
"""

import psycopg2
from psycopg2.extras import RealDictCursor
from datetime import datetime
import uuid

DB_PARAMS = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev15072025',
    'user': 'multiplex',
    'password': 'multiplex'
}

# SKU data from Excel
SKU_DATA = [
    {'ProductCode': 'EMBT3001', 'Description': 'AMBER 325ML BOTTLE-EMPTY', 'Description2': 'BOT AMB 325', 'Brand_Code': 'NCLS', 'Classification': 'NON CLASSIFIED', 'UOM1': 'BOT', 'UOM1_Value': 1.000, 'Volume_Unit': 'L', 'Liter': 0.000, 'ProductTypeUID': 'EMPTY', 'Denominations': 'KG'},
    {'ProductCode': 'EMBT6001', 'Description': 'AMBER 625ML BOTTLE-EMPTY', 'Description2': 'BOT AMB 650', 'Brand_Code': 'NCLS', 'Classification': 'NON CLASSIFIED', 'UOM1': 'BOT', 'UOM1_Value': 1.000, 'Volume_Unit': 'L', 'Liter': 0.000, 'ProductTypeUID': 'EMPTY', 'Denominations': 'KG'},
    {'ProductCode': 'EMBT6005', 'Description': 'AMBER 500ML BOTTLE-EMPTY', 'Description2': 'BOT AMB 500', 'Brand_Code': 'NCLS', 'Classification': 'NON CLASSIFIED', 'UOM1': 'BOT', 'UOM1_Value': 1.000, 'Volume_Unit': 'L', 'Liter': 0.000, 'ProductTypeUID': 'EMPTY', 'Denominations': 'KG'},
    {'ProductCode': 'EMCT6001', 'Description': 'BROWN CRATE 325ML', 'Description2': 'CRT BRN 325', 'Brand_Code': 'NCLS', 'Classification': 'NON CLASSIFIED', 'UOM1': 'CRT', 'UOM1_Value': 24.000, 'Volume_Unit': 'L', 'Liter': 0.000, 'ProductTypeUID': 'EMPTY', 'Denominations': 'KG'},
    {'ProductCode': 'EMCT6003', 'Description': 'BROWN CRATE 625ML', 'Description2': 'CRT BRN 625', 'Brand_Code': 'NCLS', 'Classification': 'NON CLASSIFIED', 'UOM1': 'CRT', 'UOM1_Value': 12.000, 'Volume_Unit': 'L', 'Liter': 0.000, 'ProductTypeUID': 'EMPTY', 'Denominations': 'KG'},
    {'ProductCode': 'FGLL000130L', 'Description': 'LION LAGER 30ML-KEG', 'Description2': 'LL DOZ 30L', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'CRT', 'UOM1_Value': 1.000, 'Volume_Unit': 'L', 'Liter': 30.000, 'ProductTypeUID': 'BEER', 'Denominations': 'KG'},
    {'ProductCode': 'FGLL0001325', 'Description': 'LION LAGER 325ML-20BOTTLE', 'Description2': 'LL 20B 325', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'BOT', 'UOM1_Value': 20.000, 'Volume_Unit': 'L', 'Liter': 6.500, 'ProductTypeUID': 'BEER', 'Denominations': 'KG'},
    {'ProductCode': 'FGLL0001625', 'Description': 'LION LAGER 625ML-12BOTTLE', 'Description2': 'LL DOZ 625', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'BOT', 'UOM1_Value': 12.000, 'Volume_Unit': 'L', 'Liter': 7.500, 'ProductTypeUID': 'BEER', 'Denominations': 'KG'},
    {'ProductCode': 'FGLL0001EA', 'Description': 'LION LAGER 330ML-24CAN', 'Description2': 'LL CAN500 Tray', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'EA', 'UOM1_Value': 24.000, 'Volume_Unit': 'L', 'Liter': 12.000, 'ProductTypeUID': 'BEER', 'Denominations': 'KG'},
    {'ProductCode': 'FGLL0001TR5', 'Description': 'LION LAGER 500ML-24CAN', 'Description2': 'LL KEG 10L', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'EA', 'UOM1_Value': 1.000, 'Volume_Unit': 'L', 'Liter': 10.000, 'ProductTypeUID': 'BEER', 'Denominations': 'KG'},
    {'ProductCode': 'LL10KL01', 'Description': 'LION LAGER 10L-KEG', 'Description2': 'LL KEG 19L', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'EA', 'UOM1_Value': 1.000, 'Volume_Unit': 'L', 'Liter': 10.000, 'ProductTypeUID': 'BEER', 'Denominations': 'KG'},
    {'ProductCode': 'LL19KL01', 'Description': 'LION LAGER 19L-KEG', 'Description2': 'LL 4PK 325 Bon', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'PAC', 'UOM1_Value': 4.000, 'Volume_Unit': 'L', 'Liter': 1.300, 'ProductTypeUID': 'BEER', 'Denominations': 'PAK'},
    {'ProductCode': 'LL325BL03', 'Description': 'LION LAGER 325ML-BOTTLE 4PACK', 'Description2': 'LL 20B 330', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'BOT', 'UOM1_Value': 20.000, 'Volume_Unit': 'L', 'Liter': 6.600, 'ProductTypeUID': 'BEER', 'Denominations': 'KT'},
    {'ProductCode': 'LL330BL01', 'Description': 'LION LARGER 330ML-20BOTTLE', 'Description2': 'LL CAN330 6Pack', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'PAC', 'UOM1_Value': 6.000, 'Volume_Unit': 'L', 'Liter': 1.980, 'ProductTypeUID': 'BEER', 'Denominations': 'PAK'},
    {'ProductCode': 'LL330CL02', 'Description': 'LION LAGER 330ML-CAN 6PACK', 'Description2': 'LL CAN500 5Pack', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'EA', 'UOM1_Value': 5.000, 'Volume_Unit': 'L', 'Liter': 2.500, 'ProductTypeUID': 'BEER', 'Denominations': 'PAK'},
    {'ProductCode': 'LL500CL04', 'Description': 'LION LAGER 500ML-CAN 5PACK', 'Description2': 'LL CAN500 6Pack', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'EA', 'UOM1_Value': 6.000, 'Volume_Unit': 'L', 'Liter': 3.000, 'ProductTypeUID': 'BEER', 'Denominations': 'PAK'},
    {'ProductCode': 'LL500CL05', 'Description': 'LION LAGER 500ML-CAN 6PACK', 'Description2': 'LL CAN500 12Pac', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'EA', 'UOM1_Value': 12.000, 'Volume_Unit': 'L', 'Liter': 6.000, 'ProductTypeUID': 'BEER', 'Denominations': 'PAK'},
    {'ProductCode': 'LL500CL06', 'Description': 'LION LAGER 500ML-CAN 12PACK', 'Description2': '', 'Brand_Code': 'lion1', 'Classification': 'LION LAGER', 'UOM1': 'EA', 'UOM1_Value': 12.000, 'Volume_Unit': 'L', 'Liter': 6.000, 'ProductTypeUID': 'BEER', 'Denominations': 'PAK'},
]

print("=" * 70)
print("Insert SKU Data into multiplexdev15072025")
print("=" * 70)

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

try:
    # First, get the Principal org UID
    print("\n1. Getting Principal organization UID:")
    print("-" * 70)
    cursor.execute("""
        SELECT uid, name
        FROM org
        WHERE org_type_uid = 'Principal' AND is_active = TRUE
        ORDER BY created_time DESC
        LIMIT 1
    """)
    org = cursor.fetchone()

    if org:
        org_uid = org['uid']
        print(f"Using Principal org: {org['name']} ({org_uid})")
    else:
        print("❌ No Principal organization found!")
        raise Exception("Principal organization not found")

    print(f"\n2. Inserting {len(SKU_DATA)} SKUs:")
    print("-" * 70)

    inserted_count = 0
    skipped_count = 0

    for sku_data in SKU_DATA:
        product_code = sku_data['ProductCode']

        # Check if SKU already exists
        cursor.execute("""
            SELECT uid FROM sku
            WHERE code = %s AND is_active = TRUE
        """, (product_code,))

        existing = cursor.fetchone()

        if existing:
            print(f"  ⏭  Skipped (exists): {product_code}")
            skipped_count += 1
            continue

        # Use SKU Code as UID
        sku_uid = product_code
        uom_uid = f"{product_code}_UOM"

        # Insert into SKU table
        cursor.execute("""
            INSERT INTO sku (
                uid, created_by, created_time, modified_by, modified_time,
                org_uid, code, name, long_name,
                base_uom, is_active, is_stockable, ss
            ) VALUES (
                %s, 'ADMIN', NOW(), 'ADMIN', NOW(),
                %s, %s, %s, %s,
                %s, TRUE, TRUE, 1
            )
        """, (
            sku_uid,
            org_uid,
            product_code,
            sku_data['Description'],
            sku_data['Description2'],
            sku_data['UOM1']
        ))

        # Insert into SKU UOM table
        cursor.execute("""
            INSERT INTO sku_uom (
                uid, created_by, created_time, modified_by, modified_time,
                sku_uid, code, name, label,
                is_base_uom, multiplier,
                volume_unit, liter
            ) VALUES (
                %s, 'ADMIN', NOW(), 'ADMIN', NOW(),
                %s, %s, %s, %s,
                TRUE, %s,
                %s, %s
            )
        """, (
            uom_uid,
            sku_uid,
            sku_data['UOM1'],
            sku_data['UOM1'],
            sku_data['UOM1'],
            sku_data['UOM1_Value'],
            sku_data['Volume_Unit'],
            sku_data['Liter']
        ))

        # Insert SKU attributes
        attributes = [
            ('Brand', sku_data['Brand_Code']),
            ('Classification', sku_data['Classification']),
            ('ProductType', sku_data.get('ProductTypeUID', '')),
            ('Denomination', sku_data.get('Denominations', '')),
        ]

        for attr_type, attr_value in attributes:
            if attr_value:
                attr_uid = f"{product_code}_{attr_type}"
                cursor.execute("""
                    INSERT INTO sku_attributes (
                        uid, created_by, created_time, modified_by, modified_time,
                        sku_uid, type, code, value
                    ) VALUES (
                        %s, 'ADMIN', NOW(), 'ADMIN', NOW(),
                        %s, %s, %s, %s
                    )
                """, (attr_uid, sku_uid, attr_type, attr_value, attr_value))

        print(f"  ✓ Inserted: {product_code} - {sku_data['Description']}")
        inserted_count += 1

    # Commit all changes
    conn.commit()

    print("\n" + "=" * 70)
    print(f"✅ Successfully inserted {inserted_count} SKUs")
    print(f"⏭  Skipped {skipped_count} existing SKUs")
    print("=" * 70)

    # Verify the insertions
    print("\n3. Verifying inserted SKUs:")
    print("-" * 70)
    cursor.execute("""
        SELECT s.code, s.name, s.base_uom, u.liter
        FROM sku s
        LEFT JOIN sku_uom u ON s.uid = u.sku_uid
        WHERE s.code IN %s
        AND s.is_active = TRUE
        ORDER BY s.code
    """, (tuple([sku['ProductCode'] for sku in SKU_DATA]),))

    verified = cursor.fetchall()
    print(f"Found {len(verified)} SKUs in database:")
    for sku in verified:
        print(f"  • {sku['code']}: {sku['name']} (UOM: {sku['base_uom']}, Liter: {sku['liter']})")

except Exception as e:
    print(f"\n❌ Error: {e}")
    conn.rollback()
    import traceback
    traceback.print_exc()
finally:
    cursor.close()
    conn.close()

print("\n" + "=" * 70)
print("Done!")
print("=" * 70)
