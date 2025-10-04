import psycopg2
import pymssql
from typing import List, Dict, Any

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

TABLES_TO_CREATE = ['sub_modules', 'sub_sub_modules', 'user_role']

def get_pg_connection():
    return psycopg2.connect(**PG_CONFIG)

def get_mssql_connection():
    return pymssql.connect(**MSSQL_CONFIG)

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
        default = row[6]
        if default:
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

def create_mssql_table(mssql_conn, table_name: str, columns: List[Dict], pk_columns: List[str]) -> bool:
    cursor = mssql_conn.cursor()

    # Check if exists
    cursor.execute(f"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table_name}'")
    if cursor.fetchone()[0] > 0:
        print(f"  Table already exists")
        cursor.close()
        return True

    # Build CREATE TABLE
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

def copy_table_data(pg_conn, mssql_conn, table_name: str) -> int:
    try:
        pg_cursor = pg_conn.cursor()
        mssql_cursor = mssql_conn.cursor()

        # Get column names
        pg_cursor.execute("""
            SELECT column_name
            FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = %s
            ORDER BY ordinal_position
        """, (table_name,))
        columns = [row[0] for row in pg_cursor.fetchall()]

        if not columns:
            return 0

        # Fetch data
        pg_cursor.execute(f"SELECT {', '.join(columns)} FROM {table_name}")
        rows = pg_cursor.fetchall()

        if not rows:
            return 0

        # Insert data
        placeholders = ', '.join(['%s'] * len(columns))
        insert_sql = f"INSERT INTO [{table_name}] ({', '.join([f'[{col}]' for col in columns])}) VALUES ({placeholders})"

        inserted = 0
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
                pass

        mssql_conn.commit()
        pg_cursor.close()
        mssql_cursor.close()

        return inserted

    except Exception as e:
        print(f"  ✗ Data copy error: {str(e)[:100]}")
        return 0

def main():
    print("=" * 80)
    print("Creating Missing Tables and Copying Data")
    print("=" * 80)
    print(f"\nTables to create: {', '.join(TABLES_TO_CREATE)}\n")

    pg_conn = get_pg_connection()
    mssql_conn = get_mssql_connection()
    print("✓ Connected to both databases\n")

    print("=" * 80)
    print("Creating tables and copying data...")
    print("=" * 80 + "\n")

    results = []

    for i, table in enumerate(TABLES_TO_CREATE, 1):
        print(f"[{i}/{len(TABLES_TO_CREATE)}] {table}")

        # Get structure from PostgreSQL
        columns = get_table_columns(pg_conn, table)
        pk_columns = get_primary_keys(pg_conn, table)

        if not columns:
            print(f"  ✗ Table not found in PostgreSQL\n")
            results.append((table, False, 0))
            continue

        # Create in MSSQL
        print(f"  Creating table structure...")
        created = create_mssql_table(mssql_conn, table, columns, pk_columns)

        if not created:
            print()
            results.append((table, False, 0))
            continue

        print(f"  ✓ Table created")

        # Copy data
        print(f"  Copying data...")
        rows = copy_table_data(pg_conn, mssql_conn, table)

        if rows > 0:
            print(f"  ✓ {rows} rows inserted\n")
        else:
            print(f"  ⚠ No data to copy\n")

        results.append((table, created, rows))

    pg_conn.close()
    mssql_conn.close()

    # Summary
    print("=" * 80)
    print("SUMMARY")
    print("=" * 80)

    total_created = sum(1 for r in results if r[1])
    total_rows = sum(r[2] for r in results)

    print(f"Tables processed: {len(results)}")
    print(f"Tables created: {total_created}")
    print(f"Total rows inserted: {total_rows}")

    print("\nDetails:")
    for table, created, rows in results:
        status = "✓ Created" if created else "✗ Failed"
        data = f", {rows} rows" if rows > 0 else ""
        print(f"  {table}: {status}{data}")

    print("=" * 80)

if __name__ == "__main__":
    main()
