using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Interfaces;

public interface ISKUGroupDL
{
    Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> SelectAllSKUGroupDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    Task<Winit.Modules.SKU.Model.Interfaces.ISKUGroup> SelectSKUGroupByUID(string UID);
    Task<int> CreateSKUGroup(Winit.Modules.SKU.Model.Interfaces.ISKUGroup sKU);
    Task<int> InsertSKUGroupHierarchy(string type, string uid);
    Task<int> UpdateSKUGroup(Winit.Modules.SKU.Model.Interfaces.ISKUGroup sKU);
    Task<int> DeleteSKUGroup(string UID);
    Task<IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupView>> SelectSKUGroupView();
    Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string skuGroupTypeUID, string parentUID);
    Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> GetSKUGroupBySKUGroupTypeUID(string skuGroupTypeUID);
    Task<List<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView>> SelectAllSKUGroupItemViews(List<SortCriteria>
    sortCriterias, List<FilterCriteria> filterCriterias);
}
