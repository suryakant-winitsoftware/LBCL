namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class PurchaseOrderSchemeSelection
{
    public bool IsSellInCnP1UnitValueSelected { get; set; } = true;
    public bool IsSellInP2AmountSelected { get; set; } = true;
    public bool IsSellInP3AmountSelected { get; set; } = true;
    public bool IsCashDiscountValueSelected { get; set; } = true;
    public bool IsP2QPSTotalValueSelected { get; set; } = true;
    public bool IsP3QPSTotalValueSelected { get; set; } = true;
    public List<string> UnselectedStandingScheme { get; set; } = new List<string>();
    
}
