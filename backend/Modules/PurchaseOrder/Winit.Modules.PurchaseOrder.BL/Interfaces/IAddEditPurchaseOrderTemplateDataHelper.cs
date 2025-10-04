using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IAddEditPurchaseOrderTemplateDataHelper
{
    Task<List<ISKUV1>> GetAllSKUs(PagingRequest pagingRequest);
    Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string? skuGroupTypeUID, string? parentUID);
    Task<List<SKUAttributeDropdownModel>> GetSKUAttributeDropDownData();
    Task<List<ISelectionItem>?> GetProductOrgSelectionItems();
    Task<List<ISelectionItem>?> GetProductDivisionSelectionItems();
    Task<List<ISKUMaster>> GetSKUsMasterBySKUUIDs(SKUMasterRequest sKUMasterRequest);
    Task<IPurchaseOrderTemplateMaster?> GetPOTemplateMasterByUID(string purchaseOrderTemplateHeaderUID);
    Task<bool> CUD_POTemplate(IPurchaseOrderTemplateMaster purchaseOrderTemplateMaster);
    Task<bool> DeletePurchaseOrderTemplateLinesByUIDs(List<string> purchaseOrderTemplateLineUids);
}
