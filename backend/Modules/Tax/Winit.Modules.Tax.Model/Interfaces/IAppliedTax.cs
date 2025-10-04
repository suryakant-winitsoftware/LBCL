using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Tax.Model.Interfaces
{
    public interface IAppliedTax
    {
        public string TaxUID { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Amount { get; set; }
        public bool IsTaxOnTaxApplicable { get; set; }
    }
}
