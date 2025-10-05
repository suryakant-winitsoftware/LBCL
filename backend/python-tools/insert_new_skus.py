#!/usr/bin/env python3
"""
Insert new SKU data into multiplexdev15072025 database
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

print("=" * 70)
print("Insert New SKU Data")
print("=" * 70)

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

try:
    # 1. First, discover what SKU tables exist
    print("\n1. Discovering SKU-related tables:")
    print("-" * 70)
    cursor.execute("""
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
        AND LOWER(table_name) LIKE '%sku%'
        ORDER BY table_name
    """)
    tables = cursor.fetchall()

    print(f"Found {len(tables)} SKU-related tables:")
    for table in tables:
        print(f"  • {table['table_name']}")

    # 2. Check the main SKU table structure
    print("\n2. Checking SKU table structure:")
    print("-" * 70)

    # Try common SKU table names
    sku_table_name = None
    for possible_name in ['sku', 'sku_master', 'skus', 'sku_details']:
        try:
            cursor.execute(f"""
                SELECT column_name, data_type, is_nullable
                FROM information_schema.columns
                WHERE table_schema = 'public'
                AND table_name = '{possible_name}'
                ORDER BY ordinal_position
            """)
            columns = cursor.fetchall()
            if columns:
                sku_table_name = possible_name
                print(f"\nFound SKU table: {sku_table_name}")
                print(f"Columns ({len(columns)}):")
                for col in columns:
                    print(f"  • {col['column_name']} ({col['data_type']}) - {'NULL' if col['is_nullable'] == 'YES' else 'NOT NULL'}")
                break
        except:
            pass

    if not sku_table_name:
        print("❌ Could not find main SKU table!")
        print("\nAvailable tables:")
        for table in tables:
            print(f"  • {table['table_name']}")

    # 3. Check if there's sample data to understand the structure
    if sku_table_name:
        print(f"\n3. Sample data from {sku_table_name}:")
        print("-" * 70)
        cursor.execute(f"SELECT * FROM {sku_table_name} WHERE ss = 1 LIMIT 3")
        sample_rows = cursor.fetchall()

        if sample_rows:
            print(f"Found {len(sample_rows)} sample rows")
            for i, row in enumerate(sample_rows, 1):
                print(f"\nRow {i}:")
                for key, value in row.items():
                    if value is not None:
                        print(f"  {key}: {value}")
        else:
            print("No sample data found (table might be empty)")

    # 4. Prepare the new SKU data from Excel
    print("\n4. New SKU data to insert:")
    print("-" * 70)

    new_skus = [
        {
            'product_code': 'EMBT3001',
            'description': 'AMBER 325ML BOTTLE-EMPTY',
            'description2': 'BOT AMB 325',
            'brand_code': 'NCLS',
            'classification': 'NON CLASSIFIED',
            'uom': 'BOT',
            'uom_value': 1.000,
        },
        {
            'product_code': 'EMBT6001',
            'description': 'AMBER 625ML BOTTLE-EMPTY',
            'description2': 'BOT AMB 650',
            'brand_code': 'NCLS',
            'classification': 'NON CLASSIFIED',
            'uom': 'BOT',
            'uom_value': 1.000,
        },
        {
            'product_code': 'EMBT6005',
            'description': 'AMBER 500ML BOTTLE-EMPTY',
            'description2': 'BOT AMB 500',
            'brand_code': 'NCLS',
            'classification': 'NON CLASSIFIED',
            'uom': 'BOT',
            'uom_value': 1.000,
        },
        {
            'product_code': 'EMCT6001',
            'description': 'BROWN CRATE 325ML',
            'description2': 'CRT BRN 325',
            'brand_code': 'NCLS',
            'classification': 'NON CLASSIFIED',
            'uom': 'CRT',
            'uom_value': 24.000,
        },
        {
            'product_code': 'EMCT6003',
            'description': 'BROWN CRATE 625ML',
            'description2': 'CRT BRN 625',
            'brand_code': 'NCLS',
            'classification': 'NON CLASSIFIED',
            'uom': 'CRT',
            'uom_value': 12.000,
        },
        {
            'product_code': 'FGLL000130L',
            'description': 'LION LAGER 30ML-KEG',
            'description2': 'LL DOZ 30L',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'EA',
            'uom_value': 1.000,
        },
        {
            'product_code': 'FGLL0001325',
            'description': 'LION LAGER 325ML-20BOTTLE',
            'description2': 'LL 20B 325',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'BOT',
            'uom_value': 20.000,
        },
        {
            'product_code': 'FGLL0001625',
            'description': 'LION LAGER 625ML-12BOTTLE',
            'description2': 'LL DOZ 625',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'BOT',
            'uom_value': 12.000,
        },
        {
            'product_code': 'FGLL0001EA',
            'description': 'LION LAGER 330ML-24CAN',
            'description2': 'LL CAN500 Tray',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'EA',
            'uom_value': 24.000,
        },
        {
            'product_code': 'FGLL0001TR5',
            'description': 'LION LAGER 500ML-24CAN',
            'description2': 'LL KEG 10L',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'EA',
            'uom_value': 1.000,
        },
        {
            'product_code': 'LL10KL01',
            'description': 'LION LAGER 10L-KEG',
            'description2': 'LL KEG 19L',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'EA',
            'uom_value': 1.000,
        },
        {
            'product_code': 'LL19KL01',
            'description': 'LION LAGER 19L-KEG',
            'description2': 'LL 4PK 325 Bon',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'PAC',
            'uom_value': 4.000,
        },
        {
            'product_code': 'LL325BL03',
            'description': 'LION LAGER 325ML-BOTTLE 4PACK',
            'description2': 'LL 20B 330',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'BOT',
            'uom_value': 20.000,
        },
        {
            'product_code': 'LL330BL01',
            'description': 'LION LARGER 330ML-20BOTTLE',
            'description2': 'LL CAN330 6Pack',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'PAC',
            'uom_value': 6.000,
        },
        {
            'product_code': 'LL330CL02',
            'description': 'LION LAGER 330ML-CAN 6PACK',
            'description2': 'LL CAN500 5Pack',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'EA',
            'uom_value': 5.000,
        },
        {
            'product_code': 'LL500CL04',
            'description': 'LION LAGER 500ML-CAN 5PACK',
            'description2': 'LL CAN500 6Pack',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'EA',
            'uom_value': 6.000,
        },
        {
            'product_code': 'LL500CL05',
            'description': 'LION LAGER 500ML-CAN 6PACK',
            'description2': 'LL CAN500 12Pac',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'EA',
            'uom_value': 12.000,
        },
        {
            'product_code': 'LL500CL06',
            'description': 'LION LAGER 500ML-CAN 12PACK',
            'description2': '',
            'brand_code': 'lion1',
            'classification': 'LION LAGER',
            'uom': 'EA',
            'uom_value': 12.000,
        },
    ]

    print(f"Prepared {len(new_skus)} SKUs to insert:")
    for sku in new_skus:
        print(f"  • {sku['product_code']}: {sku['description']}")

    print("\n" + "=" * 70)
    print("Next Steps:")
    print("=" * 70)
    print("1. Review the table structure above")
    print("2. Map the Excel columns to database columns")
    print("3. Insert the new SKU data")
    print("=" * 70)

except Exception as e:
    print(f"\n❌ Error: {e}")
    import traceback
    traceback.print_exc()
finally:
    cursor.close()
    conn.close()

print("\nDone!")
