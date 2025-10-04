using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface ISKUConfig : IBaseModel
    {
        public string OrgUID { get; set; }
        public string Name { get; set; }
        public string DistributionChannelOrgUID { get; set; }
        public string SKUUID { get; set; }
        public bool CanBuy { get; set; }
        public bool CanSell { get; set; }
        public string BuyingUOM { get; set; }
        public string SellingUOM { get; set; }
        public bool IsActive { get; set; }
    }
}
