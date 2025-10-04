using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Provisioning.BL.Interfaces
{
    public interface IProvisioningHeaderViewViewModel
    {
        public List<ISelectionItem> ChannelPartnerList { get; set; }
        public IProvisionHeaderView ProvisionHeaderViewDetails { get; set; }
        Task GetProvisioningHeaderViewData(string? UID);
        Task PopulateHeaderViewModel();

        // Provision Approval
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public string TabName { get; set; }
        public List<ISelectionItem> BranchDetails { get; set; }
        public List<ISelectionItem> SalesOfficeDetails { get; set; }
        public List<ISelectionItem> BroadClassificationDetails { get; set; }
        public List<ISelectionItem> ChannelPartnerDetails { get; set; }
        public List<ISelectionItem> ProvisionTypeDetails { get; set; }
        public List<FilterCriteria> FilterCriterias { get; set; }
        public List<IProvisionApproval> SummaryViewDetails { get; set; }
        public List<IProvisionApproval> DetailViewDetails { get; set; }
        public List<IProvisionApproval> ProvisionRequestHistoryDetails { get; set; }
        Task PopulateProvisionFilter();
        Task PopulateProvisionApproval(string Provision);
        Task PopulateProvisionRequestHistory(List<string> ProvisionIds);
        Task OnFilterApply(Dictionary<string, string> keyValuePairs, string View);
        Task PageIndexChanged(int pageNumber, string View);
        Task DataSource(string Status, string View);
        Task<bool> UpdateStatus(List<IProvisionApproval> provisionApprovals, string View);
        Task<bool> InsertProvisionRequestHistoryDetails(List<IProvisionApproval> provisionApproval);
    }
}
