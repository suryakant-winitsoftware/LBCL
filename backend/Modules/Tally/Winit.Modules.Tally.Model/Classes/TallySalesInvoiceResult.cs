using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class TallySalesInvoiceResult : ITallySalesInvoiceResult
    {
        public ITallySalesInvoiceMaster Header { get; set; }
        public List<ITallySalesInvoiceLineMaster> Lines { get; set; }
    }
}
