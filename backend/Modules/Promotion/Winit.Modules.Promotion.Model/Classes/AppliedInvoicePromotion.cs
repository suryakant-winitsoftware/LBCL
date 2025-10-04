using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class AppliedInvoicePromotion
    {
        public string PromotionUID { get; set; }
        public string PromoFormat { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<FOCItem> FOCItems { get; set; }
    }
}
