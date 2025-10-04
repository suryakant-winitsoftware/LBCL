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
using Winit.Modules.StoreActivity.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using WINITMobile.Pages.Base;
using WINITMobile.Services;

namespace WINITMobile.Pages.Sales;

public class SalesOrderBase : BaseComponentBase
{
    public Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderAppViewModel _salesOrderViewModel;
    public bool IsSignatureView = false;
    public bool IsOrderPlacedPopupVisible = false;
    [Inject]
    public Winit.Modules.StoreActivity.BL.Interfaces.IStoreActivityViewModel _StoreActivityViewmodel { get; set; }
    [Inject]
    public SecureStorageHelper _storageHelper { get; set; }
    [Inject]
    public Winit.Modules.Printing.Model.Classes.BluetoothDeviceInfo _btinfo { get; set; }
    private Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView salesOrderHeader;
    private List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView> salesOrderLines;
    public bool ShowDontMissOutPromo = false;

    public override void Dispose()
    {
        base.Dispose();
        // Disposed unmanaged resources
    }
    public override void UnSubscribeEvent()
    {

    }

    public async Task PlaceOrder_Click()
    {
        ShowDontMissOutPromo = false;
        _salesOrderViewModel.Status = SalesOrderStatus.DELIVERED;
        _salesOrderViewModel.SetSelectedSalesOrderItemViews();
        if (!ValidateData())
        {
            // Validation errors, show an error popup
            await _alertService.ShowErrorAlert(@Localizer["error"], string.Join(",", ValidationErrors));
        }
        else
        {
            //await _alertService.ShowConfirmationAlert(@Localizer["order_confirmation"],
            //    "Are you sure you want to confirm the secondary sales order?", async (param) => await ToggleSignature(),@Localizer["yes"], @Localizer["no"]);
            await _alertService.ShowConfirmationAlert(@Localizer["order_confirmation"],
                "Are you sure you want to confirm the sales capture?", async (param) => await ToggleSignature(), @Localizer["yes"], @Localizer["no"]);
        }
    }

