using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ISchemeApprovalEngineBaseViewModel
    {
        void SetUserTypeWhileCreatingScheme();
        Task PopulateApprovalEngine(string linkedItemUID);
        List<IAllApprovalRequest> AllApprovalLevelList { get; set; }
        int RequestId { get; set; }
        int RuleId { get; set; }
        string UserType { get; set; }
        bool IsReassignButtonNeeded { get; set; }
        public Dictionary<string, List<EmployeeDetail>>? ApprovalUserCodes { get; set; }
        string UserRoleCode { get; set; }
        //Task<bool> SaveApprovalRequestDetails(string requestId, string linkedItemUID, string linkedItemType, string userHierarchyType, string hierarchyUID);
        Task<List<IAllApprovalRequest>> GetAllApproveListDetails(string UID);
        void GetUserTypeWhileCreatingScheme(bool isPositiveScheme, out string userType, out int ruleId);

    }
}
