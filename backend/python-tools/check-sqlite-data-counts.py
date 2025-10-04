#!/usr/bin/env python3
"""
Check data counts in SQLite database to see what's syncing
"""
import sqlite3
import os
import glob

def check_sqlite_data_counts():
    """Check data counts in most recent SQLite database"""
    
    # Find the most recent SQLite file
    sqlite_pattern = r"D:\Multiplex\backend\WINITAPI\Data\Sqlite\User\**\*.db"
    sqlite_files = glob.glob(sqlite_pattern, recursive=True)
    
    if not sqlite_files:
        print("[ERROR] No SQLite files found")
        return
    
    # Sort by modification time to get the most recent
    sqlite_files.sort(key=os.path.getmtime, reverse=True)
    most_recent = sqlite_files[0]
    
    print(f"[INFO] Checking data counts in: {most_recent}")
    
    try:
        conn = sqlite3.connect(most_recent)
        cursor = conn.cursor()
        
        # Get all tables and their row counts
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name")
        tables = cursor.fetchall()
        
        data_tables = []
        
        for table in tables:
            table_name = table[0]
            if table_name == 'sqlite_sequence':
                continue
                
            try:
                cursor.execute(f"SELECT COUNT(*) FROM [{table_name}]")
                count = cursor.fetchone()[0]
                if count > 0:
                    data_tables.append((table_name, count))
            except Exception as e:
                print(f"[ERROR] Error counting {table_name}: {e}")
        
        print(f"[INFO] Tables with data ({len(data_tables)} out of {len(tables)}):")
        data_tables.sort(key=lambda x: x[1], reverse=True)  # Sort by count descending
        
        for table_name, count in data_tables:
            print(f"  {table_name}: {count} rows")
        
        # Check some sample data from a table that has data
        if data_tables:
            sample_table = data_tables[0][0]  # Table with most data
            print(f"[INFO] Sample data from {sample_table}:")
            
            # Get column names
            cursor.execute(f"PRAGMA table_info([{sample_table}])")
            columns = cursor.fetchall()
            column_names = [col[1] for col in columns]
            
            # Show a few key columns if they exist
            key_columns = []
            for col in ['uid', 'org_uid', 'name', 'code']:
                if col in column_names:
                    key_columns.append(col)
            
            if key_columns:
                columns_str = ', '.join(key_columns)
                cursor.execute(f"SELECT {columns_str} FROM [{sample_table}] LIMIT 3")
                records = cursor.fetchall()
                
                for i, record in enumerate(records, 1):
                    print(f"  Record {i}: {record}")
        
        # Specifically check promotion table
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='promotion'")
        if cursor.fetchone():
            cursor.execute("SELECT COUNT(*) FROM promotion")
            promo_count = cursor.fetchone()[0]
            print(f"[INFO] Promotion table: {promo_count} rows")
            
            if promo_count == 0:
                # Check if there's a schema issue by looking at table structure
                cursor.execute("PRAGMA table_info(promotion)")
                columns = cursor.fetchall()
                print(f"[INFO] Promotion table has {len(columns)} columns")
                print("[INFO] This suggests table schema sync worked but data sync failed")
        
        conn.close()
        
    except Exception as e:
        print(f"[ERROR] Failed to check SQLite data: {e}")

if __name__ == "__main__":
    check_sqlite_data_counts()