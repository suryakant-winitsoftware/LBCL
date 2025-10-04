import psycopg2
import pymssql
from typing import List, Tuple

PG_CONFIG = {
    'host': '10.20.53.130',
    'port': 5432,
    'database': 'multiplexdev170725FM',
    'user': 'multiplex',
    'password': 'multiplex'
}

MSSQL_CONFIG = {
    'server': '10.20.53.175',
    'database': 'LBSSFADev',
    'user': 'lbssfadev',
    'password': 'lbssfadev'
}

# Tables to copy (structure + data)
TABLES_TO_COPY = [
    # Employee/Team Management
    'employees',
    'emp',
    'emp_info',
    'emp_org_mapping',
    'job_position',
    'job_reporting',

    # Access Control/Permissions
    'modules',
    'sub_modules',
    'sub_sub_modules',
    'roles',
    'user_role',
    'normal_permissions',
    'principal_permissions',
    'mobile_access_group',
    'mobile_access_group_role',
]

def get_pg_connection():
    return psycopg2.connect(**PG_CONFIG)

def get_mssql_connection():
    return pymssql.connect(**MSSQL_CONFIG)

def get_table_columns(pg_conn, table_name: str) -> List[str]:
    cursor = pg_conn.cursor()
    cursor.execute("""
        SELECT column_name
        FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = %s
        ORDER BY ordinal_position
    """, (table_name,))
    columns = [row[0] for row in cursor.fetchall()]
    cursor.close()
    return columns

def copy_table_data(pg_conn, mssql_conn, table_name: str) -> Tuple[int, int, str]:
    try:
        pg_cursor = pg_conn.cursor()
        mssql_cursor = mssql_conn.cursor()

        # Check if table exists in MSSQL
        mssql_cursor.execute(f"""
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = '{table_name}'
        """)
        if mssql_cursor.fetchone()[0] == 0:
            mssql_cursor.close()
            pg_cursor.close()
            return 0, 0, "Table not found in MSSQL"

        # Get columns
        columns = get_table_columns(pg_conn, table_name)
        if not columns:
            mssql_cursor.close()
            pg_cursor.close()
            return 0, 0, "No columns found"

        # Fetch data from PostgreSQL
        pg_cursor.execute(f"SELECT {', '.join(columns)} FROM {table_name}")
        rows = pg_cursor.fetchall()

        if not rows:
            mssql_cursor.close()
            pg_cursor.close()
            return 0, 0, "No data"

        # Clear existing data
        mssql_cursor.execute(f"DELETE FROM [{table_name}]")
        mssql_conn.commit()

        # Insert data
        placeholders = ', '.join(['%s'] * len(columns))
        insert_sql = f"INSERT INTO [{table_name}] ({', '.join([f'[{col}]' for col in columns])}) VALUES ({placeholders})"

        inserted = 0
        errors = 0

        for row in rows:
            try:
                clean_row = []
                for val in row:
                    if val is None:
                        clean_row.append(None)
                    elif isinstance(val, (list, dict)):
                        clean_row.append(str(val))
                    else:
                        clean_row.append(val)

                mssql_cursor.execute(insert_sql, tuple(clean_row))
                inserted += 1
            except Exception as e:
                errors += 1

        mssql_conn.commit()
        mssql_cursor.close()
        pg_cursor.close()

        return len(rows), inserted, f"{inserted} of {len(rows)} rows inserted"

    except Exception as e:
        return 0, 0, f"Error: {str(e)[:100]}"

def main():
    print("=" * 80)
    print("Copying Administration Tables (Structure + Data)")
    print("=" * 80)
    print(f"\nSource: {PG_CONFIG['host']}/{PG_CONFIG['database']}")
    print(f"Target: {MSSQL_CONFIG['server']}/{MSSQL_CONFIG['database']}\n")

    print(f"Tables to copy: {len(TABLES_TO_COPY)}\n")

    pg_conn = get_pg_connection()
    mssql_conn = get_mssql_connection()
    print("✓ Connected to both databases\n")

    print("=" * 80)
    print("Copying data...")
    print("=" * 80 + "\n")

    results = []

    for i, table in enumerate(TABLES_TO_COPY, 1):
        print(f"[{i}/{len(TABLES_TO_COPY)}] {table}...")

        total, inserted, status = copy_table_data(pg_conn, mssql_conn, table)
        results.append((table, total, inserted, status))

        if inserted > 0:
            print(f"  ✓ {status}\n")
        else:
            print(f"  ⚠ {status}\n")

    pg_conn.close()
    mssql_conn.close()

    # Summary
    print("=" * 80)
    print("MIGRATION SUMMARY")
    print("=" * 80)

    total_rows = sum(r[1] for r in results)
    inserted_rows = sum(r[2] for r in results)

    print(f"Tables processed: {len(results)}")
    print(f"Total rows found: {total_rows:,}")
    print(f"Total rows inserted: {inserted_rows:,}")

    print("\nDetails:")
    print(f"{'Table':<40} {'Status':<40}")
    print("-" * 80)

    for table, total, inserted, status in results:
        print(f"{table:<40} {status:<40}")

    print("=" * 80)

if __name__ == "__main__":
    main()
