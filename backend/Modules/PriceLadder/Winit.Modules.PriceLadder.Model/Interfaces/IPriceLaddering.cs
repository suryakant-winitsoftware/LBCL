using Winit.Modules.Base.Model;

namespace Winit.Modules.PriceLadder.Model.Interfaces
{
    public interface IPriceLaddering : IBaseModel
    {
        int? LadderingId { get; set; }
        string OperatingUnit { get; set; }
        string Division { get; set; }
        int? ProductCategoryId { get; set; }
        string Branch { get; set; }
        string BranchName { get; set; }
        string SalesOfficeName { get; set; }
        string SalesOffice { get; set; }
        string BroadCustomerClassification { get; set; }
        string DiscountType { get; set; }
        decimal? PercentageDiscount { get; set; }
        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }
        string SkuCode { get; set; }
    }
}
