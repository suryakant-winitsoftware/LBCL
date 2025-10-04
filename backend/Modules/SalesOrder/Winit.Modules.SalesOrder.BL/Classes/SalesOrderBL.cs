using System.Data;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITSharedObjects.Constants;

namespace Winit.Modules.SalesOrder.BL.Classes;

public class SalesOrderBL : SalesOrderBaseBL, ISalesOrderBL
{
    protected readonly DL.Interfaces.ISalesOrderDL _salesOrderDL;
    public SalesOrderBL(DL.Interfaces.ISalesOrderDL salesOrderDL)
    {
        _salesOrderDL = salesOrderDL;
    }
    public async Task<IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderViewModel>> SelectSalesOrderDetailsAll(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias)
    {
        return await _salesOrderDL.SelectSalesOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);
    }
    public async Task<IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderViewModel>> SelectSalesOrderByUID(string SalesOrderUID)
    {
        return await _salesOrderDL.SelectSalesOrderByUID(SalesOrderUID);
    }
    public async Task<int> SaveSalesOrder(SalesOrderViewModelDCO salesOrderViewModel)
    {
        return await _salesOrderDL.SaveSalesOrder(salesOrderViewModel);
    }
    public async Task<Dictionary<string, List<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView>>> GetISalesSummaryItemViews(DateTime startDate, DateTime endDate, string storeUID = "")
    {
        List<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView> salesSummaryItemViews = await _salesOrderDL.GetISalesSummaryItemViews(startDate, endDate, storeUID);
        Dictionary<string, List<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView>> salesSummaryItemViewsDictionary = new()
        {
            [SalesSummaryTabs.Todays] = salesSummaryItemViews
            .Where(item => item.OrderStatus != SalesOrderStatus.DRAFT
            && item.OrderDate.Date == DateTime.Now.Date).ToList(),
            [SalesSummaryTabs.Old] = salesSummaryItemViews
            .Where(item => item.OrderStatus != SalesOrderStatus.DRAFT
            && item.OrderDate.Date != DateTime.Now.Date).ToList(),
            [SalesSummaryTabs.All] = salesSummaryItemViews
            .ToList()
        };
        salesSummaryItemViewsDictionary[SalesSummaryTabs.Draft] = salesSummaryItemViews
            .Where(item => item.OrderStatus == SalesOrderStatus.DRAFT)
            .ToList();
        return salesSummaryItemViewsDictionary;
    }
    public async Task<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView> GetSalesOrderPrintView(string SalesOrderUID)
    {
        return await _salesOrderDL.GetSalesOrderPrintView(SalesOrderUID);
    }
    public async Task<IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>> GetSalesOrderLinePrintView(string SalesOrderUID)
    {
        return await _salesOrderDL.GetSalesOrderLinePrintView(SalesOrderUID);
    }
    public async Task<PagedResponse<IDeliveredPreSales>> SelectDeliveredPreSales(List<SortCriteria> sortCriterias, int pageNumber,
     int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, DateTime startDate, DateTime endDate, string Status)
    {
        return await _salesOrderDL.SelectDeliveredPreSales(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, startDate, endDate, Status);
    }
    public async Task<IViewPreSales> SelectDeliveredPreSalesBySalesOrderUID(string SalesOrderUID)
    {
        return await _salesOrderDL.SelectDeliveredPreSalesBySalesOrderUID(SalesOrderUID);
    }
    public async Task<ISalesOrder?> GetSalesOrderByUID(string salesOrderUID)
    {
        return await _salesOrderDL.GetSalesOrderByUID(salesOrderUID);
    }

    public async Task<List<ISalesOrderLine>?> GetSalesOrderLinesBySalesOrderUID(string salesOrderUID)
    {
        return await _salesOrderDL.GetSalesOrderLinesBySalesOrderUID(salesOrderUID);
    }
    public async Task<int> CUD_SalesOrder(Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO salesOrderViewModel)
    {
        return await _salesOrderDL.CUD_SalesOrder(salesOrderViewModel);
    }

    public async Task<SalesOrderViewModelDCO> GetSalesOrderMasterDataBySalesOrderUID(string SalesOrderUID)
    {
        SalesOrderViewModelDCO salesOrderViewModelDCO = new()
        {
            SalesOrder = (Winit.Modules.SalesOrder.Model.Classes.SalesOrder)await _salesOrderDL.GetSalesOrderByUID(SalesOrderUID),
        };
        List<ISalesOrderLine>? salesOrderLines = await _salesOrderDL.GetSalesOrderLinesBySalesOrderUID(SalesOrderUID);
        salesOrderViewModelDCO.SalesOrderLines = salesOrderLines?.OfType<Winit.Modules.SalesOrder.Model.Classes.SalesOrderLine>()?.ToList();
        return salesOrderViewModelDCO;
    }

    public async Task<int> InsertorUpdate_SalesOrders(Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO salesOrderView)
    {
        return await _salesOrderDL.InsertorUpdate_SalesOrders(salesOrderView);
    }
    public async Task<int> UpdateSalesOrderStatus(Model.Classes.SalesOrderStatusModel salesOrderStatus)
    {
        return await _salesOrderDL.UpdateSalesOrderStatus(salesOrderStatus);
    }
    public async Task<List<ISalesOrderInvoice>> GetAllSalesOrderInvoices(string? storeUID = null)
    {
        return await _salesOrderDL.GetAllSalesOrderInvoices(storeUID);
    }
    public async Task<List<ISalesOrderLineInvoice>> GetSalesOrderLineInvoiceItems(string salesOrderUID)
    {
        return await _salesOrderDL.GetSalesOrderLineInvoiceItems(salesOrderUID);
    }
    public async Task<int> UpdateSalesOrderLinesReturnQty(List<ISalesOrderLine> salesOrderLines)
    {
        return await _salesOrderDL.UpdateSalesOrderLinesReturnQty(salesOrderLines);
    }

}
