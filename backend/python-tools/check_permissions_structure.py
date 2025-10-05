#!/usr/bin/env python3
"""Check permissions table structure"""

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

# Check normal_permissions columns
cursor.execute("""
    SELECT column_name, data_type
    FROM information_schema.columns
    WHERE table_name = 'normal_permissions'
    ORDER BY ordinal_position
""")
print("Columns in normal_permissions:")
for col in cursor.fetchall():
    print(f"  • {col['column_name']} ({col['data_type']})")

# Check a sample permission
cursor.execute("SELECT * FROM normal_permissions LIMIT 1")
sample = cursor.fetchone()
if sample:
    print("\nSample permission:")
    for key, value in sample.items():
        print(f"  {key}: {value}")

cursor.close()
conn.close()
