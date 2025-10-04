import pymssql

MSSQL_CONFIG = {
    'server': '10.20.53.175',
    'database': 'LBSSFADev',
    'user': 'lbssfadev',
    'password': 'lbssfadev'
}

def main():
    print(f"Connecting to MSSQL Server: {MSSQL_CONFIG['server']}")
    print(f"Database: {MSSQL_CONFIG['database']}\n")

    try:
        conn = pymssql.connect(
            server=MSSQL_CONFIG['server'],
            user=MSSQL_CONFIG['user'],
            password=MSSQL_CONFIG['password'],
            database=MSSQL_CONFIG['database']
        )

        print("✓ Connected successfully!\n")

        cursor = conn.cursor()

        # Get all tables
        print("Fetching table list...")
        cursor.execute("""
            SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_NAME
        """)

        tables = cursor.fetchall()

        if tables:
            print(f"\n✓ Found {len(tables)} tables in database '{MSSQL_CONFIG['database']}':\n")
            for i, (schema, table_name, table_type) in enumerate(tables, 1):
                print(f"{i}. [{schema}].[{table_name}]")
        else:
            print(f"\n✗ No tables found in database '{MSSQL_CONFIG['database']}'")
            print("The database is EMPTY - you need to execute the schema creation script.")

        # Get database info
        cursor.execute("SELECT @@VERSION")
        version = cursor.fetchone()[0]
        print(f"\nSQL Server Version:")
        print(version[:200])

        cursor.close()
        conn.close()

    except Exception as e:
        print(f"\n✗ Connection Error: {str(e)}")

if __name__ == "__main__":
    main()
