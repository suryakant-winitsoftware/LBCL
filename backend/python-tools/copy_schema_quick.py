import psycopg2
from typing import List, Dict, Any
import sys

# PostgreSQL connection
PG_CONFIG = {
    'host': '10.20.53.130',
    'port': 5432,
    'database': 'multiplexdev170725FM',
    'user': 'multiplex',
    'password': 'multiplex'
}

def get_pg_connection():
    """Create PostgreSQL connection"""
    return psycopg2.connect(
        host=PG_CONFIG['host'],
        port=PG_CONFIG['port'],
        database=PG_CONFIG['database'],
        user=PG_CONFIG['user'],
        password=PG_CONFIG['password']
    )

def map_pg_type_to_mssql(pg_type: str, char_max_length: int = None, numeric_precision: int = None, numeric_scale: int = None) -> str:
    """Map PostgreSQL data types to MSSQL data types"""
    pg_type = pg_type.lower()

    type_mapping = {
        'integer': 'INT',
        'int': 'INT',
        'int4': 'INT',
        'bigint': 'BIGINT',
        'int8': 'BIGINT',
        'smallint': 'SMALLINT',
        'int2': 'SMALLINT',
        'serial': 'INT IDENTITY(1,1)',
        'bigserial': 'BIGINT IDENTITY(1,1)',
        'smallserial': 'SMALLINT IDENTITY(1,1)',
        'boolean': 'BIT',
        'bool': 'BIT',
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
        'real': 'REAL',
        'float4': 'REAL',
        'double precision': 'FLOAT',
        'float8': 'FLOAT',
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

def list_tables(pg_conn, filter_pattern=None):
    """List all tables"""
    cursor = pg_conn.cursor()
    if filter_pattern:
        cursor.execute("""
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
            AND table_type = 'BASE TABLE'
            AND table_name LIKE %s
            ORDER BY table_name
        """, (filter_pattern,))
    else:
        cursor.execute("""
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
            AND table_type = 'BASE TABLE'
            AND table_name NOT LIKE 'app_request_%'
            AND table_name NOT LIKE '_store_%'
            ORDER BY table_name
        """)
    tables = [row[0] for row in cursor.fetchall()]
    cursor.close()
    return tables

def get_table_columns(pg_conn, table_name: str) -> List[Dict[str, Any]]:
    """Get column definitions for a table"""
    cursor = pg_conn.cursor()
    cursor.execute("""
        SELECT
            column_name,
            data_type,
            character_maximum_length,
            numeric_precision,
            numeric_scale,
            is_nullable,
            column_default
        FROM information_schema.columns
        WHERE table_schema = 'public'
        AND table_name = %s
        ORDER BY ordinal_position
    """, (table_name,))

    columns = []
    for row in cursor.fetchall():
        columns.append({
            'name': row[0],
            'type': row[1],
            'max_length': row[2],
            'precision': row[3],
            'scale': row[4],
            'nullable': row[5] == 'YES',
            'default': row[6]
        })
    cursor.close()
    return columns

def get_primary_keys(pg_conn, table_name: str) -> List[str]:
    """Get primary key columns"""
    cursor = pg_conn.cursor()
    cursor.execute("""
        SELECT kcu.column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
            ON tc.constraint_name = kcu.constraint_name
            AND tc.table_schema = kcu.table_schema
        WHERE tc.constraint_type = 'PRIMARY KEY'
            AND tc.table_schema = 'public'
            AND tc.table_name = %s
        ORDER BY kcu.ordinal_position
    """, (table_name,))

    pk_columns = [row[0] for row in cursor.fetchall()]
    cursor.close()
    return pk_columns

def generate_mssql_create_table(table_name: str, columns: List[Dict], pk_columns: List[str]) -> str:
    """Generate MSSQL CREATE TABLE statement"""
    sql = f"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{table_name}]') AND type in (N'U'))\n"
    sql += f"BEGIN\n"
    sql += f"CREATE TABLE [dbo].[{table_name}] (\n"

    column_defs = []
    for col in columns:
        mssql_type = map_pg_type_to_mssql(
            col['type'],
            col['max_length'],
            col['precision'],
            col['scale']
        )

        col_def = f"    [{col['name']}] {mssql_type}"

        if not col['nullable']:
            col_def += " NOT NULL"
        else:
            col_def += " NULL"

        if col['default'] and 'nextval' not in str(col['default']):
            default_val = str(col['default']).replace("::character varying", "").replace("::text", "")
            if default_val not in ['NULL', 'null']:
                col_def += f" DEFAULT {default_val}"

        column_defs.append(col_def)

    sql += ",\n".join(column_defs)

    if pk_columns:
        pk_cols = ", ".join([f"[{col}]" for col in pk_columns])
        sql += f",\n    CONSTRAINT [PK_{table_name}] PRIMARY KEY CLUSTERED ({pk_cols})"

    sql += "\n);\nEND\nGO\n"
    return sql

def main():
    print("Connecting to PostgreSQL...")
    pg_conn = get_pg_connection()

    # List core tables (excluding app_request and temp tables)
    print("\nFetching core tables...")
    tables = list_tables(pg_conn)

    print(f"\nFound {len(tables)} core tables")
    print("\nFirst 50 tables:")
    for i, table in enumerate(tables[:50], 1):
        print(f"{i}. {table}")

    output_file = '/Users/suryakantkumar/Desktop/Multiplex/backend/python-tools/mssql_schema.sql'

    print(f"\nGenerating MSSQL schema to: {output_file}")
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write("-- MSSQL Schema Migration from PostgreSQL\n")
        f.write("-- Core tables only (excludes app_request_* and temp tables)\n")
        f.write("-- Tables structure only, no data\n")
        f.write("-- Database: LBSSFADev\n\n")
        f.write("USE [LBSSFADev];\nGO\n\n")

        for i, table in enumerate(tables, 1):
            print(f"Processing {i}/{len(tables)}: {table}")

            columns = get_table_columns(pg_conn, table)
            pk_columns = get_primary_keys(pg_conn, table)

            create_sql = generate_mssql_create_table(table, columns, pk_columns)
            f.write(f"-- Table: {table}\n")
            f.write(create_sql)
            f.write("\n")

    pg_conn.close()
    print(f"\n✓ Schema exported successfully!")
    print(f"✓ Total tables: {len(tables)}")
    print(f"✓ Output file: {output_file}")
    print("\nTo execute on MSSQL:")
    print("1. Open SQL Server Management Studio")
    print("2. Connect to: 10.20.53.175")
    print("3. Open the generated .sql file")
    print("4. Execute the script")

if __name__ == "__main__":
    main()
