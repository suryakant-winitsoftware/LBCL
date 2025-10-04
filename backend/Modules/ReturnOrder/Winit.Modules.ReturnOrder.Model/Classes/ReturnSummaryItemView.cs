using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ReturnOrder.Model.Classes;

public class ReturnSummaryItemView:ReturnOrder, Model.Interfaces.IReturnSummaryItemView
{
    public string StoreCode { get; set; }
    public string StoreName { get; set; }
    public string OrderNumber { get; set; }
    public string Address { get; set; }
    public string OrderStatus { get; set; }
    public decimal OrderAmount { get; set; }
    public string CurrencyLabel { get; set; }
    public bool IsPosted { get; set; }
    public bool IsSelected { get; set; }
    public string SalesOrderNumber { get; set; }
}
