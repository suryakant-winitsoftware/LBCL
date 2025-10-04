using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace WINITMobile.Pages.PriceCheck.Models
{
    public class ProductFeedbackItem : BaseModel
    {
        public string StoreUID { get; set; }
        public string EndCustomerName { get; set; }
        public int MobileNumber { get; set; }
        public string SkuUID { get; set; }
        public List<string> FeedbackTypes { get; set; } = new List<string>();
        public string OtherRemarks { get; set; }
    }
}
