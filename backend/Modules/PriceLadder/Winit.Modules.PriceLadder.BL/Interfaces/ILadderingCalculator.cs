using Winit.Modules.PriceLadder.Model.Interfaces;

namespace Winit.Modules.PriceLadder.BL.Interfaces;

public interface ILadderingCalculator
{
    Task<List<ISKUPriceLadderingData>> ApplyPriceLaddering(
        string broadCustomerClassification, DateTime date, List<int>? productCategoryIds = null);
}
