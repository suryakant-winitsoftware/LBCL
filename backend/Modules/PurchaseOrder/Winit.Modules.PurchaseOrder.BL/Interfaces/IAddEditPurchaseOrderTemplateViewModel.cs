using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IAddEditPurchaseOrderTemplateViewModel
{
    bool IsAdd { get; set; }
    List<ISKUV1> SKUs { get; set; }
    public List<IPurchaseOrderTemplateItemView> PurchaseOrderTemplateItemViews { get; set; }
    public List<IPurchaseOrderTemplateItemView> FilteredPurchaseOrderTemplateItemViews { get; set; }
    public string ProductSearchString { get; set; }
    public List<ISelectionItem> OrganisationUnitSelectionItems { get; set; }
    public List<ISelectionItem> DivisionSelectionItems { get; set; }
    public List<ISelectionItem> ProductCategorySelectionItems { get; set; }
    List<SKUAttributeDropdownModel> SKUAttributeData { get; }
    public IPurchaseOrderTemplateMaster PurchaseOrderTemplateMaster { get; set; }
    Task PopulateViewModel(string uid);
    Task AddProductsToGridBySKUUIDs(List<string> sKUs);
    Task<List<ISelectionItem>> OnSKuAttributeDropdownValueSelect(string selectedItemUID);
    void ApplyGridFilter();
    void Validate();
    Task OnSaveOrUpdateClick();
    Task OnDeleteSelectedItems();
}
