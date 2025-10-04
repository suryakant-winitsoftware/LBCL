using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Interfaces;

public interface ISKUBL 
{
    Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKU>> SelectAllSKUDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    Task<Winit.Modules.SKU.Model.Interfaces.ISKU> SelectSKUByUID(string UID);

    Task<int> CreateSKU(Winit.Modules.SKU.Model.Interfaces.ISKUV1 sKU);
    Task<int> UpdateSKU(Winit.Modules.SKU.Model.Interfaces.ISKU sKU);
    Task<int> DeleteSKU(string UID);
    Task<List<Model.Interfaces.ISKUMaster>> PrepareSKUMaster(List<string> orgUIDs, List<string> DistributionChannelUIDs,
        List<string> skuUIDs, List<string> attributeTypes);
    Task<PagedResponse<ISKUListView>> SelectAllSKUDetailsWebView(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    Task<Model.Interfaces.ISKUMaster> SelectSKUMasterByUID(string UID);
    //niranjan for testing
    List<string> GetMethodExecutionTimesfromSkuBl();
    Task<int> CRUDWinitCache(string key, string value, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<List<ISKUMaster>> GetWinitCache(string key, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<Dictionary<string, List<string>>> GetLinkedItemUIDByStore(string linkedItemType, List<string> storeUIDs);
}
