using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class AppliedPromotionView
    {
        public AppliedPromotionView() 
        {
            this.FOCItems = new List<FOCItem>();
        }
        public string? PromotionUID { get; set; }
        public string? PromoFormat { get; set; }
        public int Priority { get; set; }
        public bool IsFOC { get; set; }
        public string? UniqueUID { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<FOCItem> FOCItems { get; set; }
    }
}
