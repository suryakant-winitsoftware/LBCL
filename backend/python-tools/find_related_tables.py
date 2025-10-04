import psycopg2

PG_CONFIG = {
    'host': '10.20.53.130',
    'port': 5432,
    'database': 'multiplexdev170725FM',
    'user': 'multiplex',
    'password': 'multiplex'
}

def get_pg_connection():
    return psycopg2.connect(
        host=PG_CONFIG['host'],
        port=PG_CONFIG['port'],
        database=PG_CONFIG['database'],
        user=PG_CONFIG['user'],
        password=PG_CONFIG['password']
    )

def search_tables(conn, patterns):
    """Search for tables matching patterns"""
    cursor = conn.cursor()

    all_tables = set()

    for pattern in patterns:
        cursor.execute("""
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
            AND table_type = 'BASE TABLE'
            AND table_name ILIKE %s
            ORDER BY table_name
        """, (f'%{pattern}%',))

        tables = [row[0] for row in cursor.fetchall()]
        if tables:
            print(f"\n  Pattern '{pattern}': Found {len(tables)} tables")
            for table in tables[:10]:  # Show first 10
                print(f"    - {table}")
            if len(tables) > 10:
                print(f"    ... and {len(tables) - 10} more")
            all_tables.update(tables)

    cursor.close()
    return list(all_tables)

def get_table_info(conn, table_name):
    """Get table structure and row count"""
    cursor = conn.cursor()

    # Get row count
    try:
        cursor.execute(f"SELECT COUNT(*) FROM {table_name}")
        count = cursor.fetchone()[0]
    except Exception as e:
        count = 0
        conn.rollback()  # Rollback failed transaction

    # Get columns
    try:
        cursor.execute("""
            SELECT column_name, data_type, is_nullable
            FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = %s
            ORDER BY ordinal_position
            LIMIT 10
        """, (table_name,))
        columns = cursor.fetchall()
    except Exception as e:
        columns = []
        conn.rollback()

    cursor.close()

    return count, columns

def main():
    print("=" * 80)
    print("Finding Tables Related to:")
    print("  1. /administration/team-management/employees")
    print("  2. /administration/access-control/permission-matrix")
    print("=" * 80)

    conn = get_pg_connection()
    print("\n✓ Connected to PostgreSQL\n")

    # Search patterns for employees/team management
    print("\n" + "=" * 80)
    print("SEARCHING: Employee/Team Management Tables")
    print("=" * 80)

    emp_patterns = [
        'employee', 'emp', 'staff', 'user', 'team',
        'designation', 'department', 'position', 'job'
    ]

    emp_tables = search_tables(conn, emp_patterns)

    # Search patterns for permissions/access control
    print("\n" + "=" * 80)
    print("SEARCHING: Permission/Access Control Tables")
    print("=" * 80)

    perm_patterns = [
        'permission', 'role', 'access', 'privilege',
        'menu', 'module', 'sub_module', 'auth', 'acl'
    ]

    perm_tables = search_tables(conn, perm_patterns)

    # Get all unique tables
    all_tables = list(set(emp_tables + perm_tables))

    print("\n" + "=" * 80)
    print(f"FOUND {len(all_tables)} UNIQUE TABLES")
    print("=" * 80)

    # Show details for key tables
    print("\nTable Details (with row counts and structure):\n")

    for table in sorted(all_tables):
        count, columns = get_table_info(conn, table)
        print(f"\n📊 {table}")
        print(f"   Rows: {count:,}")
        if columns:
            print(f"   Columns ({len(columns)} shown):")
            for col_name, col_type, nullable in columns:
                null_str = "NULL" if nullable == "YES" else "NOT NULL"
                print(f"     - {col_name}: {col_type} {null_str}")

    conn.close()

    print("\n" + "=" * 80)
    print("SUMMARY")
    print("=" * 80)
    print(f"Total tables found: {len(all_tables)}")
    print("\nTable List:")
    for i, table in enumerate(sorted(all_tables), 1):
        print(f"  {i}. {table}")

if __name__ == "__main__":
    main()
