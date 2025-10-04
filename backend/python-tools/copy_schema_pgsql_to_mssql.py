import psycopg2
import pyodbc
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
    """Create PostgreSQL connection"""
    return psycopg2.connect(
        host=PG_CONFIG['host'],
        port=PG_CONFIG['port'],
        database=PG_CONFIG['database'],
        user=PG_CONFIG['user'],
        password=PG_CONFIG['password']
    )

def get_mssql_connection():
    """Create MSSQL connection"""
    conn_str = (
        f"DRIVER={{ODBC Driver 17 for SQL Server}};"
        f"SERVER={MSSQL_CONFIG['server']};"
        f"DATABASE={MSSQL_CONFIG['database']};"
        f"UID={MSSQL_CONFIG['user']};"
        f"PWD={MSSQL_CONFIG['password']};"
        f"TrustServerCertificate=yes;"
    )
    return pyodbc.connect(conn_str)

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
        'array': 'NVARCHAR(MAX)',  # Arrays need special handling
    }

    return type_mapping.get(pg_type, 'NVARCHAR(MAX)')

def get_pg_tables(pg_conn) -> List[str]:
    """Get all table names from PostgreSQL"""
    cursor = pg_conn.cursor()
    cursor.execute("""
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
        AND table_type = 'BASE TABLE'
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

def get_foreign_keys(pg_conn, table_name: str) -> List[Dict[str, Any]]:
    """Get foreign key constraints"""
    cursor = pg_conn.cursor()
    cursor.execute("""
        SELECT
            tc.constraint_name,
            kcu.column_name,
            ccu.table_name AS foreign_table_name,
            ccu.column_name AS foreign_column_name
        FROM information_schema.table_constraints AS tc
        JOIN information_schema.key_column_usage AS kcu
            ON tc.constraint_name = kcu.constraint_name
            AND tc.table_schema = kcu.table_schema
        JOIN information_schema.constraint_column_usage AS ccu
            ON ccu.constraint_name = tc.constraint_name
            AND ccu.table_schema = tc.table_schema
        WHERE tc.constraint_type = 'FOREIGN KEY'
        AND tc.table_name = %s
    """, (table_name,))

    fks = []
    for row in cursor.fetchall():
        fks.append({
            'constraint_name': row[0],
            'column': row[1],
            'foreign_table': row[2],
            'foreign_column': row[3]
        })
    cursor.close()
    return fks

def generate_mssql_create_table(table_name: str, columns: List[Dict], pk_columns: List[str]) -> str:
    """Generate MSSQL CREATE TABLE statement"""
    sql = f"CREATE TABLE [{table_name}] (\n"

    column_defs = []
    for col in columns:
        mssql_type = map_pg_type_to_mssql(
            col['type'],
            col['max_length'],
            col['precision'],
            col['scale']
        )

        col_def = f"    [{col['name']}] {mssql_type}"

        # Handle nullable
        if not col['nullable']:
            col_def += " NOT NULL"
        else:
            col_def += " NULL"

        # Handle default values (skip sequences/serials as they're handled in type mapping)
        if col['default'] and 'nextval' not in str(col['default']):
            default_val = str(col['default']).replace("::character varying", "").replace("::text", "")
            if default_val not in ['NULL', 'null']:
                col_def += f" DEFAULT {default_val}"

        column_defs.append(col_def)

    sql += ",\n".join(column_defs)

    # Add primary key
    if pk_columns:
        pk_cols = ", ".join([f"[{col}]" for col in pk_columns])
        sql += f",\n    CONSTRAINT [PK_{table_name}] PRIMARY KEY CLUSTERED ({pk_cols})"

    sql += "\n);\n"
    return sql

def generate_mssql_foreign_keys(table_name: str, fks: List[Dict]) -> str:
    """Generate MSSQL ALTER TABLE statements for foreign keys"""
    sql = ""
    for fk in fks:
        sql += f"""ALTER TABLE [{table_name}]
    ADD CONSTRAINT [FK_{table_name}_{fk['column']}]
    FOREIGN KEY ([{fk['column']}])
    REFERENCES [{fk['foreign_table']}] ([{fk['foreign_column']}]);\n"""
    return sql

def main():
    print("Connecting to PostgreSQL...")
    pg_conn = get_pg_connection()

    print("Getting table list from PostgreSQL...")
    tables = get_pg_tables(pg_conn)
    print(f"Found {len(tables)} tables")

    # Generate DDL script
    output_file = '/Users/suryakantkumar/Desktop/Multiplex/backend/python-tools/mssql_schema.sql'

    with open(output_file, 'w') as f:
        f.write("-- MSSQL Schema Migration from PostgreSQL\n")
        f.write(f"-- Generated for database: {MSSQL_CONFIG['database']}\n")
        f.write("-- Tables only (no data)\n\n")

        # First pass: Create tables
        f.write("-- ============================================\n")
        f.write("-- CREATE TABLES\n")
        f.write("-- ============================================\n\n")

        all_fks = []

        for table in tables:
            print(f"Processing table: {table}")
            columns = get_table_columns(pg_conn, table)
            pk_columns = get_primary_keys(pg_conn, table)
            fks = get_foreign_keys(pg_conn, table)

            # Store FKs for later
            if fks:
                all_fks.extend([(table, fk) for fk in fks])

            # Generate CREATE TABLE
            create_sql = generate_mssql_create_table(table, columns, pk_columns)
            f.write(f"-- Table: {table}\n")
            f.write(create_sql)
            f.write("\n")

        # Second pass: Add foreign keys
        if all_fks:
            f.write("\n-- ============================================\n")
            f.write("-- ADD FOREIGN KEYS\n")
            f.write("-- ============================================\n\n")

            for table, fk in all_fks:
                fk_sql = generate_mssql_foreign_keys(table, [fk])
                f.write(fk_sql)

    pg_conn.close()
    print(f"\n✓ Schema exported to: {output_file}")
    print(f"✓ Total tables: {len(tables)}")
    print("\nNext steps:")
    print("1. Review the generated SQL file")
    print("2. Connect to MSSQL and execute the script")
    print("3. Or use the companion script to execute automatically")

if __name__ == "__main__":
    main()
