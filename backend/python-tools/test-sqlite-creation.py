#!/usr/bin/env python3
"""
Test script to verify SQLite database creation from PostgreSQL
"""
import os
import sqlite3
import tempfile
import shutil
import requests
import json
from datetime import datetime

def test_sqlite_creation():
    """Test that SQLite database is created with fresh PostgreSQL data"""
    
    # Create a temporary directory for test SQLite files
    test_dir = tempfile.mkdtemp()
    print(f"[INFO] Using test directory: {test_dir}")
    
    try:
        # Make a request to create a new SQLite database
        print("[INFO] Testing SQLite database creation...")
        
        # Use a test user ID - you might need to adjust this based on your system
        test_user_id = "TB1204"  # Using existing user from logs
        
        # API endpoint to create SQLite database
        api_url = "http://localhost:5000/api/MobileAppAction/InitiateDBCreation"
        payload = {
            "userID": test_user_id
        }
        
        print(f"[INFO] Making API request to: {api_url}")
        print(f"[INFO] Payload: {payload}")
        
        response = requests.post(api_url, json=payload, timeout=30)
        
        if response.status_code == 200:
            print("[SUCCESS] API request successful")
            result = response.json()
            print(f"[INFO] Response: {result}")
            
            # Check if we can find the created SQLite file
            sqlite_base_path = r"D:\Multiplex\backend\WINITAPI\Data\SyncLog"
            current_date = datetime.now()
            year = current_date.year
            month = current_date.month
            day = current_date.day
            
            user_folder = os.path.join(sqlite_base_path, str(year), str(month), str(day), test_user_id)
            
            if os.path.exists(user_folder):
                print(f"[SUCCESS] User folder found: {user_folder}")
                
                # Look for SQLite files
                sqlite_files = [f for f in os.listdir(user_folder) if f.endswith('.db')]
                
                if sqlite_files:
                    sqlite_file = os.path.join(user_folder, sqlite_files[0])
                    print(f"[SUCCESS] SQLite file found: {sqlite_file}")
                    
                    # Test the SQLite database
                    test_sqlite_content(sqlite_file)
                else:
                    print("[ERROR] No SQLite files found in user folder")
            else:
                print(f"[ERROR] User folder not found: {user_folder}")
                
        else:
            print(f"[ERROR] API request failed with status {response.status_code}")
            print(f"[ERROR] Response: {response.text}")
            
    except requests.exceptions.RequestException as e:
        print(f"[ERROR] Request failed: {e}")
    except Exception as e:
        print(f"[ERROR] Test failed: {e}")
    finally:
        # Clean up temporary directory
        shutil.rmtree(test_dir, ignore_errors=True)

def test_sqlite_content(sqlite_file):
    """Test the content of the SQLite database"""
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
            
            # Check a few sample records
            cursor.execute("SELECT uid, title, org_uid FROM promotion LIMIT 3")
            records = cursor.fetchall()
            print(f"[INFO] Sample promotion records: {records}")
            
            # Check for the specific July 2025 record that should be in PostgreSQL
            cursor.execute("SELECT COUNT(*) FROM promotion WHERE strftime('%Y', created_by) = '2025'")
            recent_count = cursor.fetchone()[0]
            print(f"[INFO] Promotion records from 2025: {recent_count}")
            
        else:
            print("[ERROR] Promotion table not found in SQLite")
            
        conn.close()
        
    except Exception as e:
        print(f"[ERROR] SQLite content test failed: {e}")

if __name__ == "__main__":
    test_sqlite_creation()