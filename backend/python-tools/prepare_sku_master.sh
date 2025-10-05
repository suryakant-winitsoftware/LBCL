#!/bin/bash

TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTk2NDczMDAsImlzcyI6Im15aXNzdWVyIn0.ejx1A5RU0oIRhbhYOIEcFFpAgN7DSBdieycVlBeQtrA"

echo "======================================================================"
echo "Calling PrepareSKUMaster for LBCL org"
echo "======================================================================"

curl -X POST "http://localhost:8000/api/DataPreparation/PrepareSKUMaster" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "OrgUIDs": ["LBCL"],
    "DistributionChannelUIDs": null,
    "SKUUIDs": null,
    "AttributeTypes": null
  }' | jq '{IsSuccess, Message, TotalCount: .Data.TotalCount, RecordCount: (.Data.PagedData | length)}'

echo ""
echo ""
echo "Waiting 3 seconds..."
sleep 3

echo ""
echo "======================================================================"
echo "Testing SelectAllSKUDetails after PrepareSKUMaster"
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
