#!/usr/bin/env python3
"""
Test Warehouse API with correct configuration
"""

import requests
import json

# API Configuration
BASE_URL = "http://localhost:8000"
TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTk2MzkzODEsImlzcyI6Im15aXNzdWVyIn0.ZaXarVldnL7yOe3R8s8DjuHwsNaWKLiByjWCrqGjxfg"
WAREHOUSE_UID = "WH001"

headers = {
    "Authorization": f"Bearer {TOKEN}",
    "Content-Type": "application/json"
}

print("=" * 70)
print("Testing Warehouse API")
print(f"Backend: {BASE_URL}")
print(f"Warehouse UID: {WAREHOUSE_UID}")
print("=" * 70)

# Test 1: Get Organizations
print("\n1. Testing /Org/GetOrgDetails to find organizations:")
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
        },
        timeout=10
    )

    print(f"Status Code: {response.status_code}")

    if response.status_code == 200:
        data = response.json()
        orgs = data.get('Data', {}).get('PagedData', [])
        print(f"Found {len(orgs)} organizations:")

        franchisee_org_uid = None
        for org in orgs:
            print(f"  • {org.get('OrganizationName', 'N/A')}")
            print(f"    UID: {org.get('UID', 'N/A')}")
            print(f"    Type: {org.get('OrgTypeName', 'N/A')}")

            # Save first org as franchisee UID
            if not franchisee_org_uid:
                franchisee_org_uid = org.get('UID')

        if franchisee_org_uid:
            print(f"\n✓ Using Franchisee UID: {franchisee_org_uid}")

            # Test 2: Get Warehouses
            print(f"\n2. Testing /Org/ViewFranchiseeWarehouse:")
            print("-" * 70)

            response2 = requests.post(
                f"{BASE_URL}/Org/ViewFranchiseeWarehouse?FranchiseeOrgUID={franchisee_org_uid}",
                headers=headers,
                json={
                    "PageNumber": 1,
                    "PageSize": 10,
                    "IsCountRequired": True,
                    "FilterCriterias": [],
                    "SortCriterias": []
                },
                timeout=10
            )

            print(f"Status Code: {response2.status_code}")

            if response2.status_code == 200:
                data2 = response2.json()
                print(f"\nFull Response:")
                print(json.dumps(data2, indent=2))

                warehouses = data2.get('Data', {}).get('PagedData', [])
                total = data2.get('Data', {}).get('TotalCount', 0)

                print(f"\n✓ Total Warehouses: {total}")
                print(f"✓ Returned: {len(warehouses)}")

                if warehouses:
                    print("\nWarehouses:")
                    for wh in warehouses:
                        print(f"  • {wh.get('WarehouseName', 'N/A')} ({wh.get('WarehouseCode', 'N/A')})")
                        print(f"    UID: {wh.get('WarehouseUID', 'N/A')}")
                else:
                    print("\n⚠ No warehouses found!")
                    print("\n📝 The issue is: No warehouses exist in the database")
                    print("   You need to create a warehouse first.")
            else:
                print(f"❌ Error Response: {response2.text}")

    else:
        print(f"❌ Error Response: {response.text}")

except requests.exceptions.ConnectionError:
    print("❌ Could not connect to backend at http://localhost:8000")
    print("   Make sure the backend is running!")
except Exception as e:
    print(f"❌ Error: {e}")
    import traceback
    traceback.print_exc()

print("\n" + "=" * 70)
print("Summary:")
print("=" * 70)
print("The frontend issue is likely one of these:")
print("1. Wrong FranchiseeOrgUID (currently hardcoded to 'DEFAULT_ORG_UID')")
print("2. No warehouses exist in the database for that organization")
print("3. Backend API endpoint is not working correctly")
print("\nSolution:")
print("- Update the frontend to use the correct FranchiseeOrgUID")
print("- Or create a warehouse using the create page first")
print("=" * 70)
