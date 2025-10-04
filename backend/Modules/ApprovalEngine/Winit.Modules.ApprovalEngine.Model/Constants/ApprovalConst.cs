using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ApprovalEngine.Model.Constants
{
    public class ApprovalConst
    {
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Pending = "Pending";
        public const string Reassign = "Reassign";
        public const string Action = "doaction/";
        public const string RoleApprover = "role";
        public const string UserApprover = "user";
        public const string SendingReassign = "Reassigned";
        public const string GetApprovalStatus = "GetApprovalStatus/";
        public const string ReassignAction = "ReassignApprover/";
        public const string EvaluateRule = "EvaluateRule/";
        public const string GetApprovalHierarchyStatus = "GetApprovalHierarchyStatus/";
        public const string ApiUrl = "https://approvalengineapi-dev.winitsoftware.com/v1/api/ruleengine/";
       // public const string ApiUrl = "http://115.112.59.241/ApprovalAPI/V1/api/ruleengine/"; //use this when u are going to upload on client uat

        //// public const string ApiUrl = "https://approvalengineapi-dev.winitsoftware.com/v1/api/ruleengine/";
        //public const string ApiUrl = "http://carrieruat-approval-api.winitsoftware.com/api/ruleengine/"; //use this when u are going to upload on client uat

    }
}
