import psycopg2
import pymssql
from typing import List, Dict, Any
import re

# PostgreSQL connection
PG_CONFIG = {
    'host': '10.20.53.130',
    'port': 5432,
    'database': 'multiplexdev170725FM',
    'user': 'multiplex',
    'password': 'multiplex'
}

# MSSQL connection
MSSQL_CONFIG = {
    'server': '10.20.53.175',
    'database': 'LBSSFADev',
    'user': 'lbssfadev',
    'password': 'lbssfadev'
}

def get_pg_connection():
    return psycopg2.connect(
        host=PG_CONFIG['host'],
        port=PG_CONFIG['port'],
        database=PG_CONFIG['database'],
        user=PG_CONFIG['user'],
        password=PG_CONFIG['password']
    )

def get_mssql_connection():
    return pymssql.connect(
        server=MSSQL_CONFIG['server'],
        user=MSSQL_CONFIG['user'],
        password=MSSQL_CONFIG['password'],
        database=MSSQL_CONFIG['database']
    )

def map_pg_type_to_mssql(pg_type: str, char_max_length: int = None, numeric_precision: int = None, numeric_scale: int = None) -> str:
    pg_type = pg_type.lower()
    type_mapping = {
        'integer': 'INT', 'int': 'INT', 'int4': 'INT',
        'bigint': 'BIGINT', 'int8': 'BIGINT',
        'smallint': 'SMALLINT', 'int2': 'SMALLINT',
        'serial': 'INT IDENTITY(1,1)',
        'bigserial': 'BIGINT IDENTITY(1,1)',
        'smallserial': 'SMALLINT IDENTITY(1,1)',
        'boolean': 'BIT', 'bool': 'BIT',
        'text': 'NVARCHAR(MAX)',
        'varchar': f'NVARCHAR({char_max_length if char_max_length else "MAX"})',
        'character varying': f'NVARCHAR({char_max_length if char_max_length else "MAX"})',
        'char': f'NCHAR({char_max_length if char_max_length else 1})',
        'character': f'NCHAR({char_max_length if char_max_length else 1})',
        'date': 'DATE',
        'timestamp': 'DATETIME2',
        'timestamp without time zone': 'DATETIME2',
        'timestamp with time zone': 'DATETIMEOFFSET',
        'timestamptz': 'DATETIMEOFFSET',
        'time': 'TIME',
        'time without time zone': 'TIME',
        'real': 'REAL', 'float4': 'REAL',
        'double precision': 'FLOAT', 'float8': 'FLOAT',
        'numeric': f'DECIMAL({numeric_precision if numeric_precision else 18},{numeric_scale if numeric_scale else 0})',
        'decimal': f'DECIMAL({numeric_precision if numeric_precision else 18},{numeric_scale if numeric_scale else 0})',
        'money': 'MONEY',
        'uuid': 'UNIQUEIDENTIFIER',
        'json': 'NVARCHAR(MAX)',
        'jsonb': 'NVARCHAR(MAX)',
        'xml': 'XML',
        'bytea': 'VARBINARY(MAX)',
        'array': 'NVARCHAR(MAX)',
    }
    return type_mapping.get(pg_type, 'NVARCHAR(MAX)')

def list_all_tables(pg_conn):
    cursor = pg_conn.cursor()
    cursor.execute("""
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
        AND table_type = 'BASE TABLE'
        AND table_name NOT LIKE 'app_request_%'
        AND table_name NOT LIKE '_store_%'
        AND table_name NOT LIKE 'beat_history_%'
        ORDER BY table_name
    """)
    tables = [row[0] for row in cursor.fetchall()]
    cursor.close()
    return tables

def get_table_columns(pg_conn, table_name: str) -> List[Dict[str, Any]]:
    cursor = pg_conn.cursor()
    cursor.execute("""
        SELECT column_name, data_type, character_maximum_length,
               numeric_precision, numeric_scale, is_nullable, column_default
        FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = %s
        ORDER BY ordinal_position
    """, (table_name,))

    columns = []
    for row in cursor.fetchall():
        # Clean up default values
        default = row[6]
        if default:
            # Remove PostgreSQL-specific defaults
            if 'nextval' in str(default) or 'now()' in str(default):
                default = None
            elif 'false' in str(default).lower():
                default = '0'
            elif 'true' in str(default).lower():
                default = '1'
            else:
                default = str(default).replace("::character varying", "").replace("::text", "").replace("::integer", "")

        columns.append({
            'name': row[0], 'type': row[1], 'max_length': row[2],
            'precision': row[3], 'scale': row[4],
            'nullable': row[5] == 'YES', 'default': default
        })
    cursor.close()
    return columns

