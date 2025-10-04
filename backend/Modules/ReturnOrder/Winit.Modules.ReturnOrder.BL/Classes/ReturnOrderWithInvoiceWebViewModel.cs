using Microsoft.IdentityModel.Tokens;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.BL.Classes;

public class ReturnOrderWithInvoiceWebViewModel : ReturnOrderWebViewModel, IReturnOrderWithInvoiceViewModel
{
    public List<ISalesOrderInvoice> SalesOrdersInvoices { get; set; }
    public List<ISelectionItem> SalesOrdersInvoiceSelectionItems { get; set; }
    public ISalesOrderInvoice? SelectedInvoice { get; set; }
    public List<ISalesOrderLineInvoice> SalesOrderLineInvoiceItems { get; set; }

    public ReturnOrderWithInvoiceWebViewModel(
        IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        Interfaces.IReturnOrderAmountCalculator amountCalculator,
        IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
        Winit.Modules.Base.BL.ApiService apiService,
        IAppUser appUser) :
        base(serviceProvider, filter, sorter, amountCalculator, listHelper, appConfigs, apiService, appUser)
    {
        SalesOrdersInvoices = [];
        SalesOrderLineInvoiceItems = [];
        SalesOrdersInvoiceSelectionItems = [];
        OrderType = "withinvoice";
    }

