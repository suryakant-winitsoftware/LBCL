using System.Data;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
namespace Winit.Modules.SalesOrder.DL.Interfaces;

public interface ISalesOrderDL
{
    Task<IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderViewModel>> SelectSalesOrderDetailsAll(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias);
    Task<IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderViewModel>> SelectSalesOrderByUID(string SalesOrderUID);
    Task<int> SaveSalesOrder(SalesOrderViewModelDCO salesOrderViewModel);
    Task<int> CreateSalesOrder(Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder salesOrder,
        IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<int> CreateSalesOrderLine(List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine> salesOrderLines,
        IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<int> UpdateSalesOrder(Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder salesOrder,
        IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<int> UpdateSalesOrderLine(List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine> salesOrderLines,
        IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<ISalesOrder?> GetSalesOrderByUID(string salesOrderUID);
    public Task<List<ISalesOrderLine>?> GetSalesOrderLinesBySalesOrderUID(string salesOrderUID);
    Task<List<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView>> GetISalesSummaryItemViews(DateTime startDate, DateTime endDate, string storeUID = "");
    Task<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView> GetSalesOrderPrintView(string SalesOrderUID);
    Task<IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>> GetSalesOrderLinePrintView(string SalesOrderUID);
    Task<PagedResponse<IDeliveredPreSales>> SelectDeliveredPreSales(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, DateTime startDate, DateTime endDate, string Status);
    Task<IViewPreSales> SelectDeliveredPreSalesBySalesOrderUID(string SalesOrderUID);
    Task<int> CUD_SalesOrder(Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO salesOrderViewModel);
    Task<int> InsertorUpdate_SalesOrders(Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO salesOrderView);
    Task<int> UpdateSalesOrderStatus(Model.Classes.SalesOrderStatusModel salesOrderStatus);
    Task<List<ISalesOrderInvoice>> GetAllSalesOrderInvoices(string? storeUID = null);
    Task<List<ISalesOrderLineInvoice>> GetSalesOrderLineInvoiceItems(string salesOrderUID);
    Task<int> UpdateSalesOrderLinesReturnQty(List<ISalesOrderLine> salesOrderLines);
}

