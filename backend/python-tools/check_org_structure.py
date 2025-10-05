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

print("ORG TABLE STRUCTURE:")
print("=" * 70)

cursor.execute("""
    SELECT column_name, data_type
    FROM information_schema.columns
    WHERE table_name = 'org'
    ORDER BY ordinal_position
""")
columns = cursor.fetchall()

for col in columns:
    print(f"  • {col['column_name']} ({col['data_type']})")

print("\nSAMPLE ORG DATA:")
print("=" * 70)
cursor.execute("SELECT * FROM org LIMIT 1")
sample = cursor.fetchone()

if sample:
    for key, value in sample.items():
        print(f"  {key}: {value}")

cursor.close()
conn.close()
