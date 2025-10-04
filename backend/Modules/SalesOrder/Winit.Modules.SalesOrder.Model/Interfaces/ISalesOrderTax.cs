using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.Model.Interfaces
{
    public interface ISalesOrderTax : IBaseModel
    {
      string SalesOrderUID { get; set; }
      string SalesOrderLineUID { get; set; }
      string TaxUID { get; set; }
      string TaxSlabUID { get; set; }
      decimal? TaxAmount { get; set; }
      string TaxName { get; set; }
      string ApplicableAt { get; set; }
      string DependentTaxUID { get; set; }
      string DependentTaxName { get; set; }
      string TaxCalculationType { get; set; }
      decimal? BaseTaxRate { get; set; }
      decimal? RangeStart { get; set; }
      decimal? RangeEnd { get; set; }
      decimal? TaxRate { get; set; }

    }
}
