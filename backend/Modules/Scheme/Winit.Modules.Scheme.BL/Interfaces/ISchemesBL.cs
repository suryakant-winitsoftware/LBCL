using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ISchemesBL
    {
        Task<PagedResponse<IManageScheme>> SelectAllSchemes(
      List<SortCriteria> sortCriterias,
      int pageNumber,
      int pageSize,
      List<FilterCriteria> filterCriterias,
      bool isCountRequired, string jobPositionUid, bool isAdmin);
        Task<List<IStore>> GetChannelPartner(string jobPositionUID, bool isAdmin);
        Task<bool> UpdateApproval(ApprovalStatusUpdate approvalStatusUpdate);
        Task<bool> CreateApproval(string linkedItemUID, string linkedItemType, ApprovalRequestItem approvalRequestItem);
        Task<List<ISchemeExtendHistory>> GetschemeExtendHistoryBySchemeUID(string schemeUID);
        Task<int> InsertSchemeExtendHistory(ISchemeExtendHistory schemeExtendHistory);
        Task<int> UpdateSchemeCustomerMappingData(string schemeType, string schemeUID, DateTime newEndDate, bool isActive);
    }
}
