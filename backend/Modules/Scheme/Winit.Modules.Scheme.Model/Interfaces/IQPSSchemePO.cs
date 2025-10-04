namespace Winit.Modules.Scheme.Model.Interfaces;

public interface IQPSSchemePO
{
    string Scheme_UID { get; set; }
    string Scheme_Code { get; set; }
    string Store_UID { get; set; }
    string SKU_UID { get; set; }
    string Offer_Type { get; set; }
    decimal Offer_Value { get; set; }
    decimal MinQty { get; set; }
    decimal MaxQty { get; set; }
    string Keyword { get; set; }
    DateTime ModifiedTime { get; set; }
}
