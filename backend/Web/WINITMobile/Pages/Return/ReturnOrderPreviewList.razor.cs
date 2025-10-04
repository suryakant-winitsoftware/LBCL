using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Printing.BL.Classes.SalesOrder;
using Winit.Modules.Printing.BL.Classes;
using Winit.Modules.Printing.BL.Interfaces;
using Winit.Modules.Printing.Model.Enum;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using WINITMobile.Models.TopBar;
using WINITMobile.Pages.Base;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
namespace WINITMobile.Pages.Return;


partial class ReturnOrderPreviewList : BaseComponentBase
{

    public List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> ReturnOrderItemViews { get; set; }
    [Parameter]
    public List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> DisplayReturnOrderItemViews { get; set; } =
    new List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView>();

    private IStoreItemView returnOrderHeaderDetails;

    private List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine> returnOrderSelectedItemLines;
    private IStoreItemView SelectedStoreDetails { get; set; }

    // IReturnSummaryItemView

    [Parameter]
    public string ReturnOrderUID { get; set; }
    [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }
    public bool IsInitialized = false;
    public bool IsSignatureView { get; set; }
    private bool IsOrderPlacedPopupVisible = false;
    private List<string> _propertiesToSearch = new List<string>
    {
        "SKUCode",
        "SKUName"
    };
    protected override async Task OnInitializedAsync()
    {
        if(_appUser != null)
        {
            SelectedStoreDetails = _appUser.SelectedCustomer;
        }
        if (ReturnOrderUID != null)
        {
            var returnOrderLines = await getReturnOrdersFromBL();
            if (returnOrderLines != null)
            {
                ReturnOrderItemViews = ConvertToIReturnOrderItemView(returnOrderLines);
                DisplayReturnOrderItemViews.Clear();
                DisplayReturnOrderItemViews.AddRange(ReturnOrderItemViews);
                await SetReturnOrderPreviewViewTopBar();
            }
        }
        else
        {
            ReturnOrderItemViews = _PageState.ReturnOrderViewModel.DisplayedReturnOrderItemViews.Where(i => i.IsSelected).ToList();
            DisplayReturnOrderItemViews.Clear();
            DisplayReturnOrderItemViews.AddRange(ReturnOrderItemViews);
            await SetReturnOrderPreviewTopBar();
        }
        //ReturnOrderItemViews = _PageState.ReturnOrderViewModel.DisplayedReturnOrderItemViews.Where(i => i.IsSelected).ToList();
        //DisplayReturnOrderItemViews.Clear();
        //DisplayReturnOrderItemViews.AddRange(ReturnOrderItemViews);
        //await SetReturnOrderPreviewTopBar();
        LoadResources(null, _languageService.SelectedCulture);
        IsInitialized = true;
    }

