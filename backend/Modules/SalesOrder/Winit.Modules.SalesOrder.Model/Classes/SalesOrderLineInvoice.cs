using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.Interfaces;

namespace Winit.Modules.SalesOrder.Model.Classes;

public class SalesOrderLineInvoice:ISalesOrderLineInvoice
{
    public string SalesOrderUID { get; set; } = string.Empty;
    public string SalesOrderLineUID { get; set; } = string.Empty;
    public string SKUUID { get; set; } = string.Empty;
    public string SKUCode { get; set; } = string.Empty;
    public string? SKUName { get; set; }
    public decimal AvailableQty { get; set; }
}
