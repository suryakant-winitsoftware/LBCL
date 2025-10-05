#!/usr/bin/env python3
"""
Territory Table Creation Script
This script connects to SQL Server and creates the territory table
"""

import pyodbc
import sys
from pathlib import Path

# Database connection details
SERVER = '10.20.53.175'
DATABASE = 'LBSSFADev'
USERNAME = 'lbssfadev'
PASSWORD = 'lbssfadev'

def connect_to_database():
    """Establish connection to SQL Server database"""
    try:
        # Connection string for SQL Server
        conn_str = (
            f'DRIVER={{ODBC Driver 17 for SQL Server}};'
            f'SERVER={SERVER};'
            f'DATABASE={DATABASE};'
            f'UID={USERNAME};'
            f'PWD={PASSWORD};'
            f'TrustServerCertificate=yes;'
        )

        print(f"Connecting to SQL Server: {SERVER}")
        print(f"Database: {DATABASE}")

        conn = pyodbc.connect(conn_str)
        print("✓ Successfully connected to database")
        return conn

    except pyodbc.Error as e:
        print(f"✗ Error connecting to database: {e}")
        sys.exit(1)

def read_sql_file():
    """Read the territory_schema.sql file"""
    try:
        sql_file = Path(__file__).parent / 'territory_schema.sql'

        if not sql_file.exists():
            print(f"✗ SQL file not found: {sql_file}")
            sys.exit(1)

        with open(sql_file, 'r', encoding='utf-8') as f:
            sql_content = f.read()

        print(f"✓ Read SQL file: {sql_file}")
        return sql_content

    except Exception as e:
        print(f"✗ Error reading SQL file: {e}")
        sys.exit(1)

def execute_sql_script(conn, sql_content):
    """Execute the SQL script to create territory table"""
    try:
        cursor = conn.cursor()

        # Split the SQL content by GO statements
        sql_commands = sql_content.split('GO')

        print("\nExecuting SQL commands...")

        for i, command in enumerate(sql_commands, 1):
            command = command.strip()
            if command:
                try:
                    cursor.execute(command)
                    conn.commit()
                    print(f"✓ Command {i} executed successfully")
                except pyodbc.Error as e:
                    print(f"⚠ Warning on command {i}: {e}")
                    # Continue with next command even if one fails

        cursor.close()
        print("\n✓ SQL script execution completed")

    except Exception as e:
        print(f"✗ Error executing SQL script: {e}")
        conn.rollback()
        sys.exit(1)

def verify_table_creation(conn):
    """Verify that the territory table was created successfully"""
    try:
        cursor = conn.cursor()

        # Check if table exists
        cursor.execute("""
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = 'territory'
        """)

        result = cursor.fetchone()

        if result:
            print("\n✓ Territory table created successfully")

            # Get column count
            cursor.execute("""
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'territory'
            """)

            column_count = cursor.fetchone()[0]
            print(f"✓ Table has {column_count} columns")

            # Get index count
            cursor.execute("""
                SELECT COUNT(*)
                FROM sys.indexes
                WHERE object_id = OBJECT_ID('territory')
                AND name IS NOT NULL
            """)

            index_count = cursor.fetchone()[0]
            print(f"✓ Table has {index_count} indexes")

            # Show table structure
            cursor.execute("""
                SELECT
                    COLUMN_NAME,
                    DATA_TYPE,
                    CHARACTER_MAXIMUM_LENGTH,
                    IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'territory'
                ORDER BY ORDINAL_POSITION
            """)

            print("\nTable Structure:")
            print("-" * 80)
            print(f"{'Column Name':<30} {'Data Type':<20} {'Nullable':<10}")
            print("-" * 80)

            for row in cursor.fetchall():
                col_name = row.COLUMN_NAME
                data_type = row.DATA_TYPE
                max_length = row.CHARACTER_MAXIMUM_LENGTH
                is_nullable = row.IS_NULLABLE

                if max_length:
                    data_type = f"{data_type}({max_length})"

                print(f"{col_name:<30} {data_type:<20} {is_nullable:<10}")

            print("-" * 80)

        else:
            print("\n✗ Territory table was not created")
            sys.exit(1)

        cursor.close()

    except Exception as e:
        print(f"✗ Error verifying table creation: {e}")
        sys.exit(1)

def main():
    """Main function to execute the script"""
    print("=" * 80)
    print("Territory Table Creation Script")
    print("=" * 80)
    print()

    # Step 1: Connect to database
    conn = connect_to_database()

    # Step 2: Read SQL file
    sql_content = read_sql_file()

    # Step 3: Execute SQL script
    execute_sql_script(conn, sql_content)

    # Step 4: Verify table creation
    verify_table_creation(conn)

    # Close connection
    conn.close()
    print("\n✓ Database connection closed")

    print("\n" + "=" * 80)
    print("Territory table setup completed successfully!")
    print("=" * 80)
    print("\nNext steps:")
    print("1. Restart the backend API server")
    print("2. Test the API endpoints at: http://localhost:8000/api/Territory")
    print("3. Access the UI at: /administration/territory-management/territories")
    print()

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n✗ Script interrupted by user")
        sys.exit(1)
    except Exception as e:
        print(f"\n✗ Unexpected error: {e}")
        sys.exit(1)
