using Winit.Modules.PriceLadder.Model.Interfaces;

namespace Winit.Modules.PriceLadder.Model.Classes;
public class SKUPriceLadderingData : ISKUPriceLadderingData
{
    public int ProductCategoryId { get; set; }
    public decimal PercentageDiscount { get; set; }
}
