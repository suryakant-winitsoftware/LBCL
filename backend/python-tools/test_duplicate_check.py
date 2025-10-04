#!/usr/bin/env python3
"""
Test script to verify duplicate check is working
"""

import requests
import json

# API Configuration
BASE_URL = "http://10.20.53.130:5001/api"  # Adjust if different
AUTH_TOKEN = "your_token_here"  # You'll need to provide this

def test_get_mapping(linked_item_uid):
    """Test GetSelectionMapMasterByLinkedItemUID endpoint"""

    url = f"{BASE_URL}/Mapping/GetSelectionMapMasterByLinkedItemUID"
    params = {"linkedItemUID": linked_item_uid}

    headers = {
        "Authorization": f"Bearer {AUTH_TOKEN}",
        "Content-Type": "application/json"
    }

    print(f"\n{'='*60}")
    print(f"Testing: GetSelectionMapMasterByLinkedItemUID")
    print(f"LinkedItemUID: {linked_item_uid}")
    print(f"URL: {url}")
    print(f"{'='*60}\n")

    try:
        response = requests.get(url, params=params, headers=headers)

        print(f"Status Code: {response.status_code}")
        print(f"Response Headers: {dict(response.headers)}\n")

        if response.status_code == 200:
            data = response.json()
            print(f"✅ SUCCESS: Found existing mapping!")
            print(f"Response: {json.dumps(data, indent=2)}")
            return data
        elif response.status_code == 404:
            print(f"❌ NOT FOUND: No mapping exists (should allow creation)")
            return None
        else:
            print(f"⚠️  Unexpected status: {response.status_code}")
            print(f"Response: {response.text}")
            return None

    except Exception as e:
        print(f"❌ ERROR: {str(e)}")
        return None

if __name__ == "__main__":
    # Test with known duplicate
    print("\n" + "="*60)
    print("DUPLICATE CHECK TEST")
    print("="*60)

    # Test 1: Check for existing duplicate (4094-PL)
    print("\n\nTest 1: Check existing Price List (4094-PL - has duplicates)")
    result1 = test_get_mapping("4094-PL")

    if result1:
        print("\n✅ API correctly found the existing mapping")
        print("   Frontend duplicate check should block creation")
    else:
        print("\n❌ API did NOT find existing mapping")
        print("   This is the BUG! Backend query is still broken")

    # Test 2: Check for existing duplicate (CLASS_GROUP_1757387290583)
    print("\n\nTest 2: Check existing SKU Class Group (CLASS_GROUP_1757387290583 - has 7 duplicates)")
    result2 = test_get_mapping("CLASS_GROUP_1757387290583")

    if result2:
        print("\n✅ API correctly found the existing mapping")
    else:
        print("\n❌ API did NOT find existing mapping - BUG!")

    # Test 3: Check non-existent mapping
    print("\n\nTest 3: Check non-existent item (should return 404)")
    result3 = test_get_mapping("NONEXISTENT_ITEM_12345")

    if not result3:
        print("\n✅ API correctly returned not found for non-existent item")
    else:
        print("\n⚠️  Unexpected: Found mapping for non-existent item!")

    print("\n" + "="*60)
    print("TEST COMPLETE")
    print("="*60 + "\n")