    async Task SetReturnOrderPreviewTopBar()
    {
        MainButtons buttons = new MainButtons()
        {
            UIButton1 = new Buttons()
            {
                ButtonType = ButtonType.Text,
                ButtonText = @Localizer["confirm"],
                IsVisible = true,
                Action = () => TakeSignatures()
            },
            TopLabel = @Localizer["return_order_preview"],
        };
        await Btnname.InvokeAsync(buttons);
    }
    void TakeSignatures()
    {
        IsSignatureView = true;
        StateHasChanged();
    }
    async Task SetReturnOrderPreviewViewTopBar()
    {
        MainButtons buttons = new MainButtons()
        {

            TopLabel = @Localizer["return_order_preview"],
        };
        await Btnname.InvokeAsync(buttons);
    }
    public async Task OnSearching(string searchValue)
    {
        DisplayReturnOrderItemViews.Clear();
        DisplayReturnOrderItemViews.AddRange(await _filter.ApplySearch<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView>(
                ReturnOrderItemViews, searchValue, _propertiesToSearch));
    }
    public async Task OnConfirmPopUpOkClick()
    {
        if (await _PageState.ReturnOrderViewModel.SaveOrder())
        {
           // await _alertService.ShowSuccessAlert("Order Status", "order posted Successfully.");
            IsOrderPlacedPopupVisible = true;
        }
        else await _alertService.ShowErrorAlert(@Localizer["order_status"], @Localizer["failed_to_post_the_order"]);
    }
    private async Task<List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine>> getReturnOrdersFromBL()
    {
        try
        {
            List<Winit.Shared.Models.Enums.FilterCriteria> filterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>
        {
            new Winit.Shared.Models.Enums.FilterCriteria("Return_Order_UID",ReturnOrderUID,Winit.Shared.Models.Enums.FilterType.Equal)
        };
            PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine> pagedResponse =
            await _ReturnOrderLineBL.SelectAllReturnOrderLineDetails(null, 0, 0, filterCriterias, false);
            return pagedResponse.PagedData.ToList();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> ConvertToIReturnOrderItemView(List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine> returnOrderLines)
    {
        List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> returnOrderItemViews = null;
        if (returnOrderLines != null && returnOrderLines.Count > 0)
        {
            returnOrderItemViews = new List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView>();
            foreach (var returnOrderLine in returnOrderLines)
            {
                Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView = ConvertToIReturnOrderItemView(returnOrderLine);
                //ToDo
                returnOrderItemViews.Add(returnOrderItemView);
            }
        }
        return returnOrderItemViews;
    }
    public Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView ConvertToIReturnOrderItemView(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine returnOrderLine)
    {
        Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView = _serviceProvider.CreateInstance<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView>();
        returnOrderItemView.SKUCode = returnOrderLine.SKUCode;
        returnOrderItemView.BaseUOM = returnOrderLine.BaseUOM;
        returnOrderItemView.OrderQty = returnOrderLine.Qty;
        returnOrderItemView.ReasonText = returnOrderLine.ReasonText;
        returnOrderItemView.PONumber = returnOrderLine.PONumber;
        returnOrderItemView.UOMLabel = returnOrderLine.UoM;
        return returnOrderItemView;
    }
    private async Task OrderPlacedPopup_CloseClicked()
    {
        IsOrderPlacedPopupVisible = false;
        await NavigateToCustomerDashboard();
    }
    private async Task NavigateToCustomerDashboard()
    {
        // Navigate("Summary_DeliverySummary", "", null, true);
        NavigateTo("CustomerCall");
    }
    private async Task OrderPlacedPopup_ShareClicked()
    {

        await OrderPlacedPopup_CloseClicked();
    }
    private async Task OrderPlacedPopup_PrintClicked()
    {

        if(true)          //(CheckPrinterAvailableOrNot())
        {
            // Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster> SelectReturnOrderMasterByUIDTest(string UID);
            returnOrderHeaderDetails = SelectedStoreDetails;
            returnOrderSelectedItemLines = await GetSelectedItemDetailsFromBl(_PageState.ReturnOrderViewModel.ReturnOrderUID);

            IPrint returnOrderPrint = new Winit.Modules.Printing.BL.Classes.ReturnOrder.ReturnOrderPrint();

            string printerTypeString = _storageHelper.GetStringFromPreferences("PrinterTypeOrBrand");
            string printerSizeString = _storageHelper.GetStringFromPreferences("PrinterPaperSize");
            // Now convert the strings to the corresponding enum values
            PrinterType printerType = (PrinterType)Enum.Parse(typeof(PrinterType), printerTypeString);
            PrinterSize printerPaperSize = (PrinterSize)Enum.Parse(typeof(PrinterSize), printerSizeString);

            string returnOrderPrintString = returnOrderPrint.CreatePrintString(printerType, printerPaperSize, (returnOrderHeaderDetails, returnOrderSelectedItemLines));

            Winit.Modules.Printing.BL.Interfaces.IPrinter userPrinter = Winit.Modules.Printing.BL.Factory.PrinterFactory.CreatePrinter(_storageHelper.GetStringFromPreferences("PrinterMacAddresses"), printerType, _storageHelper.GetStringFromPreferences("PrinterMacAddresses"));
            if (userPrinter.Type == PrinterType.Zebra)
            {
                await ((ZebraPrinter)userPrinter).Print(returnOrderPrintString);
            }
            await OrderPlacedPopup_CloseClicked();
        }
        else
        {
            await _alertService.ShowErrorAlert(@Localizer["no_printer"], @Localizer["no_printer_selected_at_printer_service_for_printing"]);
        }
    }

    private async Task<List<IReturnOrderLine>> GetSelectedItemDetailsFromBl(string returnOrderUID)
    {
        try
        {

            Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster returnOrderMaster = await _ReturnOrderBL.GetReturnOrderMasterByUID(returnOrderUID);
            return returnOrderMaster.ReturnOrderLineList;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private bool CheckPrinterAvailableOrNot()
    {
        if (String.IsNullOrEmpty(_btinfo.PrinterType.ToString()) || String.IsNullOrEmpty(_btinfo.PrinterSize.ToString()) || String.IsNullOrEmpty(_btinfo.macaddress))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public async Task HandleSignature_ProceedClick()
    {
        IsSignatureView = false;
        _PageState.ReturnOrderViewModel.IsSignaturesCaptured = true;
        IFileSys customerFileSys = ConvertFileSys(@Localizer["returnorder"], _PageState.ReturnOrderViewModel.ReturnOrderUID, @Localizer["receivedbysignature"], "Image",
            _PageState.ReturnOrderViewModel.CustomerSignatureFileName, _appUser.Emp?.Name, _PageState.ReturnOrderViewModel.CustomerSignatureFolderPath);
        IFileSys userFileSys = ConvertFileSys(@Localizer["returnorder"], _PageState.ReturnOrderViewModel.ReturnOrderUID, @Localizer["deliveredbysignature"], "Image",
            _PageState.ReturnOrderViewModel.UserSignatureFileName, _appUser.Emp?.Name, _PageState.ReturnOrderViewModel.UserSignatureFolderPath);
        if (customerFileSys is not null)
        {
            _PageState.ReturnOrderViewModel.SignatureFileSysList.Add(customerFileSys);
        }
        if (userFileSys is not null)
        {
            _PageState.ReturnOrderViewModel.SignatureFileSysList.Add(userFileSys);
        }
        _PageState.ReturnOrderViewModel.OnSignatureProceedClick();
        await OnConfirmPopUpOkClick();
    }



    
}
