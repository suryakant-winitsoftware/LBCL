#!/usr/bin/env python3
import psycopg2
from psycopg2.extras import RealDictCursor

DB_PARAMS = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev15072025',
    'user': 'multiplex',
    'password': 'multiplex'
}

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

for table_name in ['sku', 'sku_uom', 'sku_attributes']:
    print(f"\n{table_name.upper()} TABLE STRUCTURE:")
    print("=" * 70)

    cursor.execute(f"""
        SELECT column_name, data_type, is_nullable
        FROM information_schema.columns
        WHERE table_name = '{table_name}'
        ORDER BY ordinal_position
    """)
    columns = cursor.fetchall()

    for col in columns:
        nullable = "NULL" if col['is_nullable'] == 'YES' else "NOT NULL"
        print(f"  • {col['column_name']} ({col['data_type']}) - {nullable}")

cursor.close()
conn.close()
