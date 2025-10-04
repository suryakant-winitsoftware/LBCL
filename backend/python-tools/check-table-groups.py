#!/usr/bin/env python3
"""
Check table group entities configuration in SQLite
"""
import sqlite3
import os

def check_table_groups():
    """Check table group entities configuration"""
    
    # Path to the most recent SQLite file
    sqlite_file = r"D:\Multiplex\backend\WINITAPI\Data\Sqlite\User\2025\7\29\TB1204\TB1204_20250729141814720.db"
    
    if not os.path.exists(sqlite_file):
        print(f"[ERROR] SQLite file not found: {sqlite_file}")
        return
        
    print(f"[INFO] Checking table groups in: {sqlite_file}")
    
    try:
        conn = sqlite3.connect(sqlite_file)
        cursor = conn.cursor()
        
        # Check table_group_entity for promotion
        cursor.execute("SELECT * FROM table_group_entity WHERE table_name LIKE '%promo%'")
        promo_entries = cursor.fetchall()
        
        if promo_entries:
            print(f"[INFO] Found {len(promo_entries)} promotion-related entries:")
            for entry in promo_entries:
                print(f"  {entry}")
        else:
            print("[INFO] No promotion-related entries found in table_group_entity")
        
        # Check all table group entities
        cursor.execute("SELECT table_name, has_download, is_active FROM table_group_entity ORDER BY table_name")
        all_entries = cursor.fetchall()
        
        print(f"[INFO] All table group entities ({len(all_entries)} total):")
        download_enabled = []
        download_disabled = []
        
        for entry in all_entries:
            table_name, has_download, is_active = entry
            if has_download and is_active:
                download_enabled.append(table_name)
            else:
                download_disabled.append(table_name)
        
        print(f"[INFO] Tables with download enabled ({len(download_enabled)}):")
        for table in download_enabled:
            print(f"  - {table}")
            
        print(f"[INFO] Tables with download disabled ({len(download_disabled)}):")
        for table in download_disabled[:10]:  # Show first 10
            print(f"  - {table}")
        if len(download_disabled) > 10:
            print(f"  ... and {len(download_disabled) - 10} more")
        
        conn.close()
        
    except Exception as e:
        print(f"[ERROR] Failed to check table groups: {e}")

if __name__ == "__main__":
    check_table_groups()