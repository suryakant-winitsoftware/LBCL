using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes
{
    public class SKUPriceList:BaseModel,ISKUPriceList
    {
        public string CompanyUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string OrgUID { get; set; }
        public string DistributionChannelUID { get; set; }
        public int Priority { get; set; }
        public string SelectionGroup { get; set; }
        public string SelectionType { get; set; }
        public string SelectionUID { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUpto { get; set; }
    }
}
