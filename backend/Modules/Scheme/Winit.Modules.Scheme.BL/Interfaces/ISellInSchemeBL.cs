using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ISellInSchemeBL
    {
        Task<PagedResponse<ISellInSchemeLine>> SelectAllSellInDetails(List<SortCriteria> sortCriterias, int pageNumber,
                                                                               int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<ISellInSchemeHeader>> SelectAllSellInHeader(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> DeleteSellInDetail(string UID);
        Task<int> UpdateSellInDetail(ISellInSchemeLine sellInDetail);
        Task<int> CreateSellInDetail(ISellInSchemeLine sellInDetail);
        Task<int> DeleteSellInHeader(string UID);
        Task<int> UpdateSellInHeader(ISellInSchemeHeader SellInHeader);
        Task<int> CreateSellInHeader(ISellInSchemeHeader SellInHeader);
        Task<ISellInSchemeHeader> GetSellInHeaderByUID(string UID);
        Task<ISellInSchemeLine> GetSellInDetailByUID(string UID);
        Task<ISellInSchemeDTO> GetSellInMasterByHeaderUID(string UID);
        Task<List<ISellInSchemePO>> GetSellInSchemesByOrgUidAndSKUUid(string orgUid, List<string>? sKUUIDs = null);        
        Task<List<ISellInSchemePO>> GetExistSellInSchemesByPOUid(string POHeaderUID);        
        Task<int> CUSellInHeader(ISellInSchemeDTO sellInDTO);
        Task<ISellInSchemeDTO> GetSellInSchemeByOrgUid(string OrgUid, DateTime OrderDate);
        Task<List<IStandingProvision>> GetStandingProvisionAmountByChannelPartnerUID(string channelPartnerUID);
        Task<int> CreateSellInScheme(ISellInSchemeDTO sellInDTO);
    }
}
