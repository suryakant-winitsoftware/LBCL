#!/usr/bin/env python3
"""
Check the most recent backend SQLite file for table_group_entity data
"""
import sqlite3
import os
import glob

def check_recent_backend_sqlite():
    """Check recent backend SQLite for sync configuration"""
    
    # Find the most recent SQLite file from backend
    sqlite_pattern = r"D:\Multiplex\backend\WINITAPI\Data\Sqlite\User\**\*.db"
    sqlite_files = glob.glob(sqlite_pattern, recursive=True)
    
    if not sqlite_files:
        print("[ERROR] No SQLite files found in backend")
        return
    
    # Sort by modification time to get the most recent
    sqlite_files.sort(key=os.path.getmtime, reverse=True)
    most_recent = sqlite_files[0]
    
    print(f"[INFO] Checking recent backend SQLite: {most_recent}")
    print(f"[INFO] File modified: {os.path.getmtime(most_recent)}")
    
    try:
        conn = sqlite3.connect(most_recent)
        cursor = conn.cursor()
        
        # Check table_group_entity count
        cursor.execute("SELECT COUNT(*) FROM table_group_entity")
        tge_count = cursor.fetchone()[0]
        print(f"[INFO] table_group_entity records: {tge_count}")
        
        # Check if promotion table config exists
        cursor.execute("SELECT COUNT(*) FROM table_group_entity WHERE table_name = 'promotion'")
        promo_config = cursor.fetchone()[0]
        print(f"[INFO] Promotion table config: {promo_config}")
        
        # Check table_group count
        cursor.execute("SELECT COUNT(*) FROM table_group")
        tg_count = cursor.fetchone()[0]
        print(f"[INFO] table_group records: {tg_count}")
        
        # Check if promotion group exists and is active
        cursor.execute("SELECT group_name, is_active FROM table_group WHERE group_name = 'promotion'")
        promo_group = cursor.fetchone()
        if promo_group:
            print(f"[INFO] Promotion group: {promo_group[0]}, active: {promo_group[1]}")
        else:
            print("[WARNING] Promotion group not found")
        
        # Check promotion table data
        cursor.execute("SELECT COUNT(*) FROM promotion")
        promo_data = cursor.fetchone()[0]
        print(f"[INFO] Promotion data records: {promo_data}")
        
        # If promotion table has data, show sample
        if promo_data > 0:
            cursor.execute("SELECT uid, name, org_uid FROM promotion LIMIT 1")
            sample = cursor.fetchone()
            print(f"[INFO] Sample promotion: uid={sample[0]}, name={sample[1]}, org_uid={sample[2]}")
        
        conn.close()
        
    except Exception as e:
        print(f"[ERROR] Failed to check backend SQLite: {e}")

if __name__ == "__main__":
    check_recent_backend_sqlite()