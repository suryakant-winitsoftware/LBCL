#!/usr/bin/env python3
"""
Update each SKU to trigger cache population
"""

import requests
import time

TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTk2NDczMDAsImlzcyI6Im15aXNzdWVyIn0.ejx1A5RU0oIRhbhYOIEcFFpAgN7DSBdieycVlBeQtrA"

SKU_CODES = [
    'EMBT3001', 'EMBT6001', 'EMBT6005', 'EMCT6001', 'EMCT6003',
    'FGLL000130L', 'FGLL0001325', 'FGLL0001625', 'FGLL0001EA', 'FGLL0001TR5',
    'LL10KL01', 'LL19KL01', 'LL325BL03', 'LL330BL01', 'LL330CL02',
    'LL500CL04', 'LL500CL05', 'LL500CL06'
]

headers = {
    "Content-Type": "application/json",
    "Authorization": f"Bearer {TOKEN}"
}

print("=" * 70)
print("Updating SKUs to populate cache")
print("=" * 70)

for sku_code in SKU_CODES:
    print(f"\nProcessing {sku_code}...")

    # 1. Get the SKU
    response = requests.get(
        f"http://localhost:8000/api/SKU/SelectSKUByUID?UID={sku_code}",
        headers=headers
    )

    if response.status_code != 200 or not response.json().get('IsSuccess'):
        print(f"  ✗ Failed to get SKU: {response.json()}")
        continue

    sku_data = response.json()['Data']

    # 2. Update the SKU (this should trigger cache population)
    response = requests.post(
        "http://localhost:8000/api/SKU/UpdateSKU",
        headers=headers,
        json=sku_data
    )

    if response.status_code == 200 and response.json().get('IsSuccess'):
        print(f"  ✓ Updated successfully")
    else:
        print(f"  ✗ Update failed: {response.json()}")

    time.sleep(0.2)  # Small delay

print("\n" + "=" * 70)
print("Testing SelectAllSKUDetails after updates")
print("=" * 70)

response = requests.post(
    "http://localhost:8000/api/SKU/SelectAllSKUDetails",
    headers=headers,
    json={
        "pageNumber": 1,
        "pageSize": 100,
        "isCountRequired": True
    }
)

data = response.json()
print(f"\nIsSuccess: {data.get('IsSuccess')}")
print(f"TotalCount: {data.get('Data', {}).get('TotalCount')}")
print(f"PagedData Count: {len(data.get('Data', {}).get('PagedData', []))}")

if data.get('Data', {}).get('PagedData'):
    print("\n✓ SKU Codes returned:")
    for sku in sorted(data['Data']['PagedData'], key=lambda x: x.get('Code', '')):
        print(f"  - {sku.get('Code')}")
else:
    print("\n⚠️  Still no SKUs returned")

print("\nDone!")
