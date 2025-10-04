using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SalesOrder.Model.Interfaces;

public interface ISalesOrderLineInvoice
{
    string SalesOrderUID { get; set; }
    string SalesOrderLineUID { get; set; }
    string SKUUID { get; set; }
    string SKUCode { get; set; }
    string? SKUName { get; set; }    
    decimal AvailableQty { get; set; }
}
