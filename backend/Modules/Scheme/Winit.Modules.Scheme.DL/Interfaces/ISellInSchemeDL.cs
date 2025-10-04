using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Interfaces
{
    public interface ISellInSchemeDL
    {
        Task<PagedResponse<ISellInSchemeLine>> SelectAllSellInDetails(List<SortCriteria> sortCriterias, int pageNumber,
                                                                                int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<ISellInSchemeHeader>> SelectAllSellInHeader(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> DeleteSellInDetail(string UID);
        Task<int> UpdateSellInDetail(ISellInSchemeLine sellInDetail);
        Task<int> CreateSellInDetail(ISellInSchemeLine sellInDetail);
        Task<int> CreateSellInDetail(List<ISellInSchemeLine> sellInDetail);
        Task<int> CUSellInHeader(ISellInSchemeDTO sellInDTO);
        Task<int> DeleteSellInHeader(string UID);
        Task<int> UpdateSellInHeader(ISellInSchemeHeader SellInHeader);
        Task<int> CreateSellInHeader(ISellInSchemeHeader SellInHeader);
        Task<ISellInSchemeHeader> GetSellInHeaderByUID(string UID);
        Task<ISellInSchemeLine> GetSellInDetailByUID(string UID);
        Task<List<ISellInSchemeLine>> GetSellInDetailByHeaderUID(string HeaderUID);
        Task<int> UpdateSellInMappingData(string schemeUID)
        {
            throw new NotImplementedException();
        }
        Task<ISellInSchemeDTO> GetSellInSchemeByOrgUid(string OrgUid, DateTime OrderDate)
        {
            throw new NotImplementedException();
        }
        Task<List<IStandingProvision>> GetStandingProvisionAmountByChannelPartnerUID(string channelPartnerUID)
        {
            throw new NotImplementedException();
        }
        Task<List<ISellInSchemePO>> GetSellInSchemesByOrgUidAndSKUUid(string orgUid, List<string>? sKUUIDs = null);        
        Task<List<ISellInSchemePO>> GetExistSellInSchemesByPOUid(string POHeaderUID)
        {
            throw new NotImplementedException();
        }        
        Task<int> UpdateSellInHeaderAfterFinalApproval(ISellInSchemeHeader SellInHeader)
        {
            throw new NotImplementedException();
        }

    }
}
