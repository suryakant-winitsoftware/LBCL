using System.Data;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Interfaces;

public interface ISKUDL
{
    Task<PagedResponse<ISKU>> SelectAllSKUDetails(List<SortCriteria> sortCriterias,int pageNumber, int pageSize, 
        List<FilterCriteria> filterCriterias, bool isCountRequired);
    Task<ISKU> SelectSKUByUID(string UID);
    Task<int> CreateSKU(ISKUV1 sKU);
    Task<int> UpdateSKU(ISKU sKU);
    Task<int> DeleteSKU(string UID);
    Task<(List<ISKU>?, List<ISKUConfig>?, List<ISKUUOM>?, List<ISKUAttributes>?, List<ITaxSkuMap>?)> PrepareSKUMaster
        (List<string> orgUIDs,List<string> DistributionChannelUIDs, List<string> skuUIDs, List<string> attributeTypes);
    Task<PagedResponse<ISKUListView>> SelectAllSKUDetailsWebView(List<SortCriteria> sortCriterias, int pageNumber,int pageSize,
        List<FilterCriteria> filterCriterias, bool isCountRequired);
    Task<(List<ISKU>, List<ISKUConfig>, List<ISKUUOM>,List<ISKUAttributes>, List<Winit.Modules.CustomSKUField.Model.
        Interfaces.ICustomSKUFields>,List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>)> SelectSKUMasterByUID(string UID);
    Task<int> CRUDWinitCache(string key, string value, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<List<ISKUMaster>> GetWinitCache(string key, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<Dictionary<string, List<string>>> GetLinkedItemUIDByStore(string linkedItemType, List<string> storeUIDs);
}