def get_primary_keys(pg_conn, table_name: str) -> List[str]:
    cursor = pg_conn.cursor()
    cursor.execute("""
        SELECT kcu.column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
            ON tc.constraint_name = kcu.constraint_name
            AND tc.table_schema = kcu.table_schema
        WHERE tc.constraint_type = 'PRIMARY KEY'
            AND tc.table_schema = 'public' AND tc.table_name = %s
        ORDER BY kcu.ordinal_position
    """, (table_name,))
    pk_columns = [row[0] for row in cursor.fetchall()]
    cursor.close()
    return pk_columns

def create_mssql_table(mssql_conn, table_name: str, columns: List[Dict], pk_columns: List[str]):
    cursor = mssql_conn.cursor()

    # Check if table exists
    cursor.execute(f"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table_name}'")
    if cursor.fetchone()[0] > 0:
        print(f"  Table already exists, skipping...")
        cursor.close()
        return True

    # Build CREATE TABLE statement
    sql = f"CREATE TABLE [dbo].[{table_name}] (\n"

    column_defs = []
    for col in columns:
        mssql_type = map_pg_type_to_mssql(col['type'], col['max_length'], col['precision'], col['scale'])
        col_def = f"    [{col['name']}] {mssql_type}"
        col_def += " NOT NULL" if not col['nullable'] else " NULL"

        if col['default'] and col['default'] not in ['NULL', 'null']:
            col_def += f" DEFAULT {col['default']}"

        column_defs.append(col_def)

    sql += ",\n".join(column_defs)

    if pk_columns:
        pk_cols = ", ".join([f"[{col}]" for col in pk_columns])
        sql += f",\n    CONSTRAINT [PK_{table_name}] PRIMARY KEY CLUSTERED ({pk_cols})"

    sql += "\n);"

    try:
        cursor.execute(sql)
        mssql_conn.commit()
        cursor.close()
        return True
    except Exception as e:
        print(f"  ✗ Error: {str(e)[:150]}")
        cursor.close()
        return False

def main():
    print("=" * 70)
    print("PostgreSQL to MSSQL Table Structure Migration")
    print("=" * 70)
    print(f"\nSource: PostgreSQL - {PG_CONFIG['host']}:{PG_CONFIG['port']}/{PG_CONFIG['database']}")
    print(f"Target: MSSQL - {MSSQL_CONFIG['server']}/{MSSQL_CONFIG['database']}\n")

    print("Connecting to PostgreSQL...")
    pg_conn = get_pg_connection()
    print("✓ Connected to PostgreSQL\n")

    print("Connecting to MSSQL...")
    mssql_conn = get_mssql_connection()
    print("✓ Connected to MSSQL\n")

    print("Fetching table list from PostgreSQL...")
    tables = list_all_tables(pg_conn)
    print(f"✓ Found {len(tables)} tables to migrate\n")

    print("=" * 70)
    print("Starting table creation...")
    print("=" * 70)

    success_count = 0
    error_count = 0
    skipped_count = 0

    for i, table in enumerate(tables, 1):
        print(f"\n[{i}/{len(tables)}] Processing: {table}")

        try:
            columns = get_table_columns(pg_conn, table)
            pk_columns = get_primary_keys(pg_conn, table)

            if create_mssql_table(mssql_conn, table, columns, pk_columns):
                success_count += 1
                print(f"  ✓ Created successfully")
            else:
                error_count += 1

        except Exception as e:
            error_count += 1
            print(f"  ✗ Error: {str(e)[:150]}")

    pg_conn.close()
    mssql_conn.close()

    print("\n" + "=" * 70)
    print("MIGRATION SUMMARY")
    print("=" * 70)
    print(f"Total tables processed: {len(tables)}")
    print(f"✓ Successfully created: {success_count}")
    print(f"✗ Failed: {error_count}")
    print("=" * 70)

    # Verify final count
    print("\nVerifying tables in MSSQL...")
    mssql_conn = get_mssql_connection()
    cursor = mssql_conn.cursor()
    cursor.execute("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'")
    total = cursor.fetchone()[0]
    print(f"✓ Total tables in MSSQL: {total}")
    cursor.close()
    mssql_conn.close()

if __name__ == "__main__":
    main()
