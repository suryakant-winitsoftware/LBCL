using Winit.Modules.Base.Model;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class PurchaseOrderTemplateLine : BaseModel, IPurchaseOrderTemplateLine
{
    public string PurchaseOrderTemplateHeaderUID { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string SKUUID { get; set; } = string.Empty;
    public string SKUCode { get; set; } = string.Empty;
    public string UOM { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public int? inputQty
    {
        get
        {
            return (int?)Qty;
        }
        set
        {
            Qty = value.GetValueOrDefault();
        }
    }
}
