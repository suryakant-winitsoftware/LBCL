using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.CollectionModule.BL.Classes.CreatePayment;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Printing.BL.Classes;
using Winit.Modules.Printing.BL.Classes.CollectionOrder;
using Winit.Modules.Printing.BL.Interfaces;
using Winit.Modules.Printing.Model.Enum;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Services;
using WINITMobile.Data;
using WINITMobile.Models.TopBar;
using WINITMobile.Pages.Base;
using static Winit.Modules.CollectionModule.BL.Classes.CreatePayment.CreatePaymentAppViewModel;
namespace WINITMobile.Pages.Collection
{
    public partial class CollectPayment : BaseComponentBase
    {
        private string ReceiptNumber { get; set; } = "";
        private string InvoiceNumber { get; set; } = "4585101AFARG451R";
        private string TargetUID { get; set; } = "";
        private decimal Amount { get; set; } = 0;
        private decimal StaticAmount { get; set; } = 0;
        private decimal Invoice { get; set; } = 0;
        private decimal Collected { get; set; } = 0;
        private decimal RemainingAmt { get; set; } = 0;
        private decimal StaticRemainingAmt { get; set; } = 0;
        private decimal CreditNoteAmount { get; set; } = 0;
        private decimal CollectionAmount { get; set; } = 0;
        private decimal CreditNoteAmountCopy { get; set; } = 0;
        private decimal TotalRemainingAmt { get; set; } = 0;

        private string PayingAmount { get; set; } = "";
        private string InvoiceType { get; set; } = "Invoice";
        private decimal Text { get; set; } = 0;
        private static int _checkedCount { get; set; } = 0;
        private bool CashText { get; set; } = false;
        private bool IsInitialised { get; set; } = false;
        private bool onAccountflag { get; set; } = false;
        private bool Receiptflag { get; set; } = false;
        private bool Onaccountflag { get; set; } = false;
        private DateTime _collectedDate { get; set; } = DateTime.Now;
        [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }
        private Dictionary<string, bool> checkboxStates = new Dictionary<string, bool>();
        private List<IAccPayable> SelectedItems { get; set; } = new List<IAccPayable>();
        private static Winit.Modules.CollectionModule.Model.Interfaces.ICollections collection = new Collections();
        private static Winit.Modules.CollectionModule.Model.Interfaces.IAccBank accbank = new AccBank();
        private List<IBank> DropDown { get; set; } = new List<IBank>();
        private List<ISetting> Payments { get; set; } = new List<ISetting>();
        private Dictionary<string, bool> showFields { get; set; } = new Dictionary<string, bool>();
        public List<PaymentInfo> paymentInfos { get; set; } = new List<PaymentInfo>();
        private Dictionary<string, bool> selectedPayments { get; set; } = new Dictionary<string, bool>{
        { "Cash", false },   // Initially not checked
        { "Cheque", false } , // Initially not checked
        { "POS", false } , // Initially not checked
        { "Online", false }  // Initially not checked
        // Add other payment options as needed
    };
        [Parameter] public CreatePayment _createpayment { get; set; }

        //public Sample sample { get; set; }
        public List<ICollectionPrint> CollectionOrderPaymentDetails { get; set; } = new List<ICollectionPrint>();
        public List<ICollectionPrintDetails> CollectionOrderPaymentList { get; set; } = new List<ICollectionPrintDetails>();
        MultiCurrencyPopUp multi { get; set; }
        public decimal ExtraAmount { get; set; } = 0;
        public bool IsSignatureView { get; set; } = false;
        private readonly IAppConfig _appConfig;
        public string ImgFolderPath { get; set; }
        public bool IsShowMultiCurrency { get; set; } = false;
        public string PaymentMode { get; set; } = "";
        public List<IExchangeRate> CurrencyRateRecords { get; set; } = new List<IExchangeRate>();
        public List<ModeType> PaymentType { get; set; } = new List<ModeType>();
        //public MultiCurrencyPopUp multi { get; set; }
        public bool IsOrderPlacedPopupVisible { get; set; } = false;
        public string ReceiptNo { get; set; } = "";

