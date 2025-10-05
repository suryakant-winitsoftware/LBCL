#!/usr/bin/env python3
"""Check table structure in PostgreSQL database"""

import psycopg2
from psycopg2.extras import RealDictCursor

# Database connection parameters
DB_PARAMS = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev170725FM',
    'user': 'multiplex',
    'password': 'multiplex'
}

try:
    conn = psycopg2.connect(**DB_PARAMS)
    cursor = conn.cursor(cursor_factory=RealDictCursor)

    # List all tables
    cursor.execute("""
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
        AND (LOWER(table_name) LIKE '%module%' OR LOWER(table_name) LIKE '%permission%')
        ORDER BY table_name
    """)

    tables = cursor.fetchall()
    print("Tables found (module/permission related):")
    for table in tables:
        print(f"  • {table['table_name']}")

    # If we find a modules table, check its columns
    if tables:
        table_name = tables[0]['table_name'] if 'module' in tables[0]['table_name'].lower() else None
        if table_name:
            cursor.execute(f"""
                SELECT column_name, data_type
                FROM information_schema.columns
                WHERE table_name = '{table_name}'
                ORDER BY ordinal_position
            """)
            columns = cursor.fetchall()
            print(f"\nColumns in {table_name}:")
            for col in columns:
                print(f"  • {col['column_name']} ({col['data_type']})")

    cursor.close()
    conn.close()

except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