    public override async Task PopulateViewModel(string source, bool isNewOrder = true, string returnOrderUID = "")
    {
        IsNewOrder = isNewOrder;
        ReturnOrderUID = returnOrderUID;
        Source = source;
        Status = Shared.Models.Constants.SalesOrderStatus.DRAFT;
        RouteSelectionItems.Clear();
        RouteSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IRoute>
            (await GetAllRoutesAPIAsync(), ["UID", "Code", "Name"]));
        await PopulateReasons();
        InitializeOptionalDependencies();
        await SetSystemSettingValues();
        if (!returnOrderUID.IsNullOrEmpty())
        {
            await PopulateReturnOrder();
        }
        else
        {
            ReturnOrderUID = Guid.NewGuid().ToString();
        }
        IsTaxable = true;
    }
    protected override async Task PopulateReturnOrder()
    {
        if (!IsNewOrder && !string.IsNullOrEmpty(ReturnOrderUID))
        {
            Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster returnOrderMaster = await GetReturnOrder_Data();
            if (returnOrderMaster != null)
            {
                ConvertReturnOrderToReturnOrderViewModel(returnOrderMaster.ReturnOrder);
                await OnRouteSelect(RouteUID);
                RouteSelectionItems.FirstOrDefault(e => e.UID == RouteUID)!.IsSelected = true;
                await OnStoreItemViewSelected(returnOrderMaster.ReturnOrder.StoreUID);
                StoreSelectionItems.FirstOrDefault(e => e.UID == StoreUID)!.IsSelected = true;
                if (!string.IsNullOrEmpty(returnOrderMaster.ReturnOrder.SalesOrderUID))
                {
                    SelectedInvoice = SalesOrdersInvoices.Find(e => e.SalesOrderUID == returnOrderMaster.ReturnOrder.SalesOrderUID);
                    await OnInvoiceSelect();
                }
                foreach (IReturnOrderLine returnOrderLine in returnOrderMaster.ReturnOrderLineList)
                {
                    OverideReturnOrderItemView(returnOrderLine);
                }
            }
        }
    }
    public override async Task OnStoreItemViewSelected(string storeItemViewUID)
    {
        await base.OnStoreItemViewSelected(storeItemViewUID);
        await SetSalesOrdersForInvoiceList(storeItemViewUID);
    }
    public async Task SetSalesOrdersForInvoiceList(string storeItemViewUID)
    {
        SalesOrdersInvoices.Clear();
        SalesOrdersInvoiceSelectionItems.Clear();
        List<ISalesOrderInvoice> data = await GetAllSalesOrderInvoicesAPI(storeItemViewUID);
        if (data != null && data.Any())
        {
            SalesOrdersInvoices.AddRange(data);
            SalesOrdersInvoiceSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<ISalesOrderInvoice>
                (data, ["SalesOrderUID", "SalesOrderNumber", "SalesOrderNumber"]));
        }
    }
    public async Task PopulateViewModelWithInvoice(string sourceType, bool isNewOrder, string? returnOrderUID = null)
    {
        await PopulateViewModel(sourceType, isNewOrder, returnOrderUID);
    }
    protected override async Task PopulateSKUMaster()
    {
        List<ISKUMaster> skuMasters = await SKUMasters_Data(_appUser.OrgUIDs, SalesOrderLineInvoiceItems.Select(e => e.SKUUID).ToList());
        SkuAttributesList.Clear();
        SkuAttributesList.AddRange(skuMasters.SelectMany(e => e.SKUAttributes.ToList()).Where(attr => attr != null));
        SKUList.Clear();
        SKUList.AddRange(skuMasters.Select(e => e.SKU).Where(sku => sku != null));
        List<IReturnOrderItemView> returnOrderItemViews = ConvertToIReturnOrderItemView(skuMasters.ToList());
        if (returnOrderItemViews != null)
        {
            ReturnOrderItemViewsRawdata.Clear();
            ReturnOrderItemViewsRawdata.AddRange(returnOrderItemViews);
        }
    }

    public async Task OnInvoiceSelect()
    {
        SalesOrderUID = SelectedInvoice!.SalesOrderUID;
        SalesOrderLineInvoiceItems = await GetSalesOrderLineInvoiceItemsAPI(SelectedInvoice!.SalesOrderUID);
        if (SalesOrderLineInvoiceItems == null || !SalesOrderLineInvoiceItems.Any())
        {
            throw new Exception("Failed to retrive the SalesOrderLine data");
        }

        await PopulateSKUMaster();
        await PopulatePriceMaster();
        PopulateReturnOrderItemViewsWithInvoices(SalesOrderLineInvoiceItems);
        await ApplyFilter([], FilterMode.And);
        List<SortCriteria> sortCriteriaList = [new SortCriteria("SKUName", SortDirection.Asc)];
        await ApplySort(sortCriteriaList);
    }

    private void PopulateReturnOrderItemViewsWithInvoices(List<ISalesOrderLineInvoice> salesOrderLines)
    {
        foreach (IReturnOrderItemView returnOrderItemView in ReturnOrderItemViewsRawdata)
        {
            ISalesOrderLineInvoice? salesOrderLine = salesOrderLines.FirstOrDefault(e => e.SKUUID == returnOrderItemView.SKUUID);
            if (salesOrderLine == null)
            {
                throw new Exception($"{returnOrderItemView.SKUUID} is not found");
            }

            returnOrderItemView.AvailableQty = salesOrderLine.AvailableQty;
            returnOrderItemView.SalesOrderUID = salesOrderLine.SalesOrderUID;
            returnOrderItemView.SalesOrderLineUID = salesOrderLine.SalesOrderLineUID;

        }
    }
    public override async Task<bool> SaveOrder()
    {
        try
        {
            DateTime dateTime = DateTime.Now;
            _ = await UpdateSalesOrderLinesReturnQtyAPI(DisplayedReturnOrderItemViews.Select(e => new SalesOrderLine
            {
                UID = e.SalesOrderLineUID,
                ReturnedQty = e.OrderQty,
                ModifiedTime = dateTime
            }).ToList<ISalesOrderLine>());
            return await base.SaveOrder();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<List<ISalesOrderInvoice>> GetAllSalesOrderInvoicesAPI(string storeItemViewUID)
    {
        try
        {
            ApiResponse<List<SalesOrderInvoice>> apiResponse =
            await _apiService.FetchDataAsync<List<SalesOrderInvoice>>
            ($"{_appConfigs.ApiBaseUrl}SalesOrder/GetAllSalesOrderInvoices?storeUID={storeItemViewUID}",
                HttpMethod.Get);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null
                ? apiResponse.Data.ToList().ToList<ISalesOrderInvoice>()
                : throw new Exception("Failed to retrive the Invoices");
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task<List<ISalesOrderLineInvoice>> GetSalesOrderLineInvoiceItemsAPI(string salesOrderUID)
    {
        try
        {
            ApiResponse<List<SalesOrderLineInvoice>> apiResponse =
            await _apiService.FetchDataAsync<List<SalesOrderLineInvoice>>
            ($"{_appConfigs.ApiBaseUrl}SalesOrder/GetSalesOrderLineInvoiceItems?SalesOrderUID={salesOrderUID}",
                HttpMethod.Get);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null
                ? apiResponse.Data.ToList().ToList<ISalesOrderLineInvoice>()
                : throw new Exception("Failed to retrive the Sales Order Items");
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task<bool> UpdateSalesOrderLinesReturnQtyAPI(List<ISalesOrderLine> salesOrderLines)
    {
        try
        {
            ApiResponse<string> apiResponse =
            await _apiService.FetchDataAsync<string>
            ($"{_appConfigs.ApiBaseUrl}SalesOrder/UpdateSalesOrderLinesReturnQty",
                HttpMethod.Put, salesOrderLines);
            return apiResponse != null ? apiResponse.IsSuccess : throw new Exception("Failed to Update SalesOrder");
        }
        catch (Exception)
        {
            throw;
        }
    }
}
