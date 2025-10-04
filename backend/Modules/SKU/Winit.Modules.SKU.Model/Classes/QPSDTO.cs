using Winit.Modules.SKU.Model.Interfaces;
namespace Winit.Modules.SKU.Model.Classes;

public class QPSDTO:IQPSDTO
{
    public string SchemeCode { get; set; }
    public string SchemeUID { get; set; }
    public string SKUUID { get; set; }
    public decimal MinQty { get; set; }
    public decimal MaxQty { get; set; }
    public string OfferType { get; set; }
    public decimal OfferValue { get; set; }
}
