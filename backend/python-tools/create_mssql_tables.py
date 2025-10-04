import pymssql

MSSQL_CONFIG = {
    'server': '10.20.53.175',
    'database': 'LBSSFADev',
    'user': 'lbssfadev',
    'password': 'lbssfadev'
}

def execute_sql_file(sql_file):
    print(f"Reading SQL file: {sql_file}")
    with open(sql_file, 'r', encoding='utf-8') as f:
        sql_content = f.read()

    # Split by GO statements (case insensitive)
    import re
    sql_batches = re.split(r'\bGO\b', sql_content, flags=re.IGNORECASE)
    sql_batches = [batch.strip() for batch in sql_batches if batch.strip() and len(batch.strip()) > 10]

    print(f"\nConnecting to MSSQL: {MSSQL_CONFIG['server']}")
    print(f"Database: {MSSQL_CONFIG['database']}\n")

    conn = pymssql.connect(
        server=MSSQL_CONFIG['server'],
        user=MSSQL_CONFIG['user'],
        password=MSSQL_CONFIG['password'],
        database=MSSQL_CONFIG['database']
    )

    print("✓ Connected successfully!\n")

    cursor = conn.cursor()
    success_count = 0
    error_count = 0

    for i, batch in enumerate(sql_batches, 1):
        if not batch or len(batch) < 10:
            continue

        try:
            print(f"Executing batch {i}/{len(sql_batches)}...", end=' ')
            cursor.execute(batch)
            conn.commit()
            success_count += 1
            print("✓")
        except Exception as e:
            error_count += 1
            print(f"✗ Error: {str(e)[:100]}")

    cursor.close()
    conn.close()

    print(f"\n{'='*50}")
    print(f"✓ Execution complete!")
    print(f"✓ Successful batches: {success_count}")
    print(f"✗ Failed batches: {error_count}")
    print(f"{'='*50}")

    # Verify tables created
    print("\nVerifying tables...")
    conn = pymssql.connect(
        server=MSSQL_CONFIG['server'],
        user=MSSQL_CONFIG['user'],
        password=MSSQL_CONFIG['password'],
        database=MSSQL_CONFIG['database']
    )
    cursor = conn.cursor()
    cursor.execute("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME")
    tables = cursor.fetchall()

    if tables:
        print(f"\n✓ {len(tables)} tables created successfully:")
        for i, (table_name,) in enumerate(tables, 1):
            print(f"  {i}. {table_name}")
    else:
        print("\n✗ No tables found after execution.")

    cursor.close()
    conn.close()

if __name__ == "__main__":
    sql_file = '/Users/suryakantkumar/Desktop/Multiplex/backend/python-tools/mssql_master_tables.sql'
    execute_sql_file(sql_file)
