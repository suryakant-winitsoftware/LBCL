import psycopg2
from typing import List, Dict, Any

# PostgreSQL connection
PG_CONFIG = {
    'host': '10.20.53.130',
    'port': 5432,
    'database': 'multiplexdev170725FM',
    'user': 'multiplex',
    'password': 'multiplex'
}

# List of essential master tables to copy
MASTER_TABLES = [
    'address', 'bank', 'brand', 'city', 'country', 'currency', 'customer',
    'customer_classification', 'customer_type', 'department', 'designation',
    'division', 'document_type', 'geomaster', 'lov', 'menu', 'organization',
    'paymentterm', 'region', 'role', 'sku', 'sku_classification', 'sku_group',
    'state', 'store', 'store_classification', 'store_type', 'sub_division',
    'tax', 'territory', 'uom', 'user', 'usermap', 'vehicle', 'warehouse',
    'zone', 'route', 'beat', 'scheme', 'promotion', 'planogram'
]

def get_pg_connection():
    return psycopg2.connect(
        host=PG_CONFIG['host'],
        port=PG_CONFIG['port'],
        database=PG_CONFIG['database'],
        user=PG_CONFIG['user'],
        password=PG_CONFIG['password']
    )

def map_pg_type_to_mssql(pg_type: str, char_max_length: int = None, numeric_precision: int = None, numeric_scale: int = None) -> str:
    pg_type = pg_type.lower()
    type_mapping = {
        'integer': 'INT', 'int': 'INT', 'int4': 'INT',
        'bigint': 'BIGINT', 'int8': 'BIGINT',
        'smallint': 'SMALLINT', 'int2': 'SMALLINT',
        'serial': 'INT IDENTITY(1,1)',
        'bigserial': 'BIGINT IDENTITY(1,1)',
        'boolean': 'BIT', 'bool': 'BIT',
        'text': 'NVARCHAR(MAX)',
        'varchar': f'NVARCHAR({char_max_length if char_max_length else "MAX"})',
        'character varying': f'NVARCHAR({char_max_length if char_max_length else "MAX"})',
        'char': f'NCHAR({char_max_length if char_max_length else 1})',
        'date': 'DATE',
        'timestamp': 'DATETIME2',
        'timestamp without time zone': 'DATETIME2',
        'timestamp with time zone': 'DATETIMEOFFSET',
        'numeric': f'DECIMAL({numeric_precision if numeric_precision else 18},{numeric_scale if numeric_scale else 0})',
        'decimal': f'DECIMAL({numeric_precision if numeric_precision else 18},{numeric_scale if numeric_scale else 0})',
        'uuid': 'UNIQUEIDENTIFIER',
        'json': 'NVARCHAR(MAX)',
        'jsonb': 'NVARCHAR(MAX)',
        'bytea': 'VARBINARY(MAX)',
    }
    return type_mapping.get(pg_type, 'NVARCHAR(MAX)')

def table_exists(pg_conn, table_name: str) -> bool:
    cursor = pg_conn.cursor()
    cursor.execute("""
        SELECT COUNT(*) FROM information_schema.tables
        WHERE table_schema = 'public' AND table_name = %s
    """, (table_name,))
    exists = cursor.fetchone()[0] > 0
    cursor.close()
    return exists

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
        columns.append({
            'name': row[0], 'type': row[1], 'max_length': row[2],
            'precision': row[3], 'scale': row[4],
            'nullable': row[5] == 'YES', 'default': row[6]
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

def generate_mssql_create_table(table_name: str, columns: List[Dict], pk_columns: List[str]) -> str:
    sql = f"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{table_name}]') AND type in (N'U'))\n"
    sql += f"BEGIN\n    CREATE TABLE [dbo].[{table_name}] (\n"

    column_defs = []
    for col in columns:
        mssql_type = map_pg_type_to_mssql(col['type'], col['max_length'], col['precision'], col['scale'])
        col_def = f"        [{col['name']}] {mssql_type}"
        col_def += " NOT NULL" if not col['nullable'] else " NULL"

        if col['default'] and 'nextval' not in str(col['default']):
            default_val = str(col['default']).replace("::character varying", "").replace("::text", "")
            if default_val not in ['NULL', 'null']:
                col_def += f" DEFAULT {default_val}"

        column_defs.append(col_def)

    sql += ",\n".join(column_defs)

    if pk_columns:
        pk_cols = ", ".join([f"[{col}]" for col in pk_columns])
        sql += f",\n        CONSTRAINT [PK_{table_name}] PRIMARY KEY CLUSTERED ({pk_cols})"

    sql += "\n    );\nEND\nGO\n"
    return sql

def main():
    print("Connecting to PostgreSQL...")
    pg_conn = get_pg_connection()

    existing_tables = []
    missing_tables = []

    for table in MASTER_TABLES:
        if table_exists(pg_conn, table):
            existing_tables.append(table)
        else:
            missing_tables.append(table)

    print(f"\nFound {len(existing_tables)} tables")
    if missing_tables:
        print(f"Missing {len(missing_tables)} tables: {', '.join(missing_tables)}")

    output_file = '/Users/suryakantkumar/Desktop/Multiplex/backend/python-tools/mssql_master_tables.sql'

    with open(output_file, 'w', encoding='utf-8') as f:
        f.write("-- MSSQL Master Tables Migration\n")
        f.write(f"-- Database: LBSSFADev\n")
        f.write("-- Tables structure only (no data)\n\n")
        f.write("USE [LBSSFADev];\nGO\n\n")

        for i, table in enumerate(existing_tables, 1):
            print(f"Processing {i}/{len(existing_tables)}: {table}")
            columns = get_table_columns(pg_conn, table)
            pk_columns = get_primary_keys(pg_conn, table)
            create_sql = generate_mssql_create_table(table, columns, pk_columns)
            f.write(f"-- Table: {table}\n{create_sql}\n")

    pg_conn.close()
    print(f"\n✓ Schema exported to: {output_file}")
    print(f"✓ Total tables: {len(existing_tables)}")

if __name__ == "__main__":
    main()
