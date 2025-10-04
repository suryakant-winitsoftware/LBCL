using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;

namespace Winit.Modules.ApprovalEngine.BL.Classes
{

    //public List<ISelectionItem> OrgUIDs { get; set; } 
    public abstract class ApprovalEngineBaseViewModel : IApprovalEngineView
    {
        public int TotalChangeRequests { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        protected PagingRequest PagingRequest = new PagingRequest()
        {
            IsCountRequired = true,
            FilterCriterias = []
        };
        public List<string> ApprovalRoleCodes { get; set; } = new List<string>();
        public Dictionary<int, string> UserLevelRoleMap { get; set; } = new Dictionary<int, string>();
        //public bool CanReAssign { get; set; } = true;
        public string ApiURL { get; set; }
        public ApprovalStatus? ApprovalHierarchyData { get; set; }
        public List<int> ReAssignOptions { get; set; }
        public List<ISelectionItem> RoleCode_NameKVP { get; set; }
        public IApprovalRuleMapping ApprovalRuleMapping { get; set; }
        public Dictionary<string, List<EmployeeDetail>> ApprovalUserCodes { get; set; }
        public Winit.Modules.ApprovalEngine.Model.Interfaces.IAllApprovalRequest AllApprovalRequestData { get; set; }
        public List<IViewChangeRequestApproval> ViewChangeRequestApprovals { get; set; }
        public IViewChangeRequestApproval DisplayChangeRequestApproval { get; set; }
        public List<ISelectionItem> Type { get; set; }
        public List<ISelectionItem> TypeCode { get; set; }
        public List<ISelectionItem> RuleIDs { get; set; }

        public List<Winit.Modules.Store.Model.Interfaces.IChangeRecord> ChangeRecords { get; set; }
        public List<ChangeRecordDTO> ChangeRecordDTOs { get; set; }
        public List<IApprovalRuleMap> RuleMap { get; set; }
        public IServiceProvider _serviceProvider;
        protected readonly IAppUser _appUser;
        public Winit.Shared.Models.Common.IAppConfig _appConfigs;
        public Winit.Modules.Base.BL.ApiService _apiService;

        public ApprovalEngineBaseViewModel(IServiceProvider serviceProvider,
            IAppUser appUser,
            Shared.Models.Common.IAppConfig appConfigs,
            Base.BL.ApiService apiService)
        {
            _serviceProvider = serviceProvider;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;


            Type =new List<ISelectionItem>();
            TypeCode=new List<ISelectionItem>();
            RuleIDs=new List<ISelectionItem>();
            ApprovalRuleMapping= new ApprovalRuleMapping();
            SortCriterias = new List<SortCriteria>();
        }
        // public abstract Task GetApprovalLog(string requestId);
        // public abstract Task GetRoleNames();
        public abstract Task<bool> SendNotificationToNextApprover(List<string> listOfNextApprovers);
        public abstract Task DropDownsForApprovalMapping();
        public abstract Task<bool> IntegrateRule();
        public abstract void PrepareApprovalRuleMap();
        public abstract Task GetAllChangeRequestDataAsync();
        public abstract Task GetChangeRequestDataByUIDAsync(string UID);
        public abstract Task<bool> UpdateApprovalStatus(ApprovalStatusUpdate approvalStatusUpdate);
        public abstract Task GetRequestId(string UID);
        public abstract Task<ApiResponse<string>> UpdateChangesInMainTable();
        public abstract Task FetchApprovalHierarchyStatus(string requestId);
        public abstract Task<bool> DeleteAllApprovalRequest(string requestId);
        public abstract Task<bool> Reassign(string selectedOption, string remark, int approverLevel, string requestId, string approverType);
        public abstract Task OnFilterApply(Dictionary<string, string> filterCriteria);
        public abstract Task ApplySort(SortCriteria sortCriteria);
        public abstract Task PageIndexChanged(int pageNumber);

    }

}
