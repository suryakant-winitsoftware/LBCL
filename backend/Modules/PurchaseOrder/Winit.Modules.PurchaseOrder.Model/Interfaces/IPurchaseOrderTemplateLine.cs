using Winit.Modules.Base.Model;

namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IPurchaseOrderTemplateLine:IBaseModel
{
    public string PurchaseOrderTemplateHeaderUID { get; set; }
    public int LineNumber { get; set; }
    public string SKUUID { get; set; }
    public string SKUCode { get; set; }
    public string UOM { get; set; }
    public decimal Qty { get; set; }
    int? inputQty { get; set; }
}
