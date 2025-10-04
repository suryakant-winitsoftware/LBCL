using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Product.Model.Interfaces;

namespace Winit.Modules.Product.Model.Classes
{
    public class ProductConfigAll : BaseModel, IProductConfigAll
    {
        // Default constructor
        public ProductConfigAll()
        {
            // Initialization code
        }
        public Int64 SKUConfigId { get; set; }
        public string ProductCode { get; set; }
        public string DistributionChannelOrgCode { get; set; }
        public bool CanBuy { get; set; }
        public bool CanSell { get; set; }
        public string BuyingUOM { get; set; }
        public string SellingUOM { get; set; }
        public bool IsActive { get; set; }
    }

}
