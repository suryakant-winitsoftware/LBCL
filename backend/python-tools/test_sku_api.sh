#!/bin/bash

TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTk2NDczMDAsImlzcyI6Im15aXNzdWVyIn0.ejx1A5RU0oIRhbhYOIEcFFpAgN7DSBdieycVlBeQtrA"

echo "======================================================================"
echo "Testing SKU APIs on localhost:8000"
echo "======================================================================"

echo ""
echo "1. Testing /SKU/SelectAllSKUDetails"
echo "----------------------------------------------------------------------"
curl -X POST "http://localhost:8000/SKU/SelectAllSKUDetails" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "pageNumber": 0,
    "pageSize": 20,
    "sortCriterias": [],
    "filterCriterias": []
  }' | jq '.'

echo ""
echo ""
echo "2. Testing /SKU/GetSKUByUID with EMBT3001"
echo "----------------------------------------------------------------------"
curl -X GET "http://localhost:8000/SKU/GetSKUByUID?UID=EMBT3001" \
  -H "Authorization: Bearer $TOKEN" | jq '.'

echo ""
echo ""
echo "3. Testing /SKU/GetSKUByUID with FGLL0001325"
echo "----------------------------------------------------------------------"
curl -X GET "http://localhost:8000/SKU/GetSKUByUID?UID=FGLL0001325" \
  -H "Authorization: Bearer $TOKEN" | jq '.'

echo ""
echo ""
echo "4. Checking database directly for comparison"
echo "----------------------------------------------------------------------"
python3 << 'PYTHON_SCRIPT'
import psycopg2
from psycopg2.extras import RealDictCursor
import json

DB_PARAMS = {
    'host': '10.20.53.130',
    'port': '5432',
    'database': 'multiplexdev15072025',
    'user': 'multiplex',
    'password': 'multiplex'
}

conn = psycopg2.connect(**DB_PARAMS)
cursor = conn.cursor(cursor_factory=RealDictCursor)

cursor.execute("""
    SELECT
        s.uid, s.code, s.name, s.org_uid, s.base_uom,
        s.is_active, s.is_stockable
    FROM sku s
    WHERE s.code IN ('EMBT3001', 'FGLL0001325', 'LL10KL01')
    ORDER BY s.code
""")

print("Database SKUs:")
for row in cursor.fetchall():
    print(json.dumps(dict(row), indent=2, default=str))

cursor.close()
conn.close()
PYTHON_SCRIPT

echo ""
echo "======================================================================"
echo "Done!"
echo "======================================================================"
