using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ReturnOrder.Model.Interfaces;

public interface IReturnOrderInvoice
{
    public string ReturnOrderUID { get; set; }
    public string ReturnOrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string? SalesOrderNumber { get; set; }
    public string OrderType { get; set; }
    public string OrderStatus { get; set; }
    public decimal SKUCount { get; set; }
}
