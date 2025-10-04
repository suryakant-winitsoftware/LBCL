using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Provisioning.DL.Interfaces;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Provisioning.DL.Classes
{
    public class PGSQLProvisioningHeaderViewDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IProvisioningHeaderViewDL
    {
        public PGSQLProvisioningHeaderViewDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public Task<IProvisionHeaderView?> GetProvisioningHeaderViewByUID(string UID)
        {
            throw new NotImplementedException();
        }

        Task<PagedResponse<IProvisionApproval>> IProvisioningHeaderViewDL.GetProvisionApprovalDetailViewDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }

        Task<PagedResponse<IProvisionApproval>> IProvisioningHeaderViewDL.GetProvisionApprovalSummaryDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }

        Task<PagedResponse<IProvisionApproval>> IProvisioningHeaderViewDL.GetProvisionRequestHistoryDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }

       

        Task<int> IProvisioningHeaderViewDL.InsertProvisionRequestHistory(List<IProvisionApproval> provisionApproval)
        {
            throw new NotImplementedException();
        }

        Task<IProvisionApproval> IProvisioningHeaderViewDL.SelectProvisionByUID(string UID)
        {
            throw new NotImplementedException();
        }

        Task<List<IProvisionApproval>> IProvisioningHeaderViewDL.SelectProvisionRequestHistoryByProvisionIds(List<string> ProvisionIds)
        {
            throw new NotImplementedException();
        }

        Task<int> IProvisioningHeaderViewDL.UpdateProvisionData(List<string> uIDs)
        {
            throw new NotImplementedException();
        }
    }
}
