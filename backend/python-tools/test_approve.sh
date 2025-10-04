#!/bin/bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTk0OTY1MjEsImlzcyI6Im15aXNzdWVyIn0.r-qSi8dcoPooe-EUeI-MwMdYc0ESE0Tac6kRWbLVFpg"

echo "=== Testing UpdateChangesInMainTable ==="
curl -s -X PUT "https://multiplex-promotions-api.winitsoftware.com/api/ApprovalEngine/UpdateChangesInMainTable" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "UID": "SUCCESS_TEST_1758196090",
    "EmpUid": "EMP001",
    "LinkedItemType": "Store",
    "LinkedItemUid": "STORE_SUCCESS_001",
    "RequestDate": "2025-09-18T11:48:10",
    "ApprovedDate": "2025-10-03T00:00:00",
    "Status": "Approved",
    "ChangedRecord": "[{\"LinkedItemUID\":\"STORE_SUCCESS_001\",\"Action\":\"update\",\"ScreenModelName\":\"Store\",\"UID\":\"STORE_SUCCESS_001\",\"ChangeRecords\":[{\"FieldName\":\"name\",\"OldValue\":\"Old Store Name\",\"NewValue\":\"New Store Name\"}]}]",
    "RowRecognizer": "",
    "ChannelPartnerCode": "",
    "ChannelPartnerName": "",
    "RequestedBy": "EMP001",
    "OperationType": "",
    "Reference": ""
  }' | python3 -m json.tool
