#!/usr/bin/env python3
"""
Debug warehouse data - check what's actually in the database
"""

import psycopg2
from psycopg2.extras import RealDictCursor

DB_PARAMS = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev15072025',
    'user': 'multiplex',
    'password': 'multiplex'
}

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

print("=" * 70)
print("Warehouse Data Debug")
print("=" * 70)

# 1. Check organizations table
print("\n1. Organizations in database:")
print("-" * 70)
cursor.execute("""
    SELECT uid, organization_name_en, organization_code, org_type_uid, parent_uid
    FROM organizations
    WHERE ss = 1
    ORDER BY created_time DESC
    LIMIT 10
""")
orgs = cursor.fetchall()
print(f"Found {len(orgs)} organizations:")
for org in orgs:
    print(f"  • {org['organization_name_en'] or 'N/A'}")
    print(f"    UID: {org['uid']}")
    print(f"    Code: {org['organization_code']}")
    print(f"    Type UID: {org['org_type_uid']}")
    print(f"    Parent UID: {org['parent_uid'] or 'None'}")

# 2. Check warehouse-related organizations
print("\n\n2. Warehouse-type organizations:")
print("-" * 70)
cursor.execute("""
    SELECT uid, organization_name_en, organization_code, org_type_uid
    FROM organizations
    WHERE org_type_uid IN ('FRWH', 'VWH', 'WH', 'Warehouse')
    AND ss = 1
    ORDER BY created_time DESC
""")
warehouses = cursor.fetchall()
print(f"Found {len(warehouses)} warehouse organizations:")
for wh in warehouses:
    print(f"  • {wh['organization_name_en']} ({wh['organization_code']})")
    print(f"    UID: {wh['uid']}")
    print(f"    Type: {wh['org_type_uid']}")

# 3. Check org_type_master table
print("\n\n3. Organization Types:")
print("-" * 70)
cursor.execute("""
    SELECT uid, org_type_name_en, org_type_code
    FROM org_type_master
    WHERE ss = 1
    ORDER BY org_type_name_en
""")
org_types = cursor.fetchall()
print(f"Found {len(org_types)} organization types:")
for ot in org_types:
    print(f"  • {ot['org_type_name_en']} (Code: {ot['org_type_code']}, UID: {ot['uid']})")

# 4. Check if there's a warehouse_master table
print("\n\n4. Checking for warehouse_master table:")
print("-" * 70)
cursor.execute("""
    SELECT table_name
    FROM information_schema.tables
    WHERE table_schema = 'public'
    AND LOWER(table_name) LIKE '%warehouse%'
""")
warehouse_tables = cursor.fetchall()
print(f"Found {len(warehouse_tables)} warehouse-related tables:")
for table in warehouse_tables:
    print(f"  • {table['table_name']}")

    # Get count from each table
    try:
        cursor.execute(f"SELECT COUNT(*) as count FROM {table['table_name']}")
        count = cursor.fetchone()['count']
        print(f"    Records: {count}")
    except:
        print(f"    (Could not count)")

# 5. Check the actual API view/query
print("\n\n5. Simulating ViewFranchiseeWarehouse query:")
print("-" * 70)

# Get a sample org UID
if orgs:
    sample_org_uid = orgs[0]['uid']
    print(f"Testing with FranchiseeOrgUID: {sample_org_uid}")

    # This is what the backend likely does
    cursor.execute("""
        SELECT
            o.uid as warehouse_uid,
            o.organization_code as warehouse_code,
            o.organization_name_en as warehouse_name,
            o.org_type_uid,
            o.parent_uid,
            o.modified_time
        FROM organizations o
        WHERE o.parent_uid = %s
        AND o.org_type_uid IN ('FRWH', 'VWH', 'WH')
        AND o.ss = 1
    """, (sample_org_uid,))

    results = cursor.fetchall()
    print(f"\nFound {len(results)} child warehouse organizations:")
    for r in results:
        print(f"  • {r['warehouse_name']} ({r['warehouse_code']})")
        print(f"    Type: {r['org_type_uid']}")

# 6. Check all organizations regardless of parent
print("\n\n6. ALL Organizations with warehouse-like types:")
print("-" * 70)
cursor.execute("""
    SELECT
        uid,
        organization_code,
        organization_name_en,
        org_type_uid,
        parent_uid
    FROM organizations
    WHERE LOWER(organization_name_en) LIKE '%warehouse%'
    OR LOWER(organization_name_en) LIKE '%van%'
    OR org_type_uid LIKE '%WH%'
    AND ss = 1
""")
all_warehouses = cursor.fetchall()
print(f"Found {len(all_warehouses)} organizations:")
for wh in all_warehouses:
    print(f"  • {wh['organization_name_en']} ({wh['organization_code']})")
    print(f"    UID: {wh['uid']}")
    print(f"    Type: {wh['org_type_uid']}")
    print(f"    Parent: {wh['parent_uid']}")

cursor.close()
conn.close()

print("\n" + "=" * 70)
print("SOLUTION:")
print("=" * 70)
print("The issue is likely:")
print("1. No warehouse organizations exist in the 'organizations' table")
print("2. Or warehouse org_type_uid doesn't match 'FRWH' or 'VWH'")
print("3. Or warehouses don't have the correct parent_uid")
print("\nYou need to create warehouse records in the 'organizations' table")
print("with org_type_uid = 'FRWH' or 'VWH'")
print("=" * 70)
