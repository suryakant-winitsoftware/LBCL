#!/usr/bin/env python3
"""Check column names in tables"""

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

# Check sub_modules columns
cursor.execute("""
    SELECT column_name, data_type
    FROM information_schema.columns
    WHERE table_name = 'sub_modules'
    ORDER BY ordinal_position
""")
print("Columns in sub_modules:")
for col in cursor.fetchall():
    print(f"  • {col['column_name']} ({col['data_type']})")

# Check sub_sub_modules columns
cursor.execute("""
    SELECT column_name, data_type
    FROM information_schema.columns
    WHERE table_name = 'sub_sub_modules'
    ORDER BY ordinal_position
""")
print("\nColumns in sub_sub_modules:")
for col in cursor.fetchall():
    print(f"  • {col['column_name']} ({col['data_type']})")

cursor.close()
conn.close()
