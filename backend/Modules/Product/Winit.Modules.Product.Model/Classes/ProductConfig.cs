using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Product.Model.Interfaces;

namespace Winit.Modules.Product.Model.Classes
{
    public class ProductConfig : BaseModel, IProductConfig
    {

        public string ProductCode { get; set; }
        public string DistributionChannelOrgUID { get; set; }
        public bool CanBuy { get; set; }
        public bool CanSell { get; set; }
        public string BuyingUOM { get; set; }
        public string SellingUOM { get; set; }
        public bool IsActive { get; set; }
    }

}
