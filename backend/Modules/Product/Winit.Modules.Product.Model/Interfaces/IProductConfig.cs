using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Product.Model.Interfaces
{
    public interface IProductConfig : Base.Model.IBaseModel
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
