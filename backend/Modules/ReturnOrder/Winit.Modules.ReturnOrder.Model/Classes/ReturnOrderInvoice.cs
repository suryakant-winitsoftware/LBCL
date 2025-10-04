
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ReturnOrder.Model.Interfaces;

namespace Winit.Modules.ReturnOrder.Model.Classes;

public class ReturnOrderInvoice:IReturnOrderInvoice
{
    public string ReturnOrderUID { get; set; } = string.Empty;
    public string ReturnOrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } 
    public string? SalesOrderNumber { get; set; }
    public string OrderType { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public decimal SKUCount { get; set; }
}