    public async Task Distributor_Click()
    {
        if (_salesOrderViewModel.DistributorsList != null && _salesOrderViewModel.DistributorsList.Count > 0)
        {
            await _dropdownService.ShowMobilePopUpDropDown(new Winit.UIComponents.Common.Services.DropDownOptions
            {
                DataSource = _salesOrderViewModel.DistributorsList,
                OnSelect = async (eventArgs) =>
                {
                    await SelectDistributor(eventArgs);
                },
                OkBtnTxt = @Localizer["submit"],
                Title = "distributor",
                IsSearchable = true
            });
        }
        else
        {
            await _alertService.ShowErrorAlert(@Localizer["error"], "distributor list is empty");
        }

    }
    public async Task SelectDistributor(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        try
        {
            if (dropDownEvent == null || dropDownEvent.SelectionItems == null || !dropDownEvent.SelectionItems.Any())
            {
                throw new ArgumentException("Invalid or empty distributor selection.");
            }
            string currentFranchiseeOrgUID = _salesOrderViewModel.SelectedDistributor.UID;
            ISelectionItem selectedItem = dropDownEvent.SelectionItems
                .FirstOrDefault(item => item.UID == currentFranchiseeOrgUID);

            // If no match found, default to the first item
            selectedItem ??= dropDownEvent.SelectionItems.First();

            // Mark the selected item in the dropdown
            foreach (var item in dropDownEvent.SelectionItems)
            {
                item.IsSelected = item == selectedItem;
            }

            // Assign the selected distributor to the user context
            if (selectedItem != null)
            {
                _salesOrderViewModel.SelectedDistributor = selectedItem;
            }
            else
            {
                throw new InvalidOperationException("No valid distributor found to select.");
            }
        }
        catch (Exception ex)
        {
            HideLoader();
            _alertService.ShowErrorAlert("Error", ex.Message);
        }

        await Task.CompletedTask;
    }
    #region Validation
    private bool ValidateData()
    {
        bool isValid = true;

        // Reset any previous error messages
        ClearValidationError();

        if (_salesOrderViewModel.SelectedSalesOrderItemViews == null)
        {
            ValidationErrors.Add(@Localizer["sales_order_can't_be_null."]);
            isValid = false;
        }
        // Item Count Validation
        else if (_salesOrderViewModel.SelectedSalesOrderItemViews != null
            && _salesOrderViewModel.SelectedSalesOrderItemViews.Count == 0)
        {
            ValidationErrors.Add(@Localizer["atleast_one_item_should_be_selected."]);
            isValid = false;
        }
        // NetAmount
        else if (_salesOrderViewModel.NetAmount <= 0)
        {
            ValidationErrors.Add(@Localizer["order_value_can_not_be_less_than_zero(0)."]);
            isValid = false;
        }
        return isValid;
    }
    #endregion
    private async Task ToggleSignature()
    {
        await PlaceOrderClicked(SalesOrderStatus.DELIVERED);
        /*
         * -- Below commented temporarily for Farmley
        if (!_salesOrderViewModel.IsSignaturesCaptured && _salesOrderViewModel.SelectedStoreViewModel.IsCaptureSignatureRequired)
        {
            IsSignatureView = true;
            StateHasChanged();
        }
        else
        {
            await PlaceOrderClicked(SalesOrderStatus.DELIVERED);
        }
        */
    }
    public async Task HandleSignature_ProceedClick()
    {
        IsSignatureView = false;
        _salesOrderViewModel.IsSignaturesCaptured = true;
        IFileSys customerFileSys = ConvertFileSys(@Localizer["salesorder"], _salesOrderViewModel.SalesOrderUID, @Localizer["receivedbysignature"], "Image",
            _salesOrderViewModel.CustomerSignatureFileName, _appUser.Emp?.Name, _salesOrderViewModel.CustomerSignatureFolderPath);
        IFileSys userFileSys = ConvertFileSys(@Localizer["salesorder"], _salesOrderViewModel.SalesOrderUID, @Localizer["deliveredbysignature"], "Image",
            _salesOrderViewModel.UserSignatureFileName, _appUser.Emp?.Name, _salesOrderViewModel.UserSignatureFolderPath);
        if (customerFileSys is not null)
        {
            _salesOrderViewModel.SignatureFileSysList.Add(customerFileSys);
        }
        if (userFileSys is not null)
        {
            _salesOrderViewModel.SignatureFileSysList.Add(userFileSys);
        }
        _salesOrderViewModel.OnSignatureProceedClick();
        await PlaceOrderConfirmed();
    }
    private async Task PlaceOrderConfirmed()
    {
        _salesOrderViewModel.Status = SalesOrderStatus.DELIVERED;
        await PlaceOrderClicked(_salesOrderViewModel.Status);
    }
    protected async Task OnSaveConfirmed()
    {
        _salesOrderViewModel.Status = SalesOrderStatus.DRAFT;
        await PlaceOrderClicked();
    }
    private async Task PlaceOrderClicked(string Status = SalesOrderStatus.DRAFT)
    {
        try
        {
            await _salesOrderViewModel.CalculateCashDiscount(_salesOrderViewModel.TotalCashDiscount);
            bool retValue = await _salesOrderViewModel.SaveSalesOrder(Status);
            _dataManager.DeleteData("salesOrderViewModel");
            if (retValue)
            {
                if (_salesOrderViewModel.Status == SalesOrderStatus.DELIVERED)
                {
                    IsOrderPlacedPopupVisible = true;
                }
                else
                {
                    await _alertService.ShowSuccessAlert(@Localizer["success"], PrepareSuccessMessage(),
                        (param) => NavigateToCustomerDashboard());
                }
            }
            else
            {
                throw new Exception("Error while saving order");
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["error"], ex.Message);
        }
    }
    public void OrderPlacedPopup_CloseClicked()
    {

        IsOrderPlacedPopupVisible = false;
        _ = NavigateToCustomerDashboard();
    }
    public void OrderPlacedPopup_PaymentClicked()
    {
        OrderPlacedPopup_CloseClicked();
    }
    public void OrderPlacedPopup_EmailClicked()
    {
        OrderPlacedPopup_CloseClicked();
    }
    protected async Task NavigateToCustomerDashboard()
    {
        // Navigate("Summary_DeliverySummary", "", null, true);
        string StoreActivityHistoryUid = (string)_dataManager.GetData("StoreActivityHistoryUid");
        if (_appUser.SelectedCustomer != null)
        {
            _ = await _StoreActivityViewmodel.UpdateStoreActivityHistoryStatus(StoreActivityHistoryUid, Winit.Modules.Base.Model.CommonConstant.COMPLETED);
        }
        NavigateTo("CustomerCall");
        await Task.CompletedTask;
    }
    private string PrepareSuccessMessage()
    {
        string message = string.Empty;
        switch (_salesOrderViewModel.Status)
        {
            case SalesOrderStatus.DELIVERED:
                message = @Localizer["order_delivered_successfully"];
                break;
            case SalesOrderStatus.DRAFT:
                message = @Localizer["order_saved_successfully"];
                break;
            default:
                break;
        }
        return message;
    }
    public void OrderPlacedPopup_ShareClicked()
    {

        OrderPlacedPopup_CloseClicked();
    }
    public async Task OrderPlacedPopup_PrintClicked()
    {
        if (CheckPrinterAvailableOrNot())
        {
            salesOrderHeader = await _salesOrderViewModel.GetSalesOrderHeaderDetails();
            salesOrderLines = await _salesOrderViewModel.GetSalesOrderlines();

            // salesOrderStoreAddressLines = await GetSalesOrderAddressDetailsFromBL();
            // _ = _salesOrderViewModel.StoreUID;
            IPrint salesOrderPrint = new SalesOrderPrint();
            string printerTypeString = _storageHelper.GetStringFromPreferences("PrinterTypeOrBrand");   // "Zebra";  //
            string printerSizeString = _storageHelper.GetStringFromPreferences("PrinterPaperSize");    //"FourInch"; // 

            // Now convert the strings to the corresponding enum values
            PrinterType printerType = (PrinterType)Enum.Parse(typeof(PrinterType), printerTypeString);
            PrinterSize printerPaperSize = (PrinterSize)Enum.Parse(typeof(PrinterSize), printerSizeString);
            string salesOrderPrintString = salesOrderPrint.CreatePrintString(printerType, printerPaperSize, (salesOrderHeader, salesOrderLines));
            //string salesOrderPrintString = salesOrderPrint.CreatePrintString(printerType, printerPaperSize, _salesOrderViewModel.SelectedSalesOrderItemViews);
            Winit.Modules.Printing.BL.Interfaces.IPrinter userPrinter = Winit.Modules.Printing.BL.Factory.PrinterFactory.CreatePrinter(_storageHelper.GetStringFromPreferences("PrinterMacAddresses"), printerType, _storageHelper.GetStringFromPreferences("PrinterMacAddresses"));
            if (userPrinter.Type == PrinterType.Zebra)
            {
                await ((ZebraPrinter)userPrinter).Print(salesOrderPrintString);
            }
            OrderPlacedPopup_CloseClicked();
        }
        else
        {
            await _alertService.ShowErrorAlert(@Localizer["no_printer"], @Localizer["no_printer_selected_at_printer_service_for_printing"]);
        }
    }
    private bool CheckPrinterAvailableOrNot()
    {
        return true;             //!string.IsNullOrEmpty(_btinfo.PrinterType.ToString()) && !string.IsNullOrEmpty(_btinfo.PrinterType.ToString()) && !string.IsNullOrEmpty(_btinfo.PrinterType.ToString());
    }
}
