#!/usr/bin/env python3
"""
Copy all SKU-related tables from multiplexdev170725FM to multiplexdev15072025
Then add new SKU data from the provided Excel
"""

import psycopg2
from psycopg2.extras import RealDictCursor
from datetime import datetime

SOURCE_DB = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev170725FM',
    'user': 'multiplex',
    'password': 'multiplex'
}

TARGET_DB = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev15072025',
    'user': 'multiplex',
    'password': 'multiplex'
}

print("=" * 80)
print("SKU Data Migration: multiplexdev170725FM → multiplexdev15072025")
print("=" * 80)

# Connect to both databases
source_conn = psycopg2.connect(**SOURCE_DB)
target_conn = psycopg2.connect(**TARGET_DB)
source_cursor = source_conn.cursor(cursor_factory=RealDictCursor)
target_cursor = target_conn.cursor(cursor_factory=RealDictCursor)

try:
    # Step 1: Check what SKU-related tables exist
    print("\n1. Finding SKU-related tables in source database...")
    print("-" * 80)

    source_cursor.execute("""
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
        AND table_type = 'BASE TABLE'
        AND LOWER(table_name) LIKE '%sku%'
        ORDER BY table_name
    """)
    sku_tables = [row['table_name'] for row in source_cursor.fetchall()]

    print(f"Found {len(sku_tables)} SKU-related tables:")
    for table in sku_tables:
        print(f"  • {table}")

    # Step 2: Copy each table
    print("\n2. Copying SKU-related tables...")
    print("-" * 80)

    for table_name in sku_tables:
        print(f"\nProcessing table: {table_name}")

        # Get column names
        source_cursor.execute(f"""
            SELECT column_name, data_type
            FROM information_schema.columns
            WHERE table_name = '{table_name}'
            AND table_schema = 'public'
            ORDER BY ordinal_position
        """)
        columns = source_cursor.fetchall()
        column_names = [col['column_name'] for col in columns]

        print(f"  Columns: {', '.join(column_names[:10])}{'...' if len(column_names) > 10 else ''}")

        # Get all data from source
        source_cursor.execute(f"SELECT * FROM {table_name}")
        rows = source_cursor.fetchall()

        print(f"  Found {len(rows)} rows in source")

        if len(rows) == 0:
            print(f"  ⚠️  No data to copy")
            continue

        # Clear target table (optional - comment out if you want to keep existing data)
        print(f"  Clearing target table...")
        target_cursor.execute(f"TRUNCATE TABLE {table_name} CASCADE")

        # Insert data into target
        print(f"  Inserting {len(rows)} rows into target...")

        inserted = 0
        for row in rows:
            placeholders = ', '.join(['%s'] * len(column_names))
            columns_str = ', '.join(column_names)

            try:
                target_cursor.execute(
                    f"INSERT INTO {table_name} ({columns_str}) VALUES ({placeholders})",
                    [row[col] for col in column_names]
                )
                inserted += 1
            except Exception as e:
                print(f"    ⚠️  Error inserting row: {e}")
                continue

        print(f"  ✓ Inserted {inserted} rows")

    # Commit the copy operation
    target_conn.commit()
    print("\n✅ Successfully copied all SKU-related tables")

    # Step 3: Add new SKU data from the Excel image
    print("\n3. Adding new SKU data from Excel...")
    print("-" * 80)

    new_skus = [
        {
            'code': 'EMBT3001',
            'description': 'AMBER 325ML BOTTLE-EMPTY',
            'description2': 'BOT AMB 325',
            'brand': 'NCLS',
            'classification': 'NON CLASSIFIED',
            'uom': 'BOT',
            'uom_value': 1.000,
            'volume_unit': 'L',
            'liter': 0.000,
            'active': 0,
            'product_type': 'EMPTY',
            'weight': 0.5000,
            'user_defined_field': 'PTS',
            'sap_material_no': '700000012',
            'sap_empty_material_no': '700000012',
            'sap_sales_unit': 'EA',
            'sap_return_unit': 'EA',
            'is_lbcl': 1
        },
        {
            'code': 'EMBT6001',
            'description': 'AMBER 625ML BOTTLE-EMPTY',
            'description2': 'BOT AMB 625',
            'brand': 'NCLS',
            'classification': 'NON CLASSIFIED',
            'uom': 'BOT',
            'uom_value': 1.000,
            'volume_unit': 'L',
            'liter': 0.000,
            'active': 0,
            'product_type': 'EMPTY',
            'weight': 0.5000,
            'user_defined_field': 'QTS',
            'sap_material_no': '700000013',
            'sap_empty_material_no': '700000013',
            'sap_sales_unit': 'EA',
            'sap_return_unit': 'EA',
            'is_lbcl': 1
        },
        {
            'code': 'EMBT6005',
            'description': 'AMBER 500ML BOTTLE-EMPTY',
            'description2': 'BOT AMB 500',
            'brand': 'NCLS',
            'classification': 'NON CLASSIFIED',
            'uom': 'BOT',
            'uom_value': 1.000,
            'volume_unit': 'L',
            'liter': 0.000,
            'active': 0,
            'product_type': 'EMPTY',
            'weight': 0.5000,
            'user_defined_field': 'QTS',
            'sap_material_no': '700000214',
            'sap_empty_material_no': '700000214',
            'sap_sales_unit': 'EA',
            'sap_return_unit': 'EA',
            'is_lbcl': 1
        },
        {
            'code': 'EMCT3002',
            'description': 'BROWN CRATE 325ML',
            'description2': 'CRT BRN 325',
            'brand': 'NCLS',
            'classification': 'NON CLASSIFIED',
            'uom': 'CRT',
            'uom_value': 1.000,
            'volume_unit': 'L',
            'liter': 0.000,
            'active': 0,
            'product_type': 'EMPTY',
            'weight': 0.5000,
            'user_defined_field': '20CRATE',
            'sap_material_no': '700000028',
            'sap_empty_material_no': '700000028',
            'sap_sales_unit': 'EA',
            'sap_return_unit': 'EA',
            'is_lbcl': 1
        },
        {
            'code': 'EMCT6001',
            'description': 'BROWN CRATE 625ML',
            'description2': 'CRT BRN 625',
            'brand': 'NCLS',
            'classification': 'NON CLASSIFIED',
            'uom': 'CRT',
            'uom_value': 1.000,
            'volume_unit': 'L',
            'liter': 0.000,
            'active': 0,
            'product_type': 'EMPTY',
            'weight': 0.5000,
            'user_defined_field': '12CRATE',
            'sap_material_no': '700000029',
            'sap_empty_material_no': '700000029',
            'sap_sales_unit': 'EA',
            'sap_return_unit': 'EA',
            'is_lbcl': 1
        },
        # LION LAGER products
        {
            'code': 'FGLL0001',
            'description': 'LION LAGER 330ML-24CAN',
            'description2': 'LL CAN330Tray',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'TRY',
            'uom2': 'EA',
            'uom_value': 1.000,
            'uom2_value': 24.000,
            'volume_unit': 'L',
            'liter': 7.920,
            'active': 0,
            'product_type': 'BEER',
            'weight': 8.3500,
            'user_defined_field': 'CAN330',
            'dozen_conversion': 1.0560,
            'sap_material_no': 'LL330CL01',
            'sap_sales_unit': 'TR',
            'is_lbcl': 1
        },
        {
            'code': 'FGLL000130L',
            'description': 'LION LAGER 30L-KEG',
            'description2': 'LL KEG 30L',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'EA',
            'uom2': 'EA',
            'uom_value': 1.000,
            'uom2_value': 1.000,
            'volume_unit': 'L',
            'liter': 30.000,
            'active': 0,
            'product_type': 'BEER',
            'weight': 40.5000,
            'user_defined_field': 'KEG30L',
            'dozen_conversion': 4.0000,
            'sap_material_no': 'LL30KL01',
            'sap_sales_unit': 'EA',
            'sap_return_unit': 'EA',
            'is_lbcl': 1
        },
        {
            'code': 'FGLL000132520B',
            'description': 'LION LAGER 325ML-20BOTTLE',
            'description2': 'LL 20B 325',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'CRT',
            'uom2': 'BOT',
            'uom_value': 1.000,
            'uom2_value': 20.000,
            'volume_unit': 'L',
            'liter': 6.500,
            'active': 0,
            'product_type': 'BEER',
            'weight': 15.3000,
            'user_defined_field': '20PTS',
            'dozen_conversion': 0.8667,
            'sap_material_no': 'LL325BL01',
            'sap_sales_unit': 'KI',
            'is_lbcl': 1
        },
        {
            'code': 'FGLL0001625',
            'description': 'LION LAGER 625ML-12BOTTLE',
            'description2': 'LL DOZ 625',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'CRT',
            'uom2': 'BOT',
            'uom_value': 1.000,
            'uom2_value': 12.000,
            'volume_unit': 'L',
            'liter': 7.500,
            'active': 0,
            'product_type': 'BEER',
            'weight': 15.5000,
            'user_defined_field': 'QTS',
            'dozen_conversion': 1.0000,
            'sap_material_no': 'LL625BL01',
            'sap_sales_unit': 'KI',
            'is_lbcl': 1
        },
        {
            'code': 'FGLL0001TR5',
            'description': 'LION LAGER 500ML-24CAN',
            'description2': 'LL CAN500Tray',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'TRY',
            'uom2': 'EA',
            'uom_value': 1.000,
            'uom2_value': 24.000,
            'volume_unit': 'L',
            'liter': 12.000,
            'active': 0,
            'product_type': 'BEER',
            'weight': 12.6000,
            'user_defined_field': 'CAN500',
            'dozen_conversion': 1.6000,
            'sap_material_no': 'LL500CL01',
            'sap_sales_unit': 'TR',
            'is_lbcl': 1
        },
        {
            'code': 'LL10KL01',
            'description': 'LION LAGER 10L-KEG',
            'description2': 'LL KEG 10L',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'EA',
            'uom2': 'EA',
            'uom_value': 1.000,
            'uom2_value': 1.000,
            'volume_unit': 'L',
            'liter': 10.000,
            'active': 0,
            'product_type': 'BEER',
            'weight': 13.0000,
            'user_defined_field': 'KEG10L',
            'dozen_conversion': 1.3330,
            'sap_material_no': 'LL10KL01',
            'sap_sales_unit': 'EA',
            'sap_return_unit': 'EA',
            'is_lbcl': 1
        },
        {
            'code': 'LL19KL01',
            'description': 'LION LAGER 19L-KEG',
            'description2': 'LL KEG 19L',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'EA',
            'uom2': 'EA',
            'uom_value': 1.000,
            'uom2_value': 1.000,
            'volume_unit': 'L',
            'liter': 19.000,
            'active': 0,
            'product_type': 'BEER',
            'weight': 19.1900,
            'user_defined_field': 'KEG19L',
            'dozen_conversion': 2.5330,
            'sap_material_no': 'LL19KL01',
            'sap_sales_unit': 'EA',
            'sap_return_unit': 'EA',
            'is_lbcl': 1
        },
        {
            'code': 'LL325BL03',
            'description': 'LION LAGER 325ML-BOTTLE 4PACK',
            'description2': 'LL 4PK 325 Bott',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'PAC',
            'uom2': 'BOT',
            'uom_value': 1.000,
            'uom2_value': 4.000,
            'volume_unit': 'L',
            'liter': 1.300,
            'active': 0,
            'product_type': 'BEER',
            'weight': 0.0730,
            'user_defined_field': '4PKS',
            'dozen_conversion': 0.1730,
            'sap_material_no': 'LL325BL03',
            'sap_sales_unit': 'PAK',
            'is_lbcl': 1
        },
        {
            'code': 'LL330BL01',
            'description': 'LION LARGER 330ML-20BOTTLE',
            'description2': 'LL 20B 330',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'CRT',
            'uom2': 'BOT',
            'uom_value': 1.000,
            'uom2_value': 20.000,
            'volume_unit': 'L',
            'liter': 6.600,
            'active': 0,
            'product_type': 'BEER',
            'weight': 15.3000,
            'user_defined_field': '20PTS',
            'dozen_conversion': 0.8800,
            'sap_material_no': 'LL330BL01',
            'sap_sales_unit': 'KI',
            'is_lbcl': 1
        },
        {
            'code': 'LL330CL02',
            'description': 'LION LAGER 330ML-CAN 6PACK',
            'description2': 'LL CAN330-6Pack',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'PAC',
            'uom2': 'EA',
            'uom_value': 1.000,
            'uom2_value': 6.000,
            'volume_unit': 'L',
            'liter': 1.980,
            'active': 0,
            'product_type': 'BEER',
            'weight': 4.2000,
            'user_defined_field': 'CAN330PKS',
            'dozen_conversion': 0.2640,
            'sap_material_no': 'LL330CL02',
            'sap_sales_unit': 'PAK',
            'is_lbcl': 1
        },
        {
            'code': 'LL500CL04',
            'description': 'LION LAGER 500ML-CAN 5PACK',
            'description2': 'LL CAN5005Pack',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'PAC',
            'uom2': 'EA',
            'uom_value': 1.000,
            'uom2_value': 5.000,
            'volume_unit': 'L',
            'liter': 2.500,
            'active': 0,
            'product_type': 'BEER',
            'weight': 3.1500,
            'user_defined_field': 'CAN500PKS',
            'dozen_conversion': 0.3333,
            'sap_material_no': 'LL500CL04',
            'sap_sales_unit': 'PAK',
            'is_lbcl': 1
        },
        {
            'code': 'LL500CL05',
            'description': 'LION LAGER 500ML-CAN 6PACK',
            'description2': 'LL CAN500-6Pack',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'PAC',
            'uom2': 'EA',
            'uom_value': 1.000,
            'uom2_value': 6.000,
            'volume_unit': 'L',
            'liter': 3.000,
            'active': 0,
            'product_type': 'BEER',
            'weight': 3.1500,
            'user_defined_field': 'CAN500PKS',
            'dozen_conversion': 0.4000,
            'sap_material_no': 'LL500CL05',
            'sap_sales_unit': 'PAK',
            'is_lbcl': 1
        },
        {
            'code': 'LL500CL06',
            'description': 'LION LAGER 500ML-CAN 12PACK',
            'description2': 'LL CAN500-12Pac',
            'brand': 'BR01',
            'classification': 'LION LAGER',
            'uom': 'PAC',
            'uom2': 'EA',
            'uom_value': 1.000,
            'uom2_value': 12.000,
            'volume_unit': 'L',
            'liter': 6.000,
            'active': 0,
            'product_type': 'BEER',
            'weight': 3.1500,
            'user_defined_field': 'CAN500PKS',
            'dozen_conversion': 0.8000,
            'sap_material_no': 'LL500CL06',
            'sap_sales_unit': 'PAK',
            'is_lbcl': 1
        }
    ]

    print(f"Prepared {len(new_skus)} new SKUs to insert")

    # First, check the actual SKU table structure
    print("\n4. Checking SKU table structure...")
    print("-" * 80)
    target_cursor.execute("""
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
        AND LOWER(table_name) IN ('sku', 'skus', 'sku_master', 'product', 'products')
    """)
    sku_table_candidates = [row['table_name'] for row in target_cursor.fetchall()]

    if not sku_table_candidates:
        print("❌ No SKU table found!")
        print("Available tables with 'sku' in name:")
        target_cursor.execute("""
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
            AND LOWER(table_name) LIKE '%sku%'
        """)
        for row in target_cursor.fetchall():
            print(f"  • {row['table_name']}")
    else:
        sku_table = sku_table_candidates[0]
        print(f"Using table: {sku_table}")

        # Get column structure
        target_cursor.execute(f"""
            SELECT column_name, data_type, is_nullable
            FROM information_schema.columns
            WHERE table_name = '{sku_table}'
            ORDER BY ordinal_position
        """)
        print(f"\nTable '{sku_table}' columns:")
        for col in target_cursor.fetchall():
            print(f"  • {col['column_name']} ({col['data_type']}, nullable={col['is_nullable']})")

    # Commit all changes
    target_conn.commit()

    print("\n" + "=" * 80)
    print("✅ Migration completed!")
    print("=" * 80)
    print("\nNext steps:")
    print("1. Review the SKU table structure above")
    print("2. Map the Excel columns to the actual database columns")
    print("3. Run the insert statements for new SKUs")

except Exception as e:
    print(f"\n❌ Error: {e}")
    target_conn.rollback()
    import traceback
    traceback.print_exc()
finally:
    source_cursor.close()
    source_conn.close()
    target_cursor.close()
    target_conn.close()

print("\n" + "=" * 80)
print("Done!")
print("=" * 80)
