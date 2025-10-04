using Winit.Modules.Base.Model;
using Winit.Modules.PriceLadder.Model.Interfaces;

namespace Winit.Modules.PriceLadder.Model.Classes
{
    public class PriceLaddering : BaseModel, IPriceLaddering
    {
        public int? LadderingId { get; set; }
        public string OperatingUnit { get; set; }
        public string Division { get; set; }
        public int? ProductCategoryId { get; set; }
        public string Branch { get; set; }
        public string BranchName { get; set; }
        public string SalesOfficeName { get; set; }
        public string SalesOffice { get; set; }
        public string BroadCustomerClassification { get; set; }
        public string DiscountType { get; set; }
        public decimal? PercentageDiscount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SkuCode { get; set; }
    }
}
