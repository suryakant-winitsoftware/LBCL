using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.BL.Interfaces;

public interface ISalesOrderBL
{
    Task<IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderViewModel>> SelectSalesOrderDetailsAll(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias);
    Task<IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderViewModel>> SelectSalesOrderByUID(string SalesOrderUID);
    Task<int> SaveSalesOrder(SalesOrderViewModelDCO salesOrderViewModel);
    Task<Dictionary<string, List<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView>>> GetISalesSummaryItemViews(DateTime startDate, DateTime endDate, string storeUID = "");
    Task<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView> GetSalesOrderPrintView(string SalesOrderUID);
    Task<IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>> GetSalesOrderLinePrintView(string SalesOrderUID);
    Task<PagedResponse<IDeliveredPreSales>> SelectDeliveredPreSales(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, DateTime startDate, DateTime endDate,
    string Status);
    Task<IViewPreSales> SelectDeliveredPreSalesBySalesOrderUID(string SalesOrderUID);
    public Task<ISalesOrder?> GetSalesOrderByUID(string salesOrderUID);
    public Task<List<ISalesOrderLine>?> GetSalesOrderLinesBySalesOrderUID(string salesOrderUID);
    Task<int> CUD_SalesOrder(Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO salesOrderViewModel);
    Task<SalesOrderViewModelDCO> GetSalesOrderMasterDataBySalesOrderUID(string SalesOrderUID);
    Task<int> InsertorUpdate_SalesOrders(SalesOrderViewModelDCO SalesOrderUID);
    Task<int> UpdateSalesOrderStatus(Model.Classes.SalesOrderStatusModel salesOrderStatus);
    Task<List<ISalesOrderInvoice>> GetAllSalesOrderInvoices(string? storeUID = null);
    Task<List<ISalesOrderLineInvoice>> GetSalesOrderLineInvoiceItems(string salesOrderUID);
    Task<int> UpdateSalesOrderLinesReturnQty(List<ISalesOrderLine> salesOrderLines);
}
