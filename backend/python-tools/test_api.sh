#!/bin/bash

TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTkzMTk5MDgsImlzcyI6Im15aXNzdWVyIn0.Iz4ZHTWDU8cHxJN5Msf0ARhxDleMIGnP780d9ox5NPE"

echo "=========================================="
echo "Testing GetSelectionMapMasterByLinkedItemUID API"
echo "=========================================="
echo ""
echo "Test 1: Check for Price List 4094-PL (should find existing mapping)"
echo "URL: http://localhost:8000/api/Mapping/GetSelectionMapMasterByLinkedItemUID?linkedItemUID=4094-PL"
echo ""

curl -s "http://localhost:8000/api/Mapping/GetSelectionMapMasterByLinkedItemUID?linkedItemUID=4094-PL" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" | python3 -m json.tool

echo ""
echo "=========================================="
echo "Test 2: Check for CLASS_GROUP_1757387290583 (should find existing mapping)"
echo "URL: http://localhost:8000/api/Mapping/GetSelectionMapMasterByLinkedItemUID?linkedItemUID=CLASS_GROUP_1757387290583"
echo ""

curl -s "http://localhost:8000/api/Mapping/GetSelectionMapMasterByLinkedItemUID?linkedItemUID=CLASS_GROUP_1757387290583" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" | python3 -m json.tool

echo ""
echo "=========================================="
echo "Test 3: Check for non-existent item (should return 404 or null)"
echo "URL: http://localhost:8000/api/Mapping/GetSelectionMapMasterByLinkedItemUID?linkedItemUID=NONEXISTENT_12345"
echo ""

curl -s "http://localhost:8000/api/Mapping/GetSelectionMapMasterByLinkedItemUID?linkedItemUID=NONEXISTENT_12345" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" | python3 -m json.tool
