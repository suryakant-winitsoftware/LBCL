using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace WINITMobile.Pages.PriceCheck.Models
{
    public class ProductSamplingItem : BaseModel
    {
       public string StoreUID { get; set; }
        public string StoreName { get; set; }
        public string SkuUID { get; set; }
        public string SellingPrice { get; set; }
        public string UnitsUsed { get; set; }
        public string UnitsSold { get; set; }
        public int NoOfCustomersApproached{ get; set; }
    }
}
