using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKUClass.BL.Interfaces;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.BL.Classes;

public class SKUClassGroupItemsBL : ISKUClassGroupItemsBL
{
    protected readonly DL.Interfaces.ISKUClassGroupItemsDL _skuClassGroupItemsDL = null;
    public SKUClassGroupItemsBL(DL.Interfaces.ISKUClassGroupItemsDL skuClassGroupItemsBL)
    {
        _skuClassGroupItemsDL = skuClassGroupItemsBL;
    }
    public async Task<PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>> SelectAllSKUClassGroupItemsDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        return await _skuClassGroupItemsDL.SelectAllSKUClassGroupItemsDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
    }
    public async Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems> GetSKUClassGroupItemsByUID(string UID)
    {
        return await _skuClassGroupItemsDL.GetSKUClassGroupItemsByUID(UID);
    }
    public async Task<int> CreateSKUClassGroupItems(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems createSKUClassGroupItems)
    {
        return await _skuClassGroupItemsDL.CreateSKUClassGroupItems(createSKUClassGroupItems);
    }
    public async Task<int> UpdateSKUClassGroupItems(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems updateSKUClassGroupItems)
    {
        return await _skuClassGroupItemsDL.UpdateSKUClassGroupItems(updateSKUClassGroupItems);
    }
    public async Task<int> DeleteSKUClassGroupItems(string UID)
    {
        return await _skuClassGroupItemsDL.DeleteSKUClassGroupItems(UID);
    }
    public async Task<PagedResponse<ISKUClassGroupItemView>> SelectAllSKUClassGroupItemView(
        List<SortCriteria> sortCriterias, int pageNumber,
                  int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        return await _skuClassGroupItemsDL.SelectAllSKUClassGroupItemView(sortCriterias, pageNumber, pageSize,
            filterCriterias, isCountRequired);
    }
    public async Task<IEnumerable<ISKUClassGroupItems>> PrepareSkuClassForCache(List<string> linkedItemUIDs)
    {
        return await _skuClassGroupItemsDL.PrepareSkuClassForCache(linkedItemUIDs);
    }
    public async Task<List<string>> GetApplicableAllowedSKUGroupUIDs(string storeUID)
    {
        return await _skuClassGroupItemsDL.GetApplicableAllowedSKUGroupUIDs(storeUID);
    }
    public async Task<List<string>> GetApplicableAllowedSKUGBySKUClassGroupUID(string skuClassGroupUID)
    {
        return await _skuClassGroupItemsDL.GetApplicableAllowedSKUGBySKUClassGroupUID(skuClassGroupUID);
    }
}

