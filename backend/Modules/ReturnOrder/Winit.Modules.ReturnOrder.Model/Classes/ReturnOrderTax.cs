using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ReturnOrder.Model.Interfaces;

namespace Winit.Modules.ReturnOrder.Model.Classes
{
    public class ReturnOrderTax:BaseModel, IReturnOrderTax
    {
        public string ReturnOrderUID { get; set; }
        public string ReturnOrderLineUID { get; set; }
        public string TaxUID { get; set; }
        public string TaxSlabUID { get; set; }
        public decimal TaxAmount { get; set; }
        public string TaxName { get; set; }
        public string ApplicableAt { get; set; }
        public string DependentTaxUID { get; set; }
        public string DependentTaxName { get; set; }
        public string TaxCalculationType { get; set; }
        public decimal BaseTaxRate { get; set; }
        public decimal RangeStart { get; set; }
        public decimal RangeEnd { get; set; }
        public decimal TaxRate { get; set; }
    }
}
