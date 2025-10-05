#!/usr/bin/env python3
"""
Check reference SKU data from multiplexdev170725FM
"""

import psycopg2
from psycopg2.extras import RealDictCursor

DB_PARAMS = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev170725FM',
    'user': 'multiplex',
    'password': 'multiplex'
}

print("=" * 70)
print("Reference SKU Data from multiplexdev170725FM")
print("=" * 70)

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

try:
    # 1. Check SKU table structure
    print("\n1. SKU table structure:")
    print("-" * 70)
    cursor.execute("""
        SELECT column_name, data_type, is_nullable
        FROM information_schema.columns
        WHERE table_schema = 'public'
        AND table_name = 'sku'
        ORDER BY ordinal_position
    """)
    columns = cursor.fetchall()
    for col in columns:
        print(f"  • {col['column_name']} ({col['data_type']}) - {'NULL' if col['is_nullable'] == 'YES' else 'NOT NULL'}")

    # 2. Get sample SKU data
    print("\n2. Sample SKU records:")
    print("-" * 70)
    cursor.execute("""
        SELECT *
        FROM sku
        WHERE ss = 1
        ORDER BY created_time DESC
        LIMIT 3
    """)
    sample_skus = cursor.fetchall()

    for i, sku in enumerate(sample_skus, 1):
        print(f"\nSKU {i}: {sku.get('code', 'N/A')} - {sku.get('name', 'N/A')}")
        for key, value in sku.items():
            if value is not None and key != 'id':
                print(f"  {key}: {value}")

    # 3. Check SKU attributes
    print("\n\n3. SKU attributes table:")
    print("-" * 70)
    cursor.execute("""
        SELECT column_name, data_type
        FROM information_schema.columns
        WHERE table_schema = 'public'
        AND table_name = 'sku_attributes'
        ORDER BY ordinal_position
    """)
    attr_columns = cursor.fetchall()
    print(f"Columns ({len(attr_columns)}):")
    for col in attr_columns:
        print(f"  • {col['column_name']} ({col['data_type']})")

    # Get sample attributes
    cursor.execute("""
        SELECT *
        FROM sku_attributes
        WHERE ss = 1
        LIMIT 3
    """)
    sample_attrs = cursor.fetchall()

    if sample_attrs:
        print(f"\nSample attribute records ({len(sample_attrs)}):")
        for i, attr in enumerate(sample_attrs, 1):
            print(f"\nAttribute {i}:")
            for key, value in attr.items():
                if value is not None:
                    print(f"  {key}: {value}")

    # 4. Check SKU UOM table
    print("\n\n4. SKU UOM table:")
    print("-" * 70)
    cursor.execute("""
        SELECT column_name, data_type
        FROM information_schema.columns
        WHERE table_schema = 'public'
        AND table_name = 'sku_uom'
        ORDER BY ordinal_position
    """)
    uom_columns = cursor.fetchall()
    print(f"Columns ({len(uom_columns)}):")
    for col in uom_columns:
        print(f"  • {col['column_name']} ({col['data_type']})")

    # Get sample UOM data
    cursor.execute("""
        SELECT *
        FROM sku_uom
        WHERE ss = 1
        LIMIT 3
    """)
    sample_uoms = cursor.fetchall()

    if sample_uoms:
        print(f"\nSample UOM records ({len(sample_uoms)}):")
        for i, uom in enumerate(sample_uoms, 1):
            print(f"\nUOM {i}:")
            for key, value in uom.items():
                if value is not None:
                    print(f"  {key}: {value}")

    # 5. Check SKU group
    print("\n\n5. SKU group table:")
    print("-" * 70)
    cursor.execute("""
        SELECT column_name, data_type
        FROM information_schema.columns
        WHERE table_schema = 'public'
        AND table_name = 'sku_group'
        ORDER BY ordinal_position
    """)
    group_columns = cursor.fetchall()
    print(f"Columns ({len(group_columns)}):")
    for col in group_columns:
        print(f"  • {col['column_name']} ({col['data_type']})")

    # 6. Check SKU group type
    print("\n\n6. SKU group type table:")
    print("-" * 70)
    cursor.execute("""
        SELECT column_name, data_type
        FROM information_schema.columns
        WHERE table_schema = 'public'
        AND table_name = 'sku_group_type'
        ORDER BY ordinal_position
    """)
    group_type_columns = cursor.fetchall()
    print(f"Columns ({len(group_type_columns)}):")
    for col in group_type_columns:
        print(f"  • {col['column_name']} ({col['data_type']})")

    # Get sample group types
    cursor.execute("""
        SELECT *
        FROM sku_group_type
        WHERE ss = 1
        LIMIT 5
    """)
    sample_group_types = cursor.fetchall()

    if sample_group_types:
        print(f"\nSample group type records ({len(sample_group_types)}):")
        for i, gt in enumerate(sample_group_types, 1):
            print(f"  {i}. {gt.get('name', 'N/A')} (UID: {gt.get('uid', 'N/A')})")

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
