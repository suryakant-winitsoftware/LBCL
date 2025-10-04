using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Provisioning.BL.Classes
{
    public class ProvisioningHeaderViewBL : Interfaces.IProvisioningHeaderViewBL
    {
        protected readonly DL.Interfaces.IProvisioningHeaderViewDL _provisioningHeaderViewDL;
        public ProvisioningHeaderViewBL(DL.Interfaces.IProvisioningHeaderViewDL provisioningHeaderViewDL)
        {
            _provisioningHeaderViewDL = provisioningHeaderViewDL;
        }
        public async Task<Winit.Modules.Provisioning.Model.Interfaces.IProvisionHeaderView?> GetProvisioningHeaderViewByUID(string UID)
        {
            return await _provisioningHeaderViewDL.GetProvisioningHeaderViewByUID(UID);
        }
        

        // Provision Approval
        public async Task<PagedResponse<IProvisionApproval>> GetProvisionApprovalSummaryDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _provisioningHeaderViewDL.GetProvisionApprovalSummaryDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<IProvisionApproval>> GetProvisionApprovalDetailViewDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _provisioningHeaderViewDL.GetProvisionApprovalDetailViewDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<IProvisionApproval>> GetProvisionRequestHistoryDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _provisioningHeaderViewDL.GetProvisionRequestHistoryDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<int> InsertProvisionRequestHistory(List<IProvisionApproval> provisionApproval)
        {
            return await _provisioningHeaderViewDL.InsertProvisionRequestHistory(provisionApproval);
        }
        public async Task<int> UpdateProvisionData(List<string> uIDs)
        {
            return await _provisioningHeaderViewDL.UpdateProvisionData(uIDs);
        }
        public async Task<IProvisionApproval> SelectProvisionByUID(string UID)
        {
            return await _provisioningHeaderViewDL.SelectProvisionByUID(UID);
        }
        public async Task<List<IProvisionApproval>> SelectProvisionRequestHistoryByProvisionIds(List<string> ProvisionIds)
        {
            return await _provisioningHeaderViewDL.SelectProvisionRequestHistoryByProvisionIds(ProvisionIds);
        }
    }
}
