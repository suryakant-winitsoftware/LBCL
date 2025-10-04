using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SchemesBL : ISchemesBL
    {
        ISchemesDL _schemesDL;
        public SchemesBL(ISchemesDL schemesDL)
        {
            _schemesDL = schemesDL;
        }
        public async Task<PagedResponse<IManageScheme>> SelectAllSchemes(
       List<SortCriteria> sortCriterias,
       int pageNumber,
       int pageSize,
       List<FilterCriteria> filterCriterias,
       bool isCountRequired, string jobPositionUid, bool isAdmin)
        {
            return await _schemesDL.SelectAllSchemes(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, jobPositionUid, isAdmin);
        }
        public async Task<List<IStore>> GetChannelPartner(string jobPositionUID, bool isAdmin)
        {
            return await _schemesDL.GetChannelPartner(jobPositionUID, isAdmin);
        }
        public async Task<bool> CreateApproval(string linkedItemUID, string linkedItemType, ApprovalRequestItem approvalRequestItem)
        {
            return await _schemesDL.CreateApproval(linkedItemType: linkedItemType, linkedItemUID: linkedItemUID, approvalRequestItem: approvalRequestItem);
        }
        public async Task<bool> UpdateApproval(ApprovalStatusUpdate approvalStatusUpdate)
        {
            return await _schemesDL.UpdateApproval(approvalStatusUpdate);
        }
        public async Task<List<ISchemeExtendHistory>> GetschemeExtendHistoryBySchemeUID(string schemeUID)
        {
            return await _schemesDL.GetschemeExtendHistoryBySchemeUID(schemeUID);

        }
        public async Task<int> InsertSchemeExtendHistory(ISchemeExtendHistory schemeExtendHistory)
        {
            return await _schemesDL.InsertSchemeExtendHistory(schemeExtendHistory);

        }

        public async Task<int> UpdateSchemeCustomerMappingData(string schemeType, string schemeUID, DateTime newEndDate, bool isActive)
        {
            return await _schemesDL.UpdateSchemeCustomerMappingData(schemeType, schemeUID, newEndDate, isActive);
        }
    }
}
