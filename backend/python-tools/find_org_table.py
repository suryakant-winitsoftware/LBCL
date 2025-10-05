#!/usr/bin/env python3
"""
Find organization table in multiplexdev15072025
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

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

print("=" * 70)
print("Finding Organization Table")
print("=" * 70)

try:
    # Find all tables with 'org' in the name
    print("\n1. Tables with 'org' in name:")
    print("-" * 70)
    cursor.execute("""
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
        AND LOWER(table_name) LIKE '%org%'
        ORDER BY table_name
    """)
    org_tables = cursor.fetchall()

    if org_tables:
        print(f"Found {len(org_tables)} tables:")
        for table in org_tables:
            print(f"  • {table['table_name']}")
    else:
        print("No org tables found")

    # Check for common organization table names
    print("\n2. Checking common table names:")
    print("-" * 70)
    for table_name in ['org', 'organization', 'organizations', 'org_master']:
        cursor.execute("""
            SELECT COUNT(*) as count
            FROM information_schema.tables
            WHERE table_schema = 'public'
            AND table_name = %s
        """, (table_name,))
        result = cursor.fetchone()
        exists = "✓ EXISTS" if result['count'] > 0 else "✗ Not found"
        print(f"  {table_name}: {exists}")

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
