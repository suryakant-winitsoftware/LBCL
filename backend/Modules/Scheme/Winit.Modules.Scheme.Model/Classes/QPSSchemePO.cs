using Winit.Modules.Scheme.Model.Interfaces;
namespace Winit.Modules.Scheme.Model.Classes;

public class QPSSchemePO : IQPSSchemePO
{
    public string Scheme_UID { get; set; }
    public string Scheme_Code { get; set; }
    public string Store_UID { get; set; }
    public string SKU_UID { get; set; }
    public string Offer_Type { get; set; }
    public decimal Offer_Value { get; set; }
    public decimal MinQty { get; set; }
    public decimal MaxQty { get; set; }
    public string Keyword { get; set; }
    public DateTime ModifiedTime { get; set; }
    public QPSSchemePO()
    {

    }
    public QPSSchemePO(QPSSchemePO original)
    {
        Scheme_UID = original.Scheme_UID;
        Scheme_Code = original.Scheme_Code;
        Store_UID = original.Store_UID;
        SKU_UID = original.SKU_UID;
        Offer_Type = original.Offer_Type;
        Offer_Value = original.Offer_Value;
        MinQty = original.MinQty;
        MaxQty = original.MaxQty;
        Keyword = original.Keyword;
        ModifiedTime = original.ModifiedTime;
    }
}