        private FileCaptureData fileCaptureData = new FileCaptureData
        {
            AllowedExtensions = new List<string> { ".jpg", ".png" }, // Add allowed extensions
            IsCameraAllowed = true,
            IsGalleryAllowed = true,
            MaxNumberOfItems = 5,
            MaxFileSize = 10 * 1024 * 1024, // 10 MB
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
            _backbuttonhandler.ClearCurrentPage();
            _backbuttonhandler.SetCurrentPage(this);
            _createPaymentAppViewModel.Discrepency = GetParameterValueFromURL("Discrepency");
            _createPaymentAppViewModel.Code = GetParameterValueFromURL("Code");
            _createPaymentAppViewModel.Name = GetParameterValueFromURL("Name");
            await _createPaymentAppViewModel.GetBank();
            DropDown = _createPaymentAppViewModel.Banks;
            await PopulateData();
            Payments = await _createPaymentAppViewModel.GetSettings();
            ImgFolderPath = Path.Combine(_appConfigs.BaseFolderPath,
            FileSysTemplateControles.GetReturnOrderImageFolderPath(_dataManager.GetData("ReceiptNumber").ToString()));
            await BindPaymentModes();
            IsInitialised = true;
            LoadResources(null, _languageService.SelectedCulture);
        }
        public async Task BindPaymentModes()
        {
            try
            {
                foreach (var paymentType in Payments)
                {
                    paymentInfos.Add(new PaymentInfo { PaymentType = await MapPaymentModes(paymentType) });
                    PaymentType.Add(new ModeType { Name = await MapPaymentModes(paymentType) });
                }
                PaymentType = PaymentType.OrderBy(p => p.Order).ToList();
            }
            catch (Exception ex)
            {

            }
            LoadResources(null, _languageService.SelectedCulture);
        }
        public async Task<string> MapPaymentModes(ISetting paymentType)
        {
            try
            {
                switch (paymentType.Name)
                {
                    case PaymentModes.ENABLE_PAYMENT_MODE_CASH:
                        return PaymentModes.CASH;
                    case PaymentModes.ENABLE_PAYMENT_MODE_CHEQUE:
                        return PaymentModes.CHEQUE;
                    case PaymentModes.ENABLE_PAYMENT_MODE_POS:
                        return PaymentModes.POS;
                    case PaymentModes.ENABLE_PAYMENT_MODE_ONLINE:
                        return PaymentModes.ONLINE;
                    default:
                        return String.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new();
            }
        }
        public async Task PopulateData()
        {
            try
            {
                dynamic obj = _dataManager.GetData(nameof(IAccPayable));
                if (obj != null)
                {
                    SelectedItems = obj;
                    _dataManager.DeleteData(nameof(IAccPayable));

                    decimal invoiceAmount = SelectedItems.Where(p => p.SourceType.Contains("INVOICE")).Sum(p => p.PayingAmount);
                    decimal creditNoteAmount = SelectedItems.Where(p => p.SourceType.Contains("CREDITNOTE")).Sum(p => p.PayingAmount);
                    List<IAccPayable> invoices = SelectedItems.Where(item => item.SourceType.Contains("INVOICE")).ToList();
                    List<IAccPayable> creditNotes = SelectedItems.Where(item => item.SourceType.Contains("CREDITNOTE")).ToList();
                    SelectedItems = invoices.Concat(creditNotes).ToList();
                    PayingAmount = (invoiceAmount - creditNoteAmount).ToString();
                    PopulateAttributes();
                }
                if (PayingAmount == "")
                {
                    PayingAmount = _createPaymentAppViewModel.Discrepency;
                    PopulateAttributes();
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }
        }

        public async Task OpenCloseMultiCurrency(string PaymentType)
        {
            try
            {
                PaymentMode = PaymentType;
                IsShowMultiCurrency = !IsShowMultiCurrency;
                if (IsShowMultiCurrency)
                {
                    await multi.OnInit(PaymentMode);
                }
                StateHasChanged();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_navigate.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }
        public void PopulateAttributes()
        {
            Amount = Convert.ToDecimal(PayingAmount);
            StaticAmount = Amount;
            Invoice = Amount;
            RemainingAmt = Amount;
            StaticRemainingAmt = Amount;
            TotalRemainingAmt = Amount;
        }
        async Task SetTopBar()
        {
            MainButtons buttons = new MainButtons()
            {
                UIButton1 = new Buttons()
                {
                    ButtonType = ButtonType.Text,
                    ButtonText = @Localizer["confirm"],
                    IsVisible = true,
                    Action = Redirect
                },
                TopLabel = @Localizer["payment"]
            };
            await Btnname.InvokeAsync(buttons);
        }
        private async void ToggleSelection(string item)
        {
            PaymentMode = item;
            bool currentState = GetCheckboxState(item);
            if (currentState)
            {
                await multi.AdjustMultiCurrencyAmount(item);
            }
            SetCheckboxState(item, !currentState);
            if (item == "Cash")
            {
                CashText = !CashText;
            }
            //fields related 
            if (!showFields.ContainsKey(item))
            {
                showFields[item] = false; // Initialize if not present
            }
            showFields[item] = !showFields[item];
            if (selectedPayments.ContainsKey(item))
            {
                selectedPayments[item] = !selectedPayments[item];
            }
            else
            {
                // Handle error or provide default behavior
            }
            //additional
            _checkedCount = GetNumberOfCheckedCheckboxes();
            paymentInfos.FirstOrDefault(p => p.PaymentType == item).IsChecked = !paymentInfos.FirstOrDefault(p => p.PaymentType == item).IsChecked;
            if (!_appSetting.Payment_Multicurrency_Allowed)
            {
                DirectCheck(!currentState, item);
            }

        }

        public bool IsDisable()
        {
            decimal invoiceAmount = SelectedItems.Where(p => p.SourceType.Contains("INVOICE")).Sum(p => p.PayingAmount);
            decimal creditNoteAmount = SelectedItems.Where(p => p.SourceType.Contains("CREDITNOTE")).Sum(p => p.PayingAmount);
            if (invoiceAmount - creditNoteAmount == 0 && _createPaymentAppViewModel.Discrepency == "0")
            {
                return true;
            }
            return false;
        }

        private int GetNumberOfCheckedCheckboxes()
        {
            return selectedPayments.Count(kv => kv.Value);
        }

        public bool GetCheckboxState(string uid)
        {
            return checkboxStates.TryGetValue(uid, out bool state) ? state : false;
        }
        public void SetCheckboxState(string uid, bool state)
        {
            checkboxStates[uid] = state;
        }
        public async void OnChangeTextbox(string value, string payment)
        {
            paymentInfos.FirstOrDefault(p => p.PaymentType == payment).Amount = Convert.ToDecimal(value == "" ? 0 : value);
            Collected = paymentInfos.Sum(p => p.Amount);
            RemainingAmt = TotalRemainingAmt - Collected;
        }
        public void DirectCheck(bool flag, string value)
        {
            if (SelectedItems.Count == 0 && Convert.ToDecimal(_createPaymentAppViewModel.Discrepency) > 0)
            {
                paymentInfos.FirstOrDefault(p => p.PaymentType == value).Amount = Convert.ToDecimal(_createPaymentAppViewModel.Discrepency);
            }
            else
            {
                if (flag)
                {
                    paymentInfos.FirstOrDefault(p => p.PaymentType == value).Amount = RemainingAmt;
                    Collected += RemainingAmt;
                    RemainingAmt -= RemainingAmt;
                }
                else
                {
                    Collected -= paymentInfos.FirstOrDefault(p => p.PaymentType == value).Amount;
                    RemainingAmt += paymentInfos.FirstOrDefault(p => p.PaymentType == value).Amount;
                    paymentInfos.FirstOrDefault(p => p.PaymentType == value).Amount = 0;
                }
            }

        }

        //random strings
        public string Guidstring()
        {
            return (Guid.NewGuid()).ToString();
        }


        //random strings

        public async void Redirect()
        {
            try
            {
                if (!await CheckCashCollectionLimit())
                {
                    await _alertService.ShowErrorAlert(@Localizer["alert"], "Your Cash collection limit exceeds, please make a new Request to increase your limit.");
                    return;
                }
                bool response = await CheckAllValidations();
                if (!response)
                {
                    return;
                }
                if (paymentInfos.Any(p => p.IsChecked && p.PaymentType.Contains(PaymentModes.CHEQUE)))
                {
                    await ShowCheckList();
                }
                else
                {
                    await ShowSignature();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task ShowCheckList()
        {
            try
            {
                List<ISelectionItem> Check_list = _appUser.ReasonDictionary["PAYMENT_CHECK_LIST"];
                await _dropdownService.ShowMobilePopUpDropDown(new DropDownOptions
                {
                    DataSource = Check_list,
                    SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Multiple,
                    OnSelect = async (eventArgs) =>
                    {
                        await Check_List_Data(eventArgs, Check_list);
                    },
                    OkBtnTxt = @Localizer["submit"],
                    Title = @Localizer["payment_check_list"]
                });
                //await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Check_List_Data(Winit.Shared.Models.Events.DropDownEvent dropDownEvent, List<ISelectionItem> Check_list)
        {
            try
            {
                if (Check_list.Any(p => p.IsSelected))
                {
                    Dictionary<string, bool> ChecklistData = Check_list.ToDictionary(p => p.Label, p => p.IsSelected);
                    //List<KeyValueData> CheckData = Check_list.Select(p => new KeyValueData { Key = p.Label, value = p.IsSelected }).ToList();
                    _dataManager.SetData("CheckList",JsonConvert.SerializeObject(ChecklistData));
                    await ShowSignature();
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["please_select_checklist"]);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task ShowSignature()
        {
            try
            {
                if (await _alertService.ShowConfirmationReturnType(@Localizer["confirmation"], @Localizer["you_are_collecting_payment_using"] + " " + await ConcatPaymentModes() + ". " + @Localizer["do_you_want_to_proceed?"], @Localizer["yes"], @Localizer["no"]))
                {
                    _createPaymentAppViewModel.CollectionsUID = paymentInfos.Count(p => p.IsChecked) == 1 ? _dataManager.GetData("ReceiptNumber").ToString() : _dataManager.GetData("ConsolidatedReceiptNumber").ToString();
                    await _createPaymentAppViewModel.PrepareSignatureFields();
                    ReceiptNo = paymentInfos.Count(p => p.IsChecked) == 1 ? _dataManager.GetData("ReceiptNumber").ToString() : _dataManager.GetData("ConsolidatedReceiptNumber").ToString();
                    IsSignatureView = true;
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task HandleSignature_ProceedClick()
        {
            IsSignatureView = false;
            IFileSys customerFileSys = ConvertFileSys(@Localizer["collection"], _dataManager.GetData("ReceiptNumber").ToString(), @Localizer["receivedbysignature"], "Image",
                _createPaymentAppViewModel.CustomerSignatureFileName, _appUser.Emp?.Name, _createPaymentAppViewModel.CustomerSignatureFolderPath);
            IFileSys userFileSys = ConvertFileSys(@Localizer["collection"], _dataManager.GetData("ReceiptNumber").ToString(), @Localizer["deliveredbysignature"], "Image",
                _createPaymentAppViewModel.UserSignatureFileName, _appUser.Emp?.Name, _createPaymentAppViewModel.UserSignatureFolderPath);
            if (customerFileSys is not null)
            {
                _createPaymentAppViewModel.SignatureFileSysList.Add(customerFileSys);
            }
            if (userFileSys is not null)
            {
                _createPaymentAppViewModel.SignatureFileSysList.Add(userFileSys);
            }
            await _createPaymentAppViewModel.OnSignatureProceedClick();
            await CreatePayment();
        }

        //onPayment click
        public async Task CreatePayment()
        {
            //await sample.CreatePayment();
            await UpdateCollectionLimit();
            _createPaymentAppViewModel.SelectedItems = SelectedItems;
            _createPaymentAppViewModel.paymentInfos = paymentInfos;
            string result = "";
            string resultonaccount = "";
            if (SelectedItems.Count > 0)
            {
                if (RemainingAmt == 0)
                {
                    result = await _createPaymentAppViewModel.CreateReceipt(collection);
                    if (result == "1" && (_createPaymentAppViewModel.OnAccountResult == 1 && _createPaymentAppViewModel.IsOnAccount))
                    {
                        IsOrderPlacedPopupVisible = true;
                    }
                    if (result != "1" && (_createPaymentAppViewModel.OnAccountResult == 1 && _createPaymentAppViewModel.IsOnAccount))
                    {
                        await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["receipts_creation_failed,_onaccount_created"]);
                    }
                    if (result == "1" && (_createPaymentAppViewModel.OnAccountResult != 1 && _createPaymentAppViewModel.IsOnAccount))
                    {
                        if (_createPaymentAppViewModel.OnAccountResult != 1)
                        {
                            await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["receipts_created_successfully,_onaccount_failed"]);
                        }
                    }
                    else
                    {
                        IsOrderPlacedPopupVisible = true;
                    }
                    if (!IsOrderPlacedPopupVisible)
                    {
                        await SuccessNavigation();
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["invoice_and_collected_amount_should_be_same"]);
                }
            }
            else
            {
                _createPaymentAppViewModel.IsDirectOnAccountCreate = true;
                result = await _createPaymentAppViewModel.CreateOnAccount(collection, true);
                if (result == "1")
                {
                    await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["onaccount_created_successfully"]);
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["onaccount_creation_failed"]);
                }
                await SuccessNavigation();
            }
        }

        public async Task<string> ConcatPaymentModes()
        {
            try
            {
                List<string> modes = new List<string>();
                foreach (var data in paymentInfos.Where(p => p.IsChecked))
                {
                    modes.Add(data.PaymentType);
                }
                if (modes.Count == 0)
                {
                    modes.Add("CreditNote");
                }

                return string.Join(", ", modes);
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public async Task Print()
        {
            if (true)         //(CheckPrinterAvailableOrNot())
            {

                CollectionOrderPaymentDetails = await _createPaymentAppViewModel.GetCollectionStoreDataForPrinter(_createPaymentAppViewModel.UIDForPrint);
                //CollectionOrderPaymentList = await _createPaymentAppViewModel.GetAllotmentDataForPrinter(data);

                IPrint CollectionOrderPrint = new CollectionOrderPrint();
                string printerTypeString = _storageHelper.GetStringFromPreferences("PrinterTypeOrBrand");  // "Zebra"; 
                string printerSizeString = _storageHelper.GetStringFromPreferences("PrinterPaperSize");   // "FourInch";        //
                PrinterType printerType = (PrinterType)Enum.Parse(typeof(PrinterType), printerTypeString);
                PrinterSize printerPaperSize = (PrinterSize)Enum.Parse(typeof(PrinterSize), printerSizeString);
                string collectionOrderPrintString = CollectionOrderPrint.CreatePrintString(printerType, printerPaperSize, (CollectionOrderPaymentDetails));
                Winit.Modules.Printing.BL.Interfaces.IPrinter userPrinter = Winit.Modules.Printing.BL.Factory.PrinterFactory.CreatePrinter(_storageHelper.GetStringFromPreferences("PrinterMacAddresses"), printerType, _storageHelper.GetStringFromPreferences("PrinterMacAddresses"));
                if (userPrinter.Type == PrinterType.Zebra)
                {
                    await ((ZebraPrinter)userPrinter).Print(collectionOrderPrintString);
                }

            }
            else
            {
                // _alertService.ShowErrorAlert("No Printer ", "No Printer Selected at Printer Service for Printing.");
            }
        }
        public async Task UpdateCollectionLimit()
        {
            try
            {
                decimal Limit = paymentInfos.Where(p => p.PaymentType == PaymentModes.CASH).Sum(p => p.Amount);
                bool result = await _createPaymentAppViewModel.UpdateCollectionLimit(Limit, _appUser.Emp.UID, (int)Mode.Cash);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task SuccessNavigation()
        {
            await multi.ClearDictionary();
            _createPaymentAppViewModel.UIDForPrint.Clear();
            _createPaymentAppViewModel.SignatureFileSysList.Clear();
            _navigationManager.NavigateTo("paymentsummary");
        }


        private void OnImageDeleteClick(string fileName)
        {
            IFileSys fileSys = (_createPaymentAppViewModel as CreatePaymentAppViewModel).ImageFileSysList.Find
                (e => e.FileName == fileName);
            if (fileSys is not null) (_createPaymentAppViewModel as CreatePaymentAppViewModel).ImageFileSysList.Remove(fileSys);
        }
        private async Task OnImageCapture((string fileName, string folderPath) data)
        {
            IFileSys fileSys = ConvertFileSys("Collection", _createPaymentAppViewModel.ReceiptNumber, "Item", "Image",
                data.fileName, _appUser.Emp?.Name, data.folderPath);
            (_createPaymentAppViewModel as CreatePaymentAppViewModel).ImageFileSysList.Add(fileSys);
            await Task.CompletedTask;
        }

        public override async Task OnBackClick()
        {
            if (await _alertService.ShowConfirmationReturnType(@Localizer["alert"], @Localizer["do_you_want_to_cancel_this_payment?"]))
            {
                _navigationManager.NavigateTo("Payment");
                _backbuttonhandler.ClearCurrentPage();
            }

        }
        public async Task<bool> CheckCashCollectionLimit()
        {
            try
            {
                await _createPaymentAppViewModel.GetCollectionLimitForLoggedInUser(_appUser.Emp.UID);
                if (paymentInfos.Where(p => p.PaymentType == PaymentModes.CASH).Sum(p => p.Amount) > _createPaymentAppViewModel.CollectionLimit)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert(@Localizer["alert"], "Exception");
                throw ex;
            }
        }

        public async Task<bool> CheckAllValidations()
        {
            try
            {
                if (RemainingAmt != 0)
                {
                    bool result = paymentInfos.Count(p => p.IsChecked) == 0;
                    if (result)
                    {
                        await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["please_select_paymentmode"]);
                        return false;
                    }
                    bool result1 = paymentInfos.Any(p => (p.IsChecked && p.Amount == 0));
                    if (result1)
                    {
                        await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["please_enter_amount_in_selected_paymentmode"]);
                        return false;
                    }
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["invoice_and_collected_amount_should_be_same"]);
                    return false;
                }
                else
                {
                    if (paymentInfos.Any(p => (p.IsChecked && p.Amount == 0)) && _createPaymentAppViewModel.Discrepency == "0")
                    {
                        await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["entered_amount_can_not_be_zero_in_selected_payment_mode"]);
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task UpdateRemainingAmount(decimal Amount)
        {
            try
            {
                Collected = Amount;
                RemainingAmt = Invoice - Collected;
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task UpdatePaymentList(Dictionary<string, List<IExchangeRate>> StateManage)
        {
            try
            {
                //PaymentMode
                paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentMode).Amount = await ManagePaidAmount(StateManage);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<decimal> ManagePaidAmount(Dictionary<string, List<IExchangeRate>> StateManage)
        {
            try
            {
                decimal Total = StateManage[PaymentMode].Sum(p => p.ConvertedAmount);
                await Task.CompletedTask;
                return Total;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task OrderPlacedPopup_CloseClicked()
        {
            IsOrderPlacedPopupVisible = false;
            await SuccessNavigation();
        }
        public async Task OrderPlacedPopup_EmailClicked()
        {
            await OrderPlacedPopup_CloseClicked();
        }
        public async Task OrderPlacedPopup_ShareClicked()
        {
            await OrderPlacedPopup_CloseClicked();
        }
        public async Task OrderPlacedPopup_PrintClicked()
        {
            await Print();
            await OrderPlacedPopup_CloseClicked();
        }

    }
}
