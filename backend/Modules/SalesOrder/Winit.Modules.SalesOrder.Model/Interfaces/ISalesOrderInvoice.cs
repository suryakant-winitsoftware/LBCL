using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SalesOrder.Model.Interfaces;

public interface ISalesOrderInvoice
{
    public string SalesOrderUID { get; set; }
    public string SalesOrderNumber { get; set; }
    public DateTime DeliveryDate { get; set; }
    public int TotalQuantity { get; set; }
    public int AvailableQty { get; set; }
}
