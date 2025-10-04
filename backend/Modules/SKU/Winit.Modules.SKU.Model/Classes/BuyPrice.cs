using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes
{
    public class BuyPrice: IBuyPrice
    {
        public string FranchiseeOrgUID { get; set; }
        public string SKUCode { get; set; }
        public decimal Price { get; set; }
    }
}
