import pyodbc

MSSQL_CONFIG = {
    'server': '10.20.53.175',
    'database': 'LBSSFADev',
    'user': 'lbssfadev',
    'password': 'lbssfadev'
}

def get_mssql_connection():
    conn_str = (
        f"DRIVER={{ODBC Driver 17 for SQL Server}};"
        f"SERVER={MSSQL_CONFIG['server']};"
        f"DATABASE={MSSQL_CONFIG['database']};"
        f"UID={MSSQL_CONFIG['user']};"
        f"PWD={MSSQL_CONFIG['password']};"
        f"TrustServerCertificate=yes;"
    )
    return pyodbc.connect(conn_str)

def execute_sql_file(sql_file):
    print(f"Reading SQL file: {sql_file}")
    with open(sql_file, 'r', encoding='utf-8') as f:
        sql_content = f.read()

    # Split by GO statements
    sql_batches = [batch.strip() for batch in sql_content.split('GO') if batch.strip()]

    print(f"Connecting to MSSQL: {MSSQL_CONFIG['server']}")
    conn = get_mssql_connection()
    cursor = conn.cursor()

    success_count = 0
    error_count = 0

    for i, batch in enumerate(sql_batches, 1):
        if not batch or batch.startswith('--'):
            continue

        try:
            print(f"Executing batch {i}/{len(sql_batches)}...")
            cursor.execute(batch)
            conn.commit()
            success_count += 1
        except Exception as e:
            error_count += 1
            print(f"Error in batch {i}: {str(e)}")

    cursor.close()
    conn.close()

    print(f"\n✓ Execution complete!")
    print(f"✓ Successful: {success_count}")
    print(f"✗ Errors: {error_count}")

if __name__ == "__main__":
    sql_file = '/Users/suryakantkumar/Desktop/Multiplex/backend/python-tools/mssql_master_tables.sql'
    execute_sql_file(sql_file)
