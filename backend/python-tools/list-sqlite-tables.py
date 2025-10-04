#!/usr/bin/env python3
"""
List all tables in the SQLite database
"""
import sqlite3
import os

def list_sqlite_tables():
    """List all tables in the SQLite database"""
    
    # Path to the most recent SQLite file
    sqlite_file = r"D:\Multiplex\backend\WINITAPI\Data\Sqlite\User\2025\7\29\TB1204\TB1204_20250729141814720.db"
    
    if not os.path.exists(sqlite_file):
        print(f"[ERROR] SQLite file not found: {sqlite_file}")
        return
        
    print(f"[INFO] Listing tables in: {sqlite_file}")
    
    try:
        conn = sqlite3.connect(sqlite_file)
        cursor = conn.cursor()
        
        # List all tables in the database
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name")
        tables = cursor.fetchall()
        
        print(f"[INFO] Database contains {len(tables)} tables:")
        for i, table in enumerate(tables, 1):
            table_name = table[0]
            
            # Get row count for each table
            try:
                cursor.execute(f"SELECT COUNT(*) FROM [{table_name}]")
                count = cursor.fetchone()[0]
                print(f"  {i:2d}. {table_name} ({count} rows)")
            except Exception as e:
                print(f"  {i:2d}. {table_name} (error getting count: {e})")
        
        # Check if there's a table with similar name to promotion
        promotion_like = [t[0] for t in tables if 'promo' in t[0].lower()]
        if promotion_like:
            print(f"[INFO] Tables containing 'promo': {promotion_like}")
        
        conn.close()
        
    except Exception as e:
        print(f"[ERROR] Failed to list tables: {e}")

if __name__ == "__main__":
    list_sqlite_tables()