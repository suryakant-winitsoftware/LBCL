#!/usr/bin/env python3
"""
Test script to check the most recent SQLite database content
"""
import sqlite3
import os
from datetime import datetime

def test_recent_sqlite():
    """Test the content of the most recent SQLite database"""
    
    # Path to the most recent SQLite file
    sqlite_file = r"D:\Multiplex\backend\WINITAPI\Data\Sqlite\User\2025\7\29\TB1204\TB1204_20250729141814720.db"
    
    if not os.path.exists(sqlite_file):
        print(f"[ERROR] SQLite file not found: {sqlite_file}")
        return
        
    print(f"[INFO] Testing SQLite file: {sqlite_file}")
    
    try:
        conn = sqlite3.connect(sqlite_file)
        cursor = conn.cursor()
        
        # Check if promotion table exists
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='promotion'")
        if cursor.fetchone():
            print("[SUCCESS] Promotion table exists in SQLite")
            
            # Check promotion data
            cursor.execute("SELECT COUNT(*) FROM promotion")
            count = cursor.fetchone()[0]
            print(f"[INFO] Promotion count in SQLite: {count}")
            
            # Check column structure
            cursor.execute("PRAGMA table_info(promotion)")
            columns = cursor.fetchall()
            print(f"[INFO] Promotion table has {len(columns)} columns")
            
            # Display first few column names
            column_names = [col[1] for col in columns]
            print(f"[INFO] First 10 columns: {column_names[:10]}")
            
            # Check if we have the correct columns (from PostgreSQL schema)
            expected_cols = ['uid', 'title', 'description', 'format', 'org_uid']
            found_cols = [col for col in expected_cols if col in column_names]
            print(f"[INFO] Expected columns found: {found_cols}")
            
            # Check a few sample records
            cursor.execute("SELECT uid, title, description, format, org_uid FROM promotion LIMIT 3")
            records = cursor.fetchall()
            print(f"[INFO] Sample promotion records:")
            for i, record in enumerate(records, 1):
                print(f"  Record {i}: {record}")
            
            # Check for NULL org_uid (should indicate fresh PostgreSQL data)
            cursor.execute("SELECT COUNT(*) FROM promotion WHERE org_uid IS NULL")
            null_org_count = cursor.fetchone()[0]
            print(f"[INFO] Records with NULL org_uid: {null_org_count}")
            
            # Check for FR001 org_uid (would indicate old template data)
            cursor.execute("SELECT COUNT(*) FROM promotion WHERE org_uid = 'FR001'")
            fr001_count = cursor.fetchone()[0]
            print(f"[INFO] Records with FR001 org_uid: {fr001_count}")
            
            if null_org_count > 0 and fr001_count == 0:
                print("[SUCCESS] SQLite appears to have fresh PostgreSQL data (NULL org_uid)")
            elif fr001_count > 0:
                print("[WARNING] SQLite still contains old template data (FR001 org_uid)")
            else:
                print("[INFO] Mixed or different data pattern")
                
        else:
            print("[ERROR] Promotion table not found in SQLite")
            
        # List all tables in the database
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table'")
        tables = cursor.fetchall()
        print(f"[INFO] Database contains {len(tables)} tables")
        
        conn.close()
        
    except Exception as e:
        print(f"[ERROR] SQLite test failed: {e}")

if __name__ == "__main__":
    test_recent_sqlite()