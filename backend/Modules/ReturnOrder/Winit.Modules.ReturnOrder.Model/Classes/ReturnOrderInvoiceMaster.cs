using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ReturnOrder.Model.Interfaces;

namespace Winit.Modules.ReturnOrder.Model.Classes;

public class ReturnOrderInvoiceMaster: IReturnOrderInvoiceMaster
{
    public IReturnOrderInvoice ReturnOrderInvoice { get; set; } = new ReturnOrderInvoice();
    public List<IReturnOrderLineInvoice> ReturnOrderLineInvoices { get; set; } = new List<IReturnOrderLineInvoice>();
}
