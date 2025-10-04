using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
namespace Winit.Modules.ReturnOrder.BL.Classes;

public class POReturnOrderBaseViewModel : IPOReturnOrderViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    public readonly IListHelper _listHelper;
    public readonly ISortHelper _sorter;
    public readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    private readonly IAppConfig _appConfigs;
    private readonly IPOReturnOrderDataHelper _dataHelper;
    private readonly IAddProductPopUpDataHelper _addProductPopUpDataHelper;
    public bool IsNewOrder { get; set; }
    public bool IsDraftOrder { get; set; }
    public IReturnOrder ReturnOrder { get; set; }
    public IStoreMaster? SelectedStoreMaster { get; set; }
    public string? SelectedDistributor { get; set; }
    public string? StoreSearchString { get; set; }
    public IEnumerable<IStore> FilteredStores => Stores.Where(e => string.IsNullOrEmpty(StoreSearchString) ||
        (e.Name?.Contains(StoreSearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (e.Code?.Contains(StoreSearchString, StringComparison.OrdinalIgnoreCase) ?? false));
    public IEnumerable<IInvoiceView> FilteredInvoiceViews { get; }
    public List<IInvoiceView> InvoiceViews { get; }
    public string? InvoiceSearchString { get; set; }
    public List<SKUAttributeDropdownModel> SKUAttributeData { get; }
    public List<ISKUV1> SKUs { get; }
    public List<IStore> Stores { get; set; }
    public List<IPOReturnOrderLineItem> POReturnOrderLineItems { get; set; }
    public List<IPOReturnOrderLineItem> FilteredPOReturnOrderLineItems { get; set; }
    public InvoiceListRequest InvoiceListRequest { get; }
    public IInvoiceMaster? SelectedInvoiceMaster { get; set; }
    public string SelectedInvoiceUID { get; set; }
    public POReturnOrderBaseViewModel(
        IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        IListHelper listHelper,
        IAppUser appUser,
        IAppSetting appSetting,
        IAppConfig appConfigs,
        IPOReturnOrderDataHelper dataHelper,
        SKU.BL.Interfaces.IAddProductPopUpDataHelper addProductPopUpDataHelper)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        _appSetting = appSetting;
        _appConfigs = appConfigs;
        _dataHelper = dataHelper;
        _addProductPopUpDataHelper = addProductPopUpDataHelper;
        SKUAttributeData = [];
        SKUs = [];
        POReturnOrderLineItems = [];
        FilteredPOReturnOrderLineItems = [];
        Stores = [];
        ReturnOrder = serviceProvider.GetRequiredService<IReturnOrder>();
        ReturnOrder.OrderDate = DateTime.Now;
        InvoiceListRequest = new InvoiceListRequest();
        InvoiceListRequest.EndDate = DateTime.Now;
        InvoiceListRequest.StartDate = DateTime.Now.AddMonths(-2);
        FilteredInvoiceViews = [];
        InvoiceViews = [];
        SelectedInvoiceMaster ??= serviceProvider.GetRequiredService<IInvoiceMaster>();
    }

    public async Task PopulateViewModel(string source, string orderUID = "")
    {
        await SetSystemSettingValues();
        await Task.Run(() =>
        {
            PrepareFilters();
        });
    }

    async private Task PrepareFilters()
    {
        await PopulateSKUAttributeData();
    }

    public async Task PopulateSKUAttributeData()
    {
        SKUAttributeData.Clear();
        List<SKUAttributeDropdownModel>? skuAttibuteData = await _addProductPopUpDataHelper.GetSKUAttributeData();
        if (skuAttibuteData != null && skuAttibuteData.Any())
        {
            SKUAttributeData.AddRange(skuAttibuteData);
        }
    }

    public virtual async Task SetSystemSettingValues()
    {
        //SetCurrency();
        //SetDefaultMaxQty();
        await Task.CompletedTask;
    }


    public async Task PrepareDistributors()
    {
        Stores.Clear();
        List<IStore>? stores = await _dataHelper.GetChannelPartner(_appUser.SelectedJobPosition.UID);
        if (stores != null && stores.Count > 0)
        {
            Stores.AddRange(stores);
        }
    }
    public async Task OnDistributorSelect()
    {
        POReturnOrderLineItems.Clear();
        FilteredPOReturnOrderLineItems.Clear();
        if (!string.IsNullOrEmpty(ReturnOrder.UID))
        {
            //PurchaseOrderMaster = await _dataHelper.GetPurchaseOrderMasterByUID(PurchaseOrderHeader.UID);
            /*if (PurchaseOrderMaster is not null)
            {
                PurchaseOrderHeader = PurchaseOrderMaster.PurchaseOrderHeader!;
                SelectedDistributor = PurchaseOrderHeader.OrgUID;
                if (PurchaseOrderHeader.Status == PurchaseOrderStatusConst.Draft)
                {
                    IsDraftOrder = true;
                }
                else
                {
                    IsNewOrder = false;
                    _ = SetStoreCreditLimit(PurchaseOrderMaster.PurchaseOrderHeader!.OrgUID,
                    PurchaseOrderMaster.PurchaseOrderHeader.DivisionUID!);
                }
            }*/
        }
        await PrepareSelectedStoreMaster();
        await PopulateInvoices(SelectedDistributor);

    }
    public async Task PrepareSelectedStoreMaster()
    {
        string distributorUID = SelectedDistributor ?? _appUser.SelectedJobPosition.OrgUID;
        IStoreMaster? storeMaster = await _dataHelper.GetStoreMasterByStoreUID(distributorUID);
        if (storeMaster == null)
        {
            throw new CustomException(ExceptionStatus.Failed, "SKU master fetching failed..");
        }

        SelectedStoreMaster = storeMaster;
    }

    private async Task PopulateInvoices(string selectedDistributor)
    {
        InvoiceViews.Clear();
        InvoiceListRequest.OrgUID = selectedDistributor;
        var data = await _dataHelper.GetInvoicesForReturnOrder(InvoiceListRequest);
        if (data is not null && data.Any())
        {
            InvoiceViews.AddRange(data);
        }
    }

}
