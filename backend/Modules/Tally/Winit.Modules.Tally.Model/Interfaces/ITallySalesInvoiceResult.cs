using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface ITallySalesInvoiceResult
    {
        public ITallySalesInvoiceMaster Header { get; set; }
        public List<ITallySalesInvoiceLineMaster> Lines { get; set; }
    }
}
