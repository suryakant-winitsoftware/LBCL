namespace Winit.Modules.SKU.Model.Interfaces;

public interface IQPSDTO
{
    string SchemeCode { get; set; }
    string SchemeUID { get; set; }
    string SKUUID { get; set; }
    decimal MinQty { get; set; }
    decimal MaxQty { get; set; }
    string OfferType { get; set; }
    decimal OfferValue { get; set; }
}
