using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class SalesOrderTaxDetailsTally : BaseModel, ISalesOrderTaxDetailsTally
    {
        public string TaxUID { get; set; }
        public decimal TaxAmount { get; set; }
        public string TaxAmountStr { get; set; }
    }
}
