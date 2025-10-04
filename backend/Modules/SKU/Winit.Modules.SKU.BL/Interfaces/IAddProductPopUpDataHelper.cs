using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Interfaces;

public interface IAddProductPopUpDataHelper
{
    Task<List<ISKUV1>> GetAllSKUs(PagingRequest pagingRequest);
    Task<List<SKUAttributeDropdownModel>?> GetSKUAttributeData();
    Task<List<ISelectionItem>> OnSKuAttributeDropdownValueSelect(string selectedItemUID);
    bool FilterAction(List<FilterCriteria> filterCriterias, ISKU sku);

    /// <summary>
    /// added by prem
    /// </summary>
    /// <returns></returns>
    Task<List<ISelectionItem>?> GetProductDivisionSelectionItems();
    Task<List<ISKUGroup>> GetSKUGroup(PagingRequest pagingRequest);
    Task<List<ISKUGroupType>> GetSKUGroupType(PagingRequest pagingRequest);
}
