#!/bin/bash

TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTk2NDczMDAsImlzcyI6Im15aXNzdWVyIn0.ejx1A5RU0oIRhbhYOIEcFFpAgN7DSBdieycVlBeQtrA"

echo "Testing SKU API with verbose output..."
echo ""

curl -v -X POST "http://localhost:8000/SKU/SelectAllSKUDetails" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"pageNumber": 0, "pageSize": 20, "sortCriterias": [], "filterCriterias": []}'

echo ""
echo ""
echo "Testing GetSKUByUID..."
curl -v -X GET "http://localhost:8000/SKU/GetSKUByUID?UID=EMBT3001" \
  -H "Authorization: Bearer $TOKEN"
