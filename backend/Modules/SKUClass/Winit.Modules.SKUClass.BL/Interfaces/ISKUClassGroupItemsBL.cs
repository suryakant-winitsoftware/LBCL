using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.BL.Interfaces
{
    public interface ISKUClassGroupItemsBL
    {
        Task<PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>> SelectAllSKUClassGroupItemsDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems> GetSKUClassGroupItemsByUID(string UID);
        Task<int> CreateSKUClassGroupItems(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems createSKUClassGroupItems);
        Task<int> UpdateSKUClassGroupItems(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems updateSKUClassGroupItems);
        Task<int> DeleteSKUClassGroupItems(string UID);
        Task<PagedResponse<ISKUClassGroupItemView>> SelectAllSKUClassGroupItemView(List<SortCriteria> sortCriterias, int pageNumber,
                      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<IEnumerable<ISKUClassGroupItems>> PrepareSkuClassForCache(List<string> linkedItemUIDs);
        Task<List<string>> GetApplicableAllowedSKUGroupUIDs(string storeUID);
        Task<List<string>> GetApplicableAllowedSKUGBySKUClassGroupUID(string skuClassGroupUID);
    }
}
