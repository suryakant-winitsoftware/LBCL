using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tax.Model.Interfaces
{
    public interface ITax:IBaseModel
    {
        public string Name { get; set; }
        public string LegalName { get; set; }
        public string ApplicableAt { get; set; }
        public string TaxCalculationType { get; set; }
        public decimal BaseTaxRate { get; set; }
        public string Status { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUpto { get; set; }
        public string Code { get; set; }
        public bool IsTaxOnTaxApplicable { get; set; }
        public string DependentTaxes { get; set; }
    }
}
