using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ReturnOrder.Model.Interfaces;

public interface IReturnOrderInvoiceMaster
{
    public IReturnOrderInvoice ReturnOrderInvoice { get; set; }
    public List<IReturnOrderLineInvoice> ReturnOrderLineInvoices { get; set; }
}
