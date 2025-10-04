using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ProvisionComparisonReport.BL.Interfaces;
using Winit.Modules.ProvisionComparisonReport.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ProvisionComparisonReport.BL.Classes
{
    public abstract class ProvisionComparisonReportsBaseViewModel : IProvisionComparisonReportsView
    {
        public readonly IAppConfig _appConfigs;
        public readonly ApiService _apiService;
        protected readonly IAppUser _appUser;
        public List<IProvisionComparisonReportView> ProvisionsComparisonReportView { get; set; }
        public List<IProvisionComparisonReportView> ProvisionsComparisonReportViewInExportExcel { get; set; }
        protected PagingRequest PagingRequest = new PagingRequest()
        {
            FilterCriterias = [],
            IsCountRequired=true
        };
        public int TotalProvisionComparisonReportRecord { get; set; }
        public List<SortCriteria> SortCriterias = new List<SortCriteria>();
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public ProvisionComparisonReportsBaseViewModel(Winit.Shared.Models.Common.IAppConfig appConfigs,
                ApiService apiService, IAppUser appUser)
        {
            this._appConfigs = appConfigs;
            this._apiService = apiService;
            _appUser=appUser;
            PagingRequest.PageNumber = PageNumber;
            PagingRequest.PageSize = PageSize;
            PagingRequest.SortCriterias = new List<SortCriteria>();
        }
        public abstract Task GetAllProvisionComparisonReport();
        public abstract Task ExportProvisionComparisonReportBasedOnFilter();
        public abstract Task<List<ISelectionItem>> GetChannelPartnerDDLValues();
        public abstract Task<List<ISelectionItem>> GetBroadClassificationDDLValues();
        public abstract Task<List<ISelectionItem>> GetBranchDDLValues();
        public abstract Task<List<ISelectionItem>> GetSalesOfficeDDLValues();
        public abstract Task<List<ISelectionItem>> GetProvisionTypeDDLValues();
        public abstract Task ApplySort(SortCriteria sortCriteria);
        public abstract Task OnFilterApply(Dictionary<string, string> filterCriteria);
        public abstract Task PageIndexChanged(int pageNumber);
    }
}
