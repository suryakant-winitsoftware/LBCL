using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Provisioning.DL.Interfaces
{
    public interface IProvisioningHeaderViewDL
    {
        Task<IProvisionHeaderView?> GetProvisioningHeaderViewByUID(string UID);

        // Provision Approval
        Task<PagedResponse<IProvisionApproval>> GetProvisionApprovalSummaryDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        
        Task<PagedResponse<IProvisionApproval>> GetProvisionApprovalDetailViewDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        
        Task<PagedResponse<IProvisionApproval>> GetProvisionRequestHistoryDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        //Task<Winit.Modules.Provisioning.Model.Interfaces.IProvisionHeaderView> GetBroadClassificationHeaderDetailsByUID(string UID);
        //Task<int> CreateBroadClassificationHeader(Winit.Modules.Provisioning.Model.Interfaces.IProvisionHeaderView broadClassificationHeader);
        //Task<int> UpdateBroadClassificationHeader(Winit.Modules.Provisioning.Model.Interfaces.IProvisionHeaderView broadClassificationHeader);
        //Task<int> DeleteBroadClassificationHeader(String UID);
        Task<int> InsertProvisionRequestHistory(List<IProvisionApproval> provisionApproval);
        Task<int> UpdateProvisionData(List<string> uIDs);
        Task<IProvisionApproval> SelectProvisionByUID(string UID);
        Task<List<IProvisionApproval>> SelectProvisionRequestHistoryByProvisionIds(List<string> ProvisionIds);
    }
}
