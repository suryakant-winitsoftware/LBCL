using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tax.Model.Interfaces;

namespace Winit.Modules.Tax.Model.Classes
{
    public class AppliedTax : IAppliedTax
    {
        public string TaxUID { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Amount { get; set; }
        public bool IsTaxOnTaxApplicable { get; set; }
    }
}
