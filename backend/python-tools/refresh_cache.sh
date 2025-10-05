#!/bin/bash

TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTk2NDczMDAsImlzcyI6Im15aXNzdWVyIn0.ejx1A5RU0oIRhbhYOIEcFFpAgN7DSBdieycVlBeQtrA"

echo "======================================================================"
echo "Refreshing SKU Cache"
echo "======================================================================"

curl -X POST "http://localhost:8000/api/SKU/RefreshSKUCache" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" | jq '.'

echo ""
echo ""
echo "Waiting 5 seconds for cache to refresh..."
sleep 5

echo ""
echo "======================================================================"
echo "Testing API again after cache refresh"
echo "======================================================================"

curl -X POST "http://localhost:8000/api/SKU/SelectAllSKUDetails" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "pageNumber": 1,
    "pageSize": 100,
    "isCountRequired": true
  }' | jq '{
  IsSuccess: .IsSuccess,
  TotalCount: .Data.TotalCount,
  PagedDataCount: (.Data.PagedData | length),
  SKUCodes: (.Data.PagedData | map(.Code) | sort)
}'
