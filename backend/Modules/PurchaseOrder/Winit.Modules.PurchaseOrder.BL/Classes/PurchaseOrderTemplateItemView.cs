using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.SKU.Model.UIInterfaces;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class PurchaseOrderTemplateItemView : PurchaseOrderTemplateLine, IPurchaseOrderTemplateItemView
{
    public string? SKUName { get; set; }
    public string? BaseUOM { get; set; }
    public string? L2 { get; set; }
    public string? ModelCode { get; set; }
    public bool IsSelected { get; set; }
    public ISKUUOMView? SelectedUOM { get; set; }
    public string ModelDescription { get; set; } = string.Empty;
    public HashSet<string> FilterKeys { get; set; } = new HashSet<string>();
}
