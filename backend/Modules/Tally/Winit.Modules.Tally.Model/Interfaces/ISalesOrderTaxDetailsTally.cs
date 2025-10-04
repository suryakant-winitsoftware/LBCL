using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface ISalesOrderTaxDetailsTally : IBaseModel
    {
        public string TaxUID { get; set; }
        public decimal TaxAmount { get; set; }
        public string TaxAmountStr { get; set; }
    }
}
