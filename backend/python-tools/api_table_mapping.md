# Approval Engine - API to Table Mapping

## Existing APIs:

1. **GetApprovalLog** - Returns: ApprovalLogs
2. **GetRoleNames** - Returns: Roles data
3. **GetRuleId** - Returns: RuleMaster data
4. **DropDownsForApprovalMapping** - Returns: Various dropdown data
5. **IntegrateRule** - Inserts: RuleMaster, RuleParameters, RuleApproverLevelCondition, ApprovalHierarchy
6. **GetAllChangeRequest** - Returns: Change_requests
7. **GetApprovalDetailsByLinkedItemUid** - Returns: AllApprovalRequest
8. **GetApprovalHierarchy** - Returns: ApprovalHierarchy
9. **GetChangeRequestDataByUid** - Returns: Change_requests (by UID)
10. **UpdateChangesInMainTable** - Updates: Change_requests + main table
11. **DeleteApprovalRequest** - Deletes: AllApprovalRequest
12. **GetApprovalRuleMasterData** - Returns: RuleMaster
13. **GetUserHierarchyForRule** - Returns: User hierarchy data
14. **GetChangeRequestData** - Returns: Change_requests

## Tables NOT covered by APIs:

- **approvalrequest** - Need API
- **approval_request_approver** - Need API
- **approvalnotification** - Need API
- **ruleactions** - Need API
- **wh_stock_available** - Need API (might be in different module)
- **int_pushed_data_status** - Need API (integration table)

## Tables PARTIALLY covered:

- **rulemaster** ✓ (GetApprovalRuleMasterData, GetRuleId)
- **ruleparameters** ✓ (via IntegrateRule)
- **ruleapproverlevelcondition** ✓ (via IntegrateRule)
- **approvalhierarchy** ✓ (GetApprovalHierarchy)
- **approvallogs** ✓ (GetApprovalLog)
- **Change_requests** ✓ (GetAllChangeRequest, GetChangeRequestDataByUid)
- **AllApprovalRequest** ✓ (GetApprovalDetailsByLinkedItemUid)
