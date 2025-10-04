using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Diagnostics;
using Winit.Modules.Common.BL;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.SalesOrder.BL.UIClasses;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.CustomControls;
using Winit.UIComponents.Mobile.DialogBox;
using WINITMobile.Data;
using WINITMobile.Models.TopBar;
namespace WINITMobile.Pages.Sales;

public partial class CreateSalesOrder : SalesOrderBase
{
    [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }
    private bool IsGridView = true;
    private WinitTextBox wtbSearch;
    private ISalesSummaryItemView salesSummaryItemView;
    private Winit.Modules.Store.Model.Interfaces.IStore salesOrderStoreAddressLines;

    //private  string selectedBluetoothDeviceName = "";
    //private  string selectedBluetoothDeviceMacAddress = "";
    //private  Winit.Modules.SalesOrder.Model.Classes.SalesOrder salesOrder;
    private FilterDialog FilterDialog;
    private List<SelectionItemFilter> TopScrollSelectionItems = new();
    private List<SelectionItemFilter> LeftScrollSelectionItems = new();
    private bool IsPageLoaded = false;
    private bool ShowPromotionPopUp = false;
    private bool ShowAllPromotions = false;
    private List<ISelectionItem> MissedPromotionsHeaderSelectionItems = new();
    private Dictionary<string, List<ISelectionItem>> MissedPromotionsHeaderDetailsSelectionItems { get; set; }
    private List<ISelectionItem> PromotionDetails = new();
    private DmsPromotion SelectedPromotion;
    private SelectionManager LeftScrollSM;
    private SalesOrderListView? salesOrderListViewRef;
    private string ImgFolderPath = Path.Combine(FileSystem.AppDataDirectory, "Images");
    public List<IFileSys> ImageFileSysList { get; set; }
    public string FolderPathImages { get; set; }
    private FileCaptureData fileCaptureData = new FileCaptureData
    {
        AllowedExtensions = new List<string> { ".jpg", ".png" },
        IsCameraAllowed = true,
        IsGalleryAllowed = true,
        MaxNumberOfItems = 1,
        MaxFileSize = 10 * 1024 * 1024,
        EmbedLatLong = true,
        EmbedDateTime = true,
        LinkedItemType = "ItemType",
        LinkedItemUID = "ItemUID",
        EmpUID = "EmployeeUID",
        JobPositionUID = "JobPositionUID",
        IsEditable = true,
        Files = new List<FileSys>()
    };
    protected override async Task OnInitializedAsync()
    {
        if (_logger == null)
        {
            throw new InvalidOperationException("Logger is not initialized");
        }
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        ShowLoader("Loading Sales Order");
        _backbuttonhandler.ClearCurrentPage();
        ISalesOrderAppViewModel viewModel = (ISalesOrderAppViewModel)_dataManager.GetData("salesOrderViewModel");
        if (viewModel is not null)
        {
            _salesOrderViewModel = viewModel;
        }
        else
        {
            _salesOrderViewModel = _serviceProvider.GetRequiredService<ISalesOrderAppViewModel>();
        }

        if (!_salesOrderViewModel.IsInitialized)
        {
            try
            {
                await Task.Run(async () =>
                {
                    Winit.Modules.Store.Model.Interfaces.IStoreItemView storeViewModel =
                        (Winit.Modules.Store.Model.Interfaces.IStoreItemView)_dataManager.GetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView));

                    if (storeViewModel == null)
                    {
                        throw new Exception(@Localizer["store_view_model_is_null"]);
                    }

                    bool isNewOrder = true;
                    string salesOrderUID = string.Empty;
                    salesSummaryItemView = (ISalesSummaryItemView)_dataManager.GetData("salesSummaryItemView");
                    if (salesSummaryItemView != null)
                    {
                        salesOrderUID = salesSummaryItemView.SalesOrderUID;
                        isNewOrder = false;
                    }

                    // Fetch data from the database using an async method
                    await _salesOrderViewModel.PopulateViewModel(SourceType.APP, storeViewModel, isNewOrder, salesOrderUID);
                });
            }
            catch (Exception ex)
            {
                // Log or handle the exception accordingly
                Console.Error.WriteLine($"{@Localizer["error_initializing_sales_order_view_model"]}: {ex.Message}");
                Console.WriteLine($"{@Localizer["error_initializing_sales_order_view_model"]}: {ex.Message}");
            }
            finally
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        _salesOrderViewModel.PopulateFilterData();
        PrepareTopScrollSelectionItems();
        PrepareLeftScrollSelectionItems();
        await SetTopBar();
        IsPageLoaded = true;
        LoadResources(null, _languageService.SelectedCulture);
        stopwatch.Stop();
        TimeSpan timeTaken = stopwatch.Elapsed;
        HideLoader();
        _logger.LogInformation($"Time taken to execute OnInitializedAsync methode: {timeTaken.TotalSeconds} seconds");
        Console.WriteLine($"Time taken to execute OnInitializedAsync methode: {timeTaken.TotalSeconds} seconds");
        await jsRuntime.InvokeVoidAsync("logToConsole", $"Time taken for OnInitializedAsync methode: {timeTaken.TotalSeconds} seconds"); // Android/c# to javascript
        if (_salesOrderViewModel != null && _appUser?.SelectedCustomer?.FranchiseeOrgUID != null)
        {
            _salesOrderViewModel.SelectedDistributor = _salesOrderViewModel.DistributorsList.FirstOrDefault(p => p.UID == _appUser.SelectedCustomer.FranchiseeOrgUID);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && !_salesOrderViewModel.IsInitialized)
        {
            _salesOrderViewModel.IsInitialized = true;
            // Load data if needed
        }
    }
    // Implement IDisposable for object cleanup
    //public new void Dispose()
    //{
    //    // Dispose of objects and perform cleanup here
    //}
    public override void Dispose()
    {
        base.Dispose();
    }
    public override void UnSubscribeEvent()
    {
        CleanDataManager("");
        base.UnSubscribeEvent();
    }
    
    public async Task WinitTextBox_OnSearch(string searchStringFromComponent)
    {
        await _salesOrderViewModel.ApplySearch(searchStringFromComponent);
        if (salesOrderListViewRef is not null)
        {
            await salesOrderListViewRef.RefreshVirtualizedItems();
        }
        await InvokeAsync(StateHasChanged);
    }
    private async Task ApplySort()
    {
        List<SortCriteria> sortCriteriaList = new()
        {
            new SortCriteria("SKUName", SortDirection.Asc)
        };
        await _salesOrderViewModel.ApplySort(sortCriteriaList);
    }
    private bool ValidateSalesOrderBeforePreview()
    {
        return _salesOrderViewModel.SelectedSalesOrderItemViews == null
            || (_salesOrderViewModel.SelectedSalesOrderItemViews != null && _salesOrderViewModel.SelectedSalesOrderItemViews.Count == 0)
            ? throw new Exception(@Localizer["atleast_one_item_should_be_selected."])
            : true;
    }
    private async Task PreviewButtonClick()
    {
        try
        {
            _salesOrderViewModel.SetSelectedSalesOrderItemViews();
            await _salesOrderViewModel.CalculateCashDiscount(_salesOrderViewModel.TotalCashDiscount);
            if (ValidateSalesOrderBeforePreview())
            {
                _dataManager.SetData("salesOrderViewModel", _salesOrderViewModel);
                Navigate("salesorderpreview", "", null);
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["error"], ex.Message,null, @Localizer["ok"]);
        }
    }
    private bool showEnterPasscode = false;
    private string passcode = "12345";
    private void ShowEnterPasscode()
    {
        showEnterPasscode = true;
        StateHasChanged();
    }
    private void HandleEnterPasscodePopupClosed()
    {
        showEnterPasscode = false;
        StateHasChanged();
    }
    private void CheckPasscode(string newValue)
    {
        passcode = newValue;
        showEnterPasscode = false;
    }
    public void ApplyFilterAndSort((List<FilterCriteria> filterCriterias, List<SortCriteria> sortCriterias) data)
    {
        _salesOrderViewModel.FilterCriteriaList.Clear();
        _salesOrderViewModel.FilterCriteriaList.AddRange(data.filterCriterias);
        List<SortCriteria> sortCriterias = data.sortCriterias;
        _ = _salesOrderViewModel.ApplyFilter(_salesOrderViewModel.FilterCriteriaList, Winit.Shared.Models.Enums.FilterMode.And);
        _ = _salesOrderViewModel.ApplySort(sortCriterias);
    }
    public void AddItemToCart(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        foreach (ISalesOrderItemView itemview in _salesOrderViewModel.SalesOrderItemViews)
        {
            if (itemview.OrderQty > 0)
            {
                //_salesOrderViewModel.SelectedSalesOrderItemViews.Add(itemview);
            }
        }
    }

    // show Discount PopUp
    private async Task AddButtonOnQtyChange(QtyChangeEvent qtyChangeEvent)
    {
        Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView = _salesOrderViewModel.SalesOrderItemViews
            .Where(e => e.UID == qtyChangeEvent.UID)
            .FirstOrDefault();
        if (salesOrderItemView != null)
        {
            salesOrderItemView.Qty = qtyChangeEvent.Qty;
            await _salesOrderViewModel.OnQtyChange(salesOrderItemView);
        }
        await InvokeAsync(StateHasChanged);
    }

    private async Task<bool> PreValidateQtyChange(QtyChangeEvent qtyChangeEvent)
    {
        bool retValue = true;
        Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemViewCurrent = _salesOrderViewModel.SalesOrderItemViews
            .Where(e => e.UID == qtyChangeEvent.UID)
            .FirstOrDefault();

        if(salesOrderItemViewCurrent == null)
        { 
            return false; 
        }

        decimal currentQty = qtyChangeEvent.Qty * salesOrderItemViewCurrent.SelectedUOM.Multiplier;

        decimal sameItemWithOtherLineQty = _salesOrderViewModel.SalesOrderItemViews
            .Where(e => e.SKUUID == salesOrderItemViewCurrent.SKUUID && e.UID != qtyChangeEvent.UID)
            .Sum(e => e.QtyBU);
        
        //commented by mahir as VanQty is given 0 by default
        //if((currentQty + sameItemWithOtherLineQty) > salesOrderItemViewCurrent.VanQty)
        //{
        //    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["qty_exceeds_van_qty"], null);
        //    retValue = false;
        //}
        return retValue;
    }
    public async Task DeleteClonedProduct(ISalesOrderItemView salesOrderItemView)
    {
        await _salesOrderViewModel.RemoveItemFromList(salesOrderItemView);
        await InvokeAsync(StateHasChanged);
    }
    public List<ISelectionItem> GetSelectionItemsForUOM(ISalesOrderItemView salesOrderItemView)
    {
        return _salesOrderViewModel.GetAvailableUOMForDDL(salesOrderItemView);

    }
    public List<ISelectionItem> GetCloneUOMSelectionItems(ISalesOrderItemView salesOrderItemView)
    {
        return _salesOrderViewModel.GetAvailableUOMForClone(salesOrderItemView);
    }
    public async Task DropDownSelection_OnSingleSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView = _salesOrderViewModel.SalesOrderItemViews
                .Where(e => e.UID == dropDownEvent.UID)
                .FirstOrDefault();
            if (salesOrderItemView != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                Winit.Shared.Models.Common.ISelectionItem selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                if (selectionItem != null)
                {
                    salesOrderItemView.SelectedUOM = salesOrderItemView.AllowedUOMs.Find(uom => uom.Code == selectionItem.Code);
                    await _salesOrderViewModel.UpdateUnitPriceByUOMChange(salesOrderItemView);
                }
            }
        }
        await InvokeAsync(StateHasChanged);
    }
    public void DropDownClone_OnSingleSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView = _salesOrderViewModel.SalesOrderItemViews
            .Where(e => e.UID == dropDownEvent.UID)
            .FirstOrDefault();
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                Winit.Shared.Models.Common.ISelectionItem selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                if (selectionItem != null)
                {
                    Winit.Modules.SKU.Model.UIInterfaces.ISKUUOMView newUOM = salesOrderItemView.AllowedUOMs.Find(prod => prod.Code == selectionItem.Code);
                    ISalesOrderItemView clonedcopy = salesOrderItemView.Clone(newUOM, Winit.Shared.Models.Enums.ItemState.Cloned, newUOM.Code);
                    _ = salesOrderItemView.UsedUOMCodes.Add(newUOM.Code);
                    _ = _salesOrderViewModel.AddClonedItemToList(clonedcopy);
                }
            }
        }
    }


    #region Barcode Scanner
    private void AfterBarcodeScanned(string scannedText)
    {
        wtbSearch.UpdateValue(scannedText);
    }
    #endregion
        #region TopBar
    private async Task SetTopBar()
    {
        MainButtons buttons = new()
        {
            //UIButton1 = new Buttons() { ButtonType = ButtonType.Image, URL = "/Images/announce.png", IsVisible = true, Action = () => ShowAllPromotions = true },
            //UIButton2 = new Buttons() { ButtonType = ButtonType.Image, URL = "/Images/carbon_filter.png", IsVisible = true, Action = () => FilterDialog.ShowFilterDialog() },
            TopLabel = "Sales Order",
        };

        await Btnname.InvokeAsync(buttons);
    }

    public async Task PromotionButtonClick()
    {
        await _alertService.ShowSuccessAlert("Alert", "Work in progress");
    }
    #endregion
    public async Task HandleDontMissOutPromo()
    {
        try
        {
            _salesOrderViewModel.SetSelectedSalesOrderItemViews();
            if (ValidateSalesOrderBeforePreview())
            {
                MissedPromotionsHeaderSelectionItems.Clear();
                (List<ISelectionItem>, Dictionary<string, List<ISelectionItem>>) missedPromotionData = await _salesOrderViewModel.PrepareMissingPromotion();
                if (missedPromotionData.Item1 == null || missedPromotionData.Item2 == null)
                {
                    await PlaceOrder_Click();
                    return;
                }
                MissedPromotionsHeaderSelectionItems.AddRange(missedPromotionData.Item1);
                MissedPromotionsHeaderDetailsSelectionItems = missedPromotionData.Item2;
                ShowDontMissOutPromo = true;
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["error"], ex.Message);
        }
    }
    #region OrderPlacedPopup

    #endregion

    private void PrepareTopScrollSelectionItems()
    {
        TopScrollSelectionItems.AddRange(_salesOrderViewModel.GetTopScrollSelectionItems());

    }
    private void PrepareLeftScrollSelectionItems()
    {
        LeftScrollSelectionItems.AddRange(_salesOrderViewModel.GetLeftScrollSelectionItems());
        LeftScrollSM = new SelectionManager(LeftScrollSelectionItems.ToList<ISelectionItem>(),
            Winit.Shared.Models.Enums.SelectionMode.Single);
        LeftScrollSM.Select(LeftScrollSelectionItems.First());
    }
    private void OnTopScrollItmSelect(ISelectionItem selectionItem)
    {
        try
        {
            ShowLoader();
            selectionItem.IsSelected = !selectionItem.IsSelected;
            TopScrollSelectionItems = TopScrollSelectionItems.OrderBy(equals => !equals.IsSelected).ToList();
            if (selectionItem.IsSelected)
            {
                if ((selectionItem as SelectionItemFilter).FilterGroup == FilterGroupType.Attribute)
                {
                    _salesOrderViewModel.TopScrollFilterCriterias.Add(new FilterCriteria(selectionItem.UID,
                        selectionItem.Code, FilterType.Equal, filterGroup: FilterGroupType.Attribute));
                }
                else
                {
                    _salesOrderViewModel.TopScrollFilterCriterias.Add(new FilterCriteria(selectionItem.Code, true,
                        FilterType.Equal, typeof(bool)));
                }
            }
            else
            {
                if ((selectionItem as SelectionItemFilter).FilterGroup == FilterGroupType.Attribute)
                {
                    _ = _salesOrderViewModel.TopScrollFilterCriterias.RemoveAll(e => e.Value == selectionItem.Code);
                }
                else
                {
                    _ = _salesOrderViewModel.TopScrollFilterCriterias.RemoveAll(e => e.Name == selectionItem.Code);
                }
            }
            _ = _salesOrderViewModel.ApplyFilter(_salesOrderViewModel.TopScrollFilterCriterias,
                Winit.Shared.Models.Enums.FilterMode.And);
            StateHasChanged();
        }
        catch (Exception ex)
        {
        }
        HideLoader();
    }
    private void OnLeftScrollItmSelect(ISelectionItem selectionItem)
    {
        if (!selectionItem.IsSelected)
        {
            try
            {
                ShowLoader();
                LeftScrollSM.Select(selectionItem);
                TopScrollSelectionItems.Clear();
                TopScrollSelectionItems.AddRange(_salesOrderViewModel.GetTopScrollSelectionItems
                    (selectionItem.Code == "All" ? null : selectionItem.Code));
                _salesOrderViewModel.TopScrollFilterCriterias.Clear();
                _salesOrderViewModel.LeftScrollFilterCriteria = selectionItem.Code == "All" ? null : new FilterCriteria(selectionItem.UID, selectionItem.Code,
                    FilterType.Equal, filterGroup: FilterGroupType.Attribute);
                _salesOrderViewModel.ApplyFilter(_salesOrderViewModel.TopScrollFilterCriterias,
                    Winit.Shared.Models.Enums.FilterMode.And);
                StateHasChanged();
            }
            catch (Exception ex)
            {
            }
            HideLoader();
        }
    }
    private async Task HandleUnitPriceChange((ISalesOrderItemView item, decimal price) data)
    {
        await _salesOrderViewModel.UpdateUnitPrice(data.item, data.price);

        await InvokeAsync(StateHasChanged);
    }
    private void HandlePromotionClick(ISalesOrderItemView salesOrderItemView)
    {
        _ = OnGetPromotionsForItem(salesOrderItemView);
        ShowPromotionPopUp = true;
        StateHasChanged();
    }
    private List<ISelectionItem> OnGetPromotionsForItem(ISalesOrderItemView salesOrderItemView)
    {
        PromotionDetails.Clear();
        List<ISelectionItem> data = _salesOrderViewModel.GetPromotionListForItem(salesOrderItemView);
        if (data is not null)
        {
            PromotionDetails.AddRange(data);
        }

        return PromotionDetails;
    }
    private async Task OnPromotionsOrderClick(string promotionUID)
    {
        ShowAllPromotions = false;
        SelectedPromotion = null;
        _salesOrderViewModel.SelectedPromotionUID = promotionUID;
        _ = _salesOrderViewModel.DMSPromotionDictionary.TryGetValue(promotionUID, out SelectedPromotion);
        await _salesOrderViewModel.ApplySpecialPromotionFilter();
    }
    private async Task HandleSpecialPromotionRemove()
    {
        SelectedPromotion = null;
        _salesOrderViewModel.SelectedPromotionUID = null;
        await _salesOrderViewModel.RemoveSpecialPromotionFilter();
    }
    ~CreateSalesOrder()
    {
        
    }
    
    private void OnImageDeleteClick(string fileName)
    {
        IFileSys fileSys = ImageFileSysList.Find
            (e => e.FileName == fileName);
        if (fileSys is not null) ImageFileSysList.Remove(fileSys);
    }
    private async Task OnImageCapture((string fileName, string folderPath) data)
    {
        string relativePath = "";

        // Ensure UID is available and not empty
        string selectedCustomerCode = _appUser?.SelectedJobPosition?.UID;

        if (!string.IsNullOrWhiteSpace(selectedCustomerCode))
        {
            relativePath = FileSysTemplateControles.GetCaptureCapitatorImageFolderPath(selectedCustomerCode) ?? "";
        }

        IFileSys fileSys = ConvertFileSys("ProductFeedback", "12", "ProductFeedback", "Image",
            data.fileName, _appUser.Emp?.Name, data.folderPath);
        fileSys.SS = -1;
        fileSys.RelativePath = relativePath;
        ImageFileSysList.Add(fileSys);
        FolderPathImages = data.folderPath;
        await Task.CompletedTask;
    }
}