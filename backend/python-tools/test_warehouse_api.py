#!/usr/bin/env python3
"""
Test Warehouse API to understand the issue
"""

import requests
import json

# API Configuration
BASE_URL = "http://localhost:5000"  # Adjust if different
TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTk1ODQ5ODgsImlzcyI6Im15aXNzdWVyIn0.EQKrqJ8Q8PvQmyEk47AzGCZ1mofGbmeUebTb_r_jPd8"

headers = {
    "Authorization": f"Bearer {TOKEN}",
    "Content-Type": "application/json"
}

print("=" * 70)
print("Testing Warehouse API")
print("=" * 70)

# Test 1: Get Organizations to find the correct UID
print("\n1. Testing /Org/GetOrgDetails to find valid Organization UIDs:")
print("-" * 70)

try:
    response = requests.post(
        f"{BASE_URL}/Org/GetOrgDetails",
        headers=headers,
        json={
            "PageNumber": 1,
            "PageSize": 10,
            "IsCountRequired": True,
            "FilterCriterias": [],
            "SortCriterias": []
        }
    )

    print(f"Status Code: {response.status_code}")

    if response.status_code == 200:
        data = response.json()
        orgs = data.get('Data', {}).get('PagedData', [])
        print(f"Found {len(orgs)} organizations:")
        for org in orgs:
            print(f"  • {org.get('OrganizationName', 'N/A')} (UID: {org.get('UID', 'N/A')})")
            print(f"    Type: {org.get('OrgTypeName', 'N/A')}")

        # Use first org UID for testing
        if orgs:
            test_org_uid = orgs[0].get('UID')
            print(f"\nUsing UID for testing: {test_org_uid}")

            # Test 2: Get Warehouses for this UID
            print(f"\n2. Testing /Org/ViewFranchiseeWarehouse with UID: {test_org_uid}")
            print("-" * 70)

            response2 = requests.post(
                f"{BASE_URL}/Org/ViewFranchiseeWarehouse?FranchiseeOrgUID={test_org_uid}",
                headers=headers,
                json={
                    "PageNumber": 1,
                    "PageSize": 10,
                    "IsCountRequired": True,
                    "FilterCriterias": [],
                    "SortCriterias": []
                }
            )

            print(f"Status Code: {response2.status_code}")

            if response2.status_code == 200:
                data2 = response2.json()
                warehouses = data2.get('Data', {}).get('PagedData', [])
                total = data2.get('Data', {}).get('TotalCount', 0)

                print(f"Total Warehouses: {total}")
                print(f"Returned: {len(warehouses)}")

                if warehouses:
                    print("\nWarehouses found:")
                    for wh in warehouses:
                        print(f"  • {wh.get('WarehouseName', 'N/A')} ({wh.get('WarehouseCode', 'N/A')})")
                else:
                    print("\n⚠ No warehouses found for this organization!")
                    print("\nYou need to create a warehouse using:")
                    print(f"  POST /Org/CreateViewFranchiseeWarehouse")
            else:
                print(f"Error Response: {response2.text}")
    else:
        print(f"Error Response: {response.text}")

except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()

# Test 3: Check if there's a default/current org in session
print("\n\n3. Checking for session/current organization endpoint:")
print("-" * 70)

try:
    # Try common endpoints for getting current user org
    endpoints_to_try = [
        "/Auth/GetCurrentUser",
        "/Auth/GetUserInfo",
        "/Session/GetCurrentOrg",
        "/User/GetProfile"
    ]

    for endpoint in endpoints_to_try:
        try:
            response = requests.get(f"{BASE_URL}{endpoint}", headers=headers)
            if response.status_code == 200:
                print(f"✓ {endpoint} returned:")
                print(json.dumps(response.json(), indent=2)[:500])
                break
        except:
            pass

except Exception as e:
    print(f"Could not find session endpoint: {e}")

print("\n" + "=" * 70)
print("API Test Complete")
print("=" * 70)
