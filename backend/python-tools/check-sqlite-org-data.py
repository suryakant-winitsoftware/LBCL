#!/usr/bin/env python3
"""
Check org data in SQLite to understand what OrgUID is being used during sync
"""
import sqlite3
import os
import glob

def check_sqlite_org_data():
    """Check org data in SQLite to understand OrgUID usage"""
    
    # Find the most recent SQLite file
    sqlite_pattern = r"D:\Multiplex\backend\WINITAPI\Data\Sqlite\User\**\*.db"
    sqlite_files = glob.glob(sqlite_pattern, recursive=True)
    
    if not sqlite_files:
        print("[ERROR] No SQLite files found")
        return
    
    # Sort by modification time to get the most recent
    sqlite_files.sort(key=os.path.getmtime, reverse=True)
    most_recent = sqlite_files[0]
    
    print(f"[INFO] Checking org data in: {most_recent}")
    
    try:
        conn = sqlite3.connect(most_recent)
        cursor = conn.cursor()
        
        # Check org table data
        cursor.execute("SELECT uid, code, name FROM org LIMIT 5")
        org_records = cursor.fetchall()
        print(f"[INFO] Sample org records:")
        for i, record in enumerate(org_records, 1):
            print(f"  {i}. UID: {record[0]}, Code: {record[1]}, Name: {record[2]}")
        
        # Check if there are any tables with org_uid constraints that have data
        # This will help us understand what org_uid is being used during sync
        
        # Check sku table (which has org_uid filter in query)
        cursor.execute("SELECT DISTINCT org_uid FROM sku LIMIT 5")
        sku_orgs = cursor.fetchall()
        print(f"[INFO] Org UIDs in SKU table:")
        for org_uid in sku_orgs:
            print(f"  {org_uid[0]}")
        
        # Check store table
        cursor.execute("SELECT uid FROM store LIMIT 1")
        store_record = cursor.fetchone()
        if store_record:
            store_uid = store_record[0]
            print(f"[INFO] Store UID in sync: {store_uid}")
        
        # Look at route_customer to understand the relationship
        cursor.execute("SELECT store_uid, route_uid FROM route_customer LIMIT 1")
        route_customer = cursor.fetchone()
        if route_customer:
            print(f"[INFO] Route-Customer relationship: Store={route_customer[0]}, Route={route_customer[1]}")
        
        # Check job_position
        cursor.execute("SELECT uid FROM job_position LIMIT 1")
        job_pos = cursor.fetchone()
        if job_pos:
            print(f"[INFO] Job Position UID: {job_pos[0]}")
        
        conn.close()
        
        # The key insight is that if SKU table has data with specific org_uid values,
        # then that's the org_uid being passed to the promotion query
        if sku_orgs and sku_orgs[0][0] is not None:
            suspected_org_uid = sku_orgs[0][0]
            print(f"[INFO] Suspected OrgUID being used in sync: {suspected_org_uid}")
            print(f"[INFO] This would explain why promotion (org_uid=NULL) doesn't match")
        
    except Exception as e:
        print(f"[ERROR] Failed to check SQLite org data: {e}")

if __name__ == "__main__":
    check_sqlite_org_data()