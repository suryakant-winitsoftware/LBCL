using Winit.Modules.SKU.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes;

public class SKUGroupBL : ISKUGroupBL
{
    protected readonly DL.Interfaces.ISKUGroupDL _skuGroupDL;
    public SKUGroupBL(DL.Interfaces.ISKUGroupDL skuGroupDL)
    {
        _skuGroupDL = skuGroupDL;
    }
    public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> SelectAllSKUGroupDetails(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        return await _skuGroupDL.SelectAllSKUGroupDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
    }
    public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUGroup> SelectSKUGroupByUID(string UID)
    {
        return await _skuGroupDL.SelectSKUGroupByUID(UID);
    }
    public async Task<int> CreateSKUGroup(Winit.Modules.SKU.Model.Interfaces.ISKUGroup sKUGroup)
    {
        return await _skuGroupDL.CreateSKUGroup(sKUGroup);
    }
    public async Task<int> InsertSKUGroupHierarchy(string type, string uid)
    {
        return await _skuGroupDL.InsertSKUGroupHierarchy(type, uid);
    }

    public async Task<int> UpdateSKUGroup(Winit.Modules.SKU.Model.Interfaces.ISKUGroup sKUGroup)
    {
        return await _skuGroupDL.UpdateSKUGroup(sKUGroup);
    }

    public async Task<int> DeleteSKUGroup(string UID)
    {
        return await _skuGroupDL.DeleteSKUGroup(UID);
    }
    public async Task<IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupView>> SelectSKUGroupView()
    {
        return await _skuGroupDL.SelectSKUGroupView();
    }
    public async Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string skuGroupTypeUID, string parentUID)
    {
        return await _skuGroupDL.GetSKUGroupSelectionItemBySKUGroupTypeUID(skuGroupTypeUID, parentUID);
    }
    public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> GetSKUGroupBySKUGroupTypeUID(string skuGroupTypeUID)
    {
        return await _skuGroupDL.GetSKUGroupBySKUGroupTypeUID(skuGroupTypeUID);
    }
    public async Task<List<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView>> SelectAllSKUGroupItemViews(List<SortCriteria>
        sortCriterias, List<FilterCriteria> filterCriterias)
    {
        return await _skuGroupDL.SelectAllSKUGroupItemViews(sortCriterias, filterCriterias);
    }
}
