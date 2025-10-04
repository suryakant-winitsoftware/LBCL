using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.ListHeader.BL.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.Modules.ReturnOrder.BL.Classes;

public class ReturnOrderWithInvoiceAppViewModel : ReturnOrderAppViewModel, IReturnOrderWithInvoiceViewModel
{
    public List<ISalesOrderInvoice> SalesOrdersInvoices { get; set; }
    public List<ISelectionItem> SalesOrdersInvoiceSelectionItems { get; set; }
    public ISalesOrderInvoice? SelectedInvoice { get; set; }
    public List<ISalesOrderLineInvoice> SalesOrderLineInvoiceItems { get; set; }
    public ISalesOrderBL _salesOrderBL { get; set; }

    public ReturnOrderWithInvoiceAppViewModel(
        IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        Interfaces.IReturnOrderAmountCalculator amountCalculator,
        IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
        Winit.Modules.Base.BL.ApiService apiService,
        IAppUser appUser, ISKUBL sKUBL, IListHeaderBL listHeaderBL, ISKUPriceBL sKUPriceBL,
        IReturnOrderBL returnOrderBL, IFileSysBL fileSysBL, ISalesOrderBL salesOrderBL, IStringLocalizer<LanguageKeys> localizer,
        IDataManager dataManager) :
        base(serviceProvider, filter, sorter, amountCalculator, listHelper, appConfigs, apiService, appUser, sKUBL, listHeaderBL,
           sKUPriceBL, returnOrderBL, fileSysBL,localizer,dataManager)
    {
        SalesOrdersInvoices = new List<ISalesOrderInvoice>();
        _salesOrderBL = salesOrderBL;
        SalesOrderLineInvoiceItems = new List<ISalesOrderLineInvoice>();
        SalesOrdersInvoiceSelectionItems = new List<ISelectionItem>();
        OrderType = "withinvoice";
    }
    public override async Task PopulateViewModel(string source, bool isNewOrder = true, string returnOrderUID = "")
    {
        await base.PopulateViewModel(source, isNewOrder, returnOrderUID);
        await SetSalesOrdersForInvoiceList(SelectedStoreViewModel.StoreUID);
    }
    public async Task SetSalesOrdersForInvoiceList(string storeUID)
    {
        var data = await _salesOrderBL.GetAllSalesOrderInvoices(storeUID);
        if (data != null && data.Any())
        {
            SalesOrdersInvoices.AddRange(data);
        }
    }
    public async Task PopulateViewModelWithInvoice(string sourceType, bool isNewOrder,string? returnOrderUID = null)
    {
        ReturnOrderUID = returnOrderUID;
        SalesOrderUID = SelectedInvoice!.SalesOrderUID;
        SalesOrderLineInvoiceItems = await _salesOrderBL.GetSalesOrderLineInvoiceItems(SelectedInvoice!.SalesOrderUID);
        if (SalesOrderLineInvoiceItems == null || !SalesOrderLineInvoiceItems.Any()) throw new Exception("Failed to retrive the SalesOrderLine data");
        await base.PopulateViewModel(sourceType, isNewOrder);
        PopulateReturnOrderItemViewsWithInvoices(SalesOrderLineInvoiceItems);

    }
    protected async override Task PopulateSKUMaster()
    {
        List<ISKUMaster> skuMasters = await SKUMasters_Data(_appUser.OrgUIDs, SalesOrderLineInvoiceItems.Select(e => e.SKUUID).ToList());
        SkuAttributesList.AddRange(skuMasters.SelectMany(e => e.SKUAttributes.ToList()).Where(attr => attr != null));
        SKUList.AddRange(skuMasters.Select(e => e.SKU).Where(sku => sku != null));
        List<IReturnOrderItemView> returnOrderItemViews = ConvertToIReturnOrderItemView(skuMasters.ToList());
        if (returnOrderItemViews != null)
        {
            ReturnOrderItemViewsRawdata.AddRange(returnOrderItemViews);
        }
    }
    private void PopulateReturnOrderItemViewsWithInvoices(List<ISalesOrderLineInvoice> salesOrderLines)
    {
        foreach (var returnOrderItemView in ReturnOrderItemViewsRawdata)
        {
            var salesOrderLine = salesOrderLines.FirstOrDefault(e => e.SKUUID == returnOrderItemView.SKUUID);
            if (salesOrderLine == null) throw new Exception($"{returnOrderItemView.SKUUID} is not found");
            returnOrderItemView.AvailableQty = salesOrderLine.AvailableQty;
            returnOrderItemView.SalesOrderUID = salesOrderLine.SalesOrderUID;
            returnOrderItemView.SalesOrderLineUID = salesOrderLine.SalesOrderLineUID;

        }
    }
    public async override Task<bool> SaveOrder()
    {
        try
        {
            DateTime dateTime = DateTime.Now;
            await _salesOrderBL.UpdateSalesOrderLinesReturnQty(DisplayedReturnOrderItemViews.Select(e => new SalesOrderLine
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
    public Task OnInvoiceSelect()
    {
        throw new NotImplementedException("");
    }
}
