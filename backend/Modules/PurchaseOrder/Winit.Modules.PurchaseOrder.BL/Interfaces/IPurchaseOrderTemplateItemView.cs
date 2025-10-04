using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IPurchaseOrderTemplateItemView : IPurchaseOrderTemplateLine
{
    string? SKUName { get; set; }
    string? BaseUOM { get; set; }
    bool IsSelected { get; set; }
    string? L2 { get; set; }
    string? ModelCode { get; set; }
    public string ModelDescription { get; set; }
    ISKUUOMView? SelectedUOM { get; set; }
    HashSet<string> FilterKeys { get; set; }
}
