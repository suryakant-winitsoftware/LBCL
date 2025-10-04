#!/usr/bin/env python3
"""
Check promotion data in the most recent SQLite database
"""
import sqlite3
import os
import glob

def check_promotion_data():
    """Check promotion data in SQLite databases"""
    
    # Find the most recent SQLite file
    sqlite_pattern = r"D:\Multiplex\backend\WINITAPI\Data\Sqlite\User\**\*.db"
    sqlite_files = glob.glob(sqlite_pattern, recursive=True)
    
    if not sqlite_files:
        print("[ERROR] No SQLite files found")
        return
    
    # Sort by modification time to get the most recent
    sqlite_files.sort(key=os.path.getmtime, reverse=True)
    most_recent = sqlite_files[0]
    
    print(f"[INFO] Checking most recent SQLite file: {most_recent}")
    print(f"[INFO] File modified: {os.path.getmtime(most_recent)}")
    
    try:
        conn = sqlite3.connect(most_recent)
        cursor = conn.cursor()
        
        # Check if promotion table exists
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='promotion'")
        if cursor.fetchone():
            print("[SUCCESS] Promotion table exists in SQLite")
            
            # Check table structure
            cursor.execute("PRAGMA table_info(promotion)")
            columns = cursor.fetchall()
            print(f"[INFO] Promotion table has {len(columns)} columns")
            
            # Check promotion data count
            cursor.execute("SELECT COUNT(*) FROM promotion")
            count = cursor.fetchone()[0]
            print(f"[INFO] Promotion records in SQLite: {count}")
            
            if count > 0:
                # Check sample data
                cursor.execute("SELECT uid, name, description, format, org_uid FROM promotion LIMIT 3")
                records = cursor.fetchall()
                print(f"[INFO] Sample promotion records:")
                for i, record in enumerate(records, 1):
                    print(f"  Record {i}: {record}")
                    
                # Check org_uid distribution
                cursor.execute("SELECT org_uid, COUNT(*) FROM promotion GROUP BY org_uid")
                org_distribution = cursor.fetchall()
                print(f"[INFO] Org UID distribution:")
                for org_uid, count in org_distribution:
                    print(f"  {org_uid}: {count} records")
            else:
                print("[WARNING] Promotion table exists but contains no data")
                
        else:
            print("[ERROR] Promotion table not found in SQLite")
            
        conn.close()
        
    except Exception as e:
        print(f"[ERROR] Failed to check promotion data: {e}")

if __name__ == "__main__":
    check_promotion_data()