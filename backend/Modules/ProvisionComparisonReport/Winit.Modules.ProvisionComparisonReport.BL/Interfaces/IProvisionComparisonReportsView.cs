using Winit.Modules.ProvisionComparisonReport.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ProvisionComparisonReport.BL.Interfaces
{
    public interface IProvisionComparisonReportsView
    {
        public int TotalProvisionComparisonReportRecord { get; set; }
        public int PageSize { get; set; }
        public List<IProvisionComparisonReportView> ProvisionsComparisonReportView { get; set; }
        public List<IProvisionComparisonReportView> ProvisionsComparisonReportViewInExportExcel { get; set; }
        Task ExportProvisionComparisonReportBasedOnFilter();
        public int PageNumber { get; set; }
        Task GetAllProvisionComparisonReport();
        Task<List<ISelectionItem>> GetChannelPartnerDDLValues();
        Task<List<ISelectionItem>> GetBroadClassificationDDLValues();
        Task<List<ISelectionItem>> GetBranchDDLValues();
        Task<List<ISelectionItem>> GetSalesOfficeDDLValues();
        Task<List<ISelectionItem>> GetProvisionTypeDDLValues();
        Task ApplySort(SortCriteria sortCriteria);
        Task OnFilterApply(Dictionary<string, string> filterCriteria);
        Task PageIndexChanged(int pageNumber);
    }
}
