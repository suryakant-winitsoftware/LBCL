using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ReturnOrder.BL.Interfaces;

public interface IReturnOrderWithInvoiceViewModel : IReturnOrderViewModel
{
    ISalesOrderInvoice? SelectedInvoice { get; set; }
    List<ISalesOrderInvoice> SalesOrdersInvoices { get; set; }
    List<ISelectionItem> SalesOrdersInvoiceSelectionItems { get; set; }
    Task SetSalesOrdersForInvoiceList(string storeUID);
    Task PopulateViewModelWithInvoice(string SourceType, bool IsNewOrder, string? returnOrderUID = null);
    Task OnInvoiceSelect();
}
