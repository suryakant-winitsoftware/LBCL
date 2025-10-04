using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Nest;
using Newtonsoft.Json;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Constants;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using static WinIt.Pages.Collection.ViewPaymentdetails.ViewPayments;


namespace WinIt.Pages.Collection.CashierSettlementDetails
{
    public partial class Cashier
    {
        bool fixed_header = true;
        bool fixed_footer = false;
        private HashSet<AccElement> selectedItems = new HashSet<AccElement>();
        private HashSet<object> selectedItems1 = new HashSet<object>();
        private AccElement[] Elements { get; set; } = new AccElement[0];
        private AccElement[] Elementssettled { get; set; } = new AccElement[0];
        private AccElement[] Elementsvoid { get; set; } = new AccElement[0];
        private List<AccElement> ElementsList { get; set; } = new List<AccElement>();
        private List<AccElement> TotalList { get; set; } = new List<AccElement>();
        public List<DataGridColumn> DataGridColumns { get; set; }
        private List<AccElement> GridDataSource { get; set; } = new List<AccElement>();
        private List<AccElement> pendingList { get; set; } = new List<AccElement>();
        private List<AccElement> settledListCopy { get; set; } = new List<AccElement>();
        private List<AccElement> settledList2 { get; set; } = new List<AccElement>();
        private List<AccElement> settledList { get; set; } = new List<AccElement>();
        private List<AccElement> reversalList { get; set; } = new List<AccElement>();
        private List<AccElement> voidList { get; set; } = new List<AccElement>();
        private List<AccElement> ElementsListCopy { get; set; } = new List<AccElement>();
        private List<AccElement> ElementsListempty { get; set; } = new List<AccElement>();
        private List<AccElement> checkBoxItems { get; set; } = new List<AccElement>();
        private List<AccElement> filteredItems { get; set; } = new List<AccElement>();


        private string PaymentMode { get; set; } = "";
        private AccDocument[] states =
        {
        new AccDocument { TargetUID = "AllInvoices" },
    };
        private string DocumentType { get; set; } = "";

        private bool _selectOnRowClick = false;
        private string _selectedItemText = "No row clicked";
        private decimal enteredAmount = 0;



        private static List<string> Multiple = new List<string>();
        private int _currentPage = 1;
        private int _pageSize = 10;
        private static int error { get; set; } = 0;
        private string ReceiptNumberNew { get; set; } = "";
        private string TargetUIDNew { get; set; } = "";
        private string AmountNew { get; set; } = "";
        private string ChequeNoNew { get; set; } = "";
        private string SessionUserCodeNew { get; set; } = "";
        private string ReasonforCancelationNew { get; set; } = "";
        private string PaymentUIDNew { get; set; } = "";
        private string PaymentModeNew { get; set; } = "";
        private string DocumentTypeNew { get; set; } = "";
        private string CustomerNameNew { get; set; } = "";
        private string searchText { get; set; } = "";
        [Parameter] public int Count { get; set; } = 0;
        public int PendingCount { get; set; } = 0;
        public int SettledCount { get; set; } = 0;
        public int VoidCount { get; set; } = 0;
        public int ReversalCount { get; set; } = 0;
        public static Winit.Modules.Base.BL.ApiService apiService { get; set; }
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }

        private readonly List<ISelectionItem> TabSelectionItems =
       [
           new SelectionItemTab{ Label=TabConstants.Pending, Code = TabConstants.Pending, UID="1"},
            new SelectionItemTab{ Label=TabConstants.Settled, Code = TabConstants.Settled, UID="2"},
            new SelectionItemTab{ Label=TabConstants.Void, Code=TabConstants.Void, UID="3"},
            new SelectionItemTab{ Label=TabConstants.Reversal, Code=TabConstants.Reversal, UID="4"},
        ];
        private List<DataGridColumn> pendingColumns { get; set; }
        private List<DataGridColumn> settleColumns { get; set; }
        private List<DataGridColumn> voidColumns { get; set; }
        private List<DataGridColumn> reversalColumns { get; set; }

        public List<IAccCollection> accCollection = new List<IAccCollection>();

        List<DataGridColumn> showPaymentDetailsColumns { get; set; } = new List<DataGridColumn>();
        private List<Winit.Modules.Store.Model.Classes.Store> CustomerCode { get; set; } = new List<Winit.Modules.Store.Model.Classes.Store>();
        private static Dictionary<string, string> storeData = new Dictionary<string, string>();
        
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        public List<PaymentModes> status { get; set; } = new List<PaymentModes>();
        public bool IsInitialized { get; set; }
        public string TabName { get; set; } = "";
        ISelectionItem SelectedTab { get; set; }
        private SelectionManager TabSelectionManager =>
        new(TabSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Cashier Settlement",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Cashier Settlement"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            try
            {
                _loadingService.ShowLoading();
                LoadResources(null, _languageService.SelectedCulture);
                _cashSettlementViewModel.filterCriterias.Clear();
                status.Add(new PaymentModes { Mode = "Collected" });
                status.Add(new PaymentModes { Mode = "Submitted" });
                status.Add(new PaymentModes { Mode = "Reversed" });
                status.Add(new PaymentModes { Mode = "Voided" });
                await _cashSettlementViewModel.GetCustomerCodeName();
                GridColumns();
                ReceiptNumberNew = GetParameterValueFromURL("ReceiptNumber");
                TargetUIDNew = GetParameterValueFromURL("TargetUID");
                AmountNew = GetParameterValueFromURL("Amount");
                ChequeNoNew = GetParameterValueFromURL("ChequeNo");
                SessionUserCodeNew = GetParameterValueFromURL("SessionUserCode");
                ReasonforCancelationNew = GetParameterValueFromURL("ReasonforCancelation");
                PaymentUIDNew = GetParameterValueFromURL("PaymentUID");
                PaymentModeNew = GetParameterValueFromURL("PaymentMode");
                DocumentTypeNew = GetParameterValueFromURL("DocumentType");
                CustomerNameNew = GetParameterValueFromURL("CustomerName");
                await SetHeaderName();
                await OnTabSelect(TabSelectionItems.FirstOrDefault()!);
                IsInitialized = true;
                FilterInitialized();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                StateHasChanged();
                _loadingService.HideLoading();
            }

        }
        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            try
            {
                ShowLoader();
                await CommonMethod();
                TabSelectionManager.Select(selectionItem);
                TabName = selectionItem.Label;
                await AssignDataSource(TabName);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                StateHasChanged();
                HideLoader();
            }
        }
        public async Task AssignDataSource(string TabName)
        {
            try
            {
                switch(TabName)
                {
                    case TabConstants.Pending:
                        GridDataSource = pendingList;
                        Count = PendingCount;
                        break;
                    case TabConstants.Void:
                        GridDataSource = voidList;
                        Count = VoidCount;
                        break;
                    case TabConstants.Settled:
                        GridDataSource = settledList;
                        Count = SettledCount;
                        break;
                    case TabConstants.Reversal:
                        GridDataSource = reversalList;
                        Count = ReversalCount;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async Task PageIndexChanged(int pageNumber)
        {
            await InvokeAsync(async () =>
            {
                ShowLoader();
                await _cashSettlementViewModel.PageIndexChanged(pageNumber);
                HideLoader();
            });
        }
        public void FilterInitialized()
        {
            List<ISelectionItem> Status = Winit.Shared.CommonUtilities.Common.CommonFunctions.ConvertToSelectionItems(status, new List<string> { "Mode", "Mode", "Mode" });
            List<ISelectionItem> CustomerNames = Winit.Shared.CommonUtilities.Common.CommonFunctions.ConvertToSelectionItems(_cashSettlementViewModel.CustomerCode.ToList(), new List<string> { "UID", "Code", "Name" });
            ColumnsForFilter = new List<FilterModel>
        {
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["receiptnumber"],ColumnName = "ReceiptNumber"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues= CustomerNames, ColumnName = "StoreName", Label = @Localizer["customer_name"], SelectionMode= Winit.Shared.Models.Enums.SelectionMode.Multiple},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, ColumnName = "CollectedDate", Label = @Localizer["date"]}      };
        }

        public async void ShowFilter()
        {
            filterRef.ToggleFilter();
        }

        private async void OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            _cashSettlementViewModel.filterCriterias.Clear();
            await _cashSettlementViewModel.OnFilterApply(keyValuePairs, "CashSettlement");
            await CommonMethod();
        }
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_collection"], IsClickable = true, URL = "showpaymentdetails" });
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 3, Text = @Localizer["cash_settlement"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["cash_settlement"];
            await CallbackService.InvokeAsync(_IDataService);
        }
        private async Task CommonMethod()
        {
            try
            {
                await _cashSettlementViewModel.GetCashierDetails();
                PagedResponse<AccElement> pagedResponse = _cashSettlementViewModel.pageResponse;
                if (pagedResponse != null)
                {
                    Elements = pagedResponse.PagedData.ToArray();
                    PendingCount = pagedResponse.TotalCount;
                }
                pendingList = Elements.ToList();

                PagedResponse<AccElement> pagedResponse1 = _cashSettlementViewModel.pagedResponse1;
                if (pagedResponse1 != null)
                {
                    Elementsvoid = pagedResponse1.PagedData.ToArray();
                    VoidCount = pagedResponse1.TotalCount;
                }
                voidList = Elementsvoid.ToList();

                PagedResponse<AccElement> pagedResponse2 = _cashSettlementViewModel.pagedResponse2;
                if (pagedResponse2 != null)
                {
                    Elementssettled = pagedResponse2.PagedData.ToArray();
                }
                settledList = Elementssettled.ToList();
                settledListCopy = settledList;
                settledList = settledListCopy.Where(p => p.Status == "Settled").ToList();
                SettledCount = settledList.Count;
                reversalList = settledListCopy.Where(p => p.Status == "Reversed" && p.UID.Contains("R -")).ToList();
                ReversalCount = reversalList.Count;
                ElementsListCopy = ElementsList;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public void ConvertToCollection()
        {
            try
            {

                //foreach(var data in accElement)
                //{
                //    AccCollection collection = new AccCollection();
                //    collection.Amount = data.Amount;
                //    collection.Status = data.Status;
                //    accCollection.Add(collection);
                //}
            }
            catch(Exception ex)
            {

            }
        }


        private void Search()
        {
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    try
                    {
                        filteredItems = ElementsListCopy.Where(item =>
                            item.ReceiptNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                        ElementsList = filteredItems;
                    }
                    catch (Exception ex)
                    {
                        // elemEmpty = new Elemental[]
                        // {
                        //     new Elemental { UID = "no records found" }
                        // };
                        ElementsList = ElementsListempty;
                    }
                }
                else
                {
                    ElementsList = ElementsListCopy;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task ExportToExcel()
        {
            try
            {
                string fileName = "CollectionDetails" + Guid.NewGuid().ToString();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Collection Details");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "ReceiptNumber";
                    worksheet.Cell(1, 2).Value = "Category";
                    worksheet.Cell(1, 3).Value = "PaidAmount";
                    worksheet.Cell(1, 4).Value = "IsSettled";
                    worksheet.Cell(1, 5).Value = "IsVoid";//hi bro
                    worksheet.Cell(1, 6).Value = "IsReversal";

                    // Populate the Excel worksheet with your data from elem
                    for (int i = 0; i < ElementsList.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = ElementsList[i].ReceiptNumber;
                        worksheet.Cell(row, 2).Value = ElementsList[i].Category;
                        worksheet.Cell(row, 3).Value = ElementsList[i].PaidAmount;
                        worksheet.Cell(row, 4).Value = ElementsList[i].IsSettled;
                        worksheet.Cell(row, 5).Value = ElementsList[i].IsVoid;
                        worksheet.Cell(row, 6).Value = ElementsList[i].IsReversal;
                    }

                    var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    var bytes = stream.ToArray();
                    string base64 = Convert.ToBase64String(bytes);

                    var anchor = $@"<a href='data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,{base64}' download='{fileName}.xlsx'>Download Excel</a>";

                    // Use JavaScript interop to trigger a click event on the anchor element
                    await JSRuntime.InvokeVoidAsync("eval", $"var a = document.createElement('a'); a.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,{base64}'; a.download = '{fileName}.xlsx'; a.click();");

                    // Optionally, you can show a confirmation message to the user
                    //Snackbar.Add("Exported to Excel", Severity.Success);
                }
            }
            catch (Exception ex)
            {

            }
        }

        string BidirectionalLookup(string input)
        {
            // Check if the input is a key in the dictionary
            if (_cashSettlementViewModel.storeData.TryGetValue(input, out string value))
            {
                return value; // Return the value (customer name or GUID) if found
            }

            // Check if the input is a value in the dictionary
            foreach (var pair in _cashSettlementViewModel.storeData)
            {
                if (pair.Value == input)
                {
                    return pair.Key; // Return the key (customer name) if found
                }
            }

            return "";
        }
        public void GridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Receipt Number", GetValue = s => ((AccElement)s).ReceiptNumber ?? "N/A"},
                new DataGridColumn { Header = "Customer Name ", GetValue = s => ((AccElement)s).StoreUID ?? "N/A"},
                new DataGridColumn {Header = "Date", GetValue = s =>CommonFunctions.GetDateTimeInFormat(((AccElement)s).CollectedDate) ?? "N/A"},
                new DataGridColumn {Header = "Payment Mode", GetValue = s =>((AccElement) s).Category ?? "N/A"    },
                new DataGridColumn {Header = "Status", GetValue = s =>((AccElement) s).Status ?? "N/A"},
                new DataGridColumn {Header = "Amount", GetValue = s =>((AccElement) s).Amount + "(" +((AccElement) s).CurrencyUID + ")" ?? "N/A"},
                new DataGridColumn
                {
                    Header = @Localizer["actions"],
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            Text = @Localizer["view"],
                            Action = async item => await HandleAction1_Product((AccElement)item)
                        },
                    }
                }
             };
            //pendingColumns = new List<DataGridColumn>
            //{
            //    new DataGridColumn { Header = @Localizer["receipt_number"], GetValue = s => ((AccElement)s).ReceiptNumber },
            //    new DataGridColumn { Header = @Localizer["customer_name"], GetValue = s => ((AccElement)s).StoreUID },
            //    new DataGridColumn { Header = @Localizer["date"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((AccElement)s).CollectedDate) },
            //    new DataGridColumn { Header = @Localizer["payment_mode"], GetValue = s => ((AccElement)s).Category  },
            //    new DataGridColumn { Header = @Localizer["status"], GetValue = s => ((AccElement)s).Status  },
            //    new DataGridColumn { Header = @Localizer["amount"], GetValue = s => ((AccElement)s).Amount + "(" + ((AccElement)s).CurrencyUID + ")" },
            //    new DataGridColumn
            //    {
            //        Header = @Localizer["actions"],
            //        IsButtonColumn = true,
            //        ButtonActions = new List<ButtonAction>
            //        {
            //            new ButtonAction
            //            {
            //                Text = @Localizer["view"],
            //                Action = async item => await HandleAction1_Product((AccElement)item)
            //            },
            //        }
            //    }
            //};
        }
        private async Task HandleAction1_Product(AccElement item)
        {
            try
            {
                await Clicked(item);
            }
            catch (Exception ex)
            {

            }
        }

        private async void Product_OnPageChange(int pageNumber)
        {
            try
            {
                _currentPage = pageNumber;
                await CommonMethod();
            }
            catch (Exception ex)
            {

            }
        }
        protected async Task Clicked(AccElement context)
        {
            try
            {

                string cashnum = context.CashNumber;
                Winit.Shared.Models.Common.ApiResponse<string> response1 = await _cashSettlementViewModel.Clicked(context.Status);
                if (response1.StatusCode == 200)
                {
                    //if it is settled it would be present and statuscode is 200 then void is not possible
                    NavigationManager.NavigateTo("revers?ReceiptNumber=" + context.UID + "&Amount=" + context.DefaultCurrencyAmount + "&ChequeNo=" + context.CashNumber + "&Customer=" + context.StoreUID + "&Date=" + context.CollectedDate + "&Salesman=" + context.Salesman + "&SessionUserCode=" + "1001" + "&ReasonforCancelation=" + context.Comments + "&Route=" + context.Route + "&PaymentMode=" + PaymentMode + "&DocumentType=" + DocumentType + "&CustomerName=" + context.ReceiptNumber + "&Button=" + "Reversal" + "&IsReversed=" + context.Status + "&IsSettled=" + context.IsSettled + "&IsVoid=" + context.IsVoid + "&Comments=" + context.Comments);
                }
                else
                {
                    //because if it is settled then statuscode is 200 if not then void is possible
                    NavigationManager.NavigateTo("revers?ReceiptNumber=" + context.UID + "&Amount=" + context.DefaultCurrencyAmount + "&ChequeNo=" + context.CashNumber + "&Customer=" + context.StoreUID + "&Date=" + context.CollectedDate + "&Salesman=" + context.Salesman + "&SessionUserCode=" + "1001" + "&ReasonforCancelation=" + context.Comments + "&Route=" + context.Route + "&PaymentMode=" + PaymentMode + "&DocumentType=" + DocumentType + "&CustomerName=" + context.ReceiptNumber + "&Button=" + "Void" + "&IsReversed=" + context.Status + "&IsSettled=" + context.IsSettled + "&IsVoid=" + context.IsVoid + "&Comments=" + context.Comments);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }


        public async void OnCheckBoxSelection(HashSet<object> hashSet)
        {
            try
            {
                selectedItems1 = hashSet;
                Multiple = new List<string>();

                foreach (var items in hashSet)
                {

                    if (items is AccElement elem)
                    {
                        _selectedItemText = elem.UID;
                        //Multiple.Add(_selectedItemText);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task GetInformation()
        {
            try
            {
                if (selectedItems1.Count == 0)
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["select_records_to_settle"]);
                }
                else
                {
                    foreach (var list in selectedItems1)
                    {
                        if (list is AccElement elem)
                        {
                            //if selected records are already settled then it will show error otherwise it will settle, if one also present shows error
                            if (elem.Status == "Collected")
                            {
                                Multiple.Add(elem.UID);
                            }
                            else
                            {
                                error++;
                            }
                        }
                    }
                    if (Multiple.Count > 0)
                    {
                        if (error > 0)
                        {
                            error = 0;
                            //selected records already settled then it will enter here
                            await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_unselect_the_settled_records"]);

                        }
                        else
                        {
                            Winit.Shared.Models.Common.ApiResponse<string> response = await _cashSettlementViewModel.SettleRecords(Multiple);
                            if (response.StatusCode == 201)
                            {
                                if (selectedItems1.OfType<AccElement>().Any(p => !p.ReceiptNumber.Contains("OA -")))
                                {
                                    decimal Amount = selectedItems1.OfType<AccElement>().Where(p => !p.ReceiptNumber.Contains("OA -")).Sum(p => p.Amount);
                                    bool result = await _createPaymentViewModel.UpdateCollectionLimit(Amount, _iAppUser.Emp.UID, 1);
                                }
                                NavigationManager.NavigateTo("cash", true);
                                await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["successfully_settled"]);
                                //NavigationManager.NavigateTo("revers?ReceiptNumber=" + ReceiptNumberNew + "&TargetUID=" + "Cash" + "&Amount=" + AmountNew + "&ChequeNo=" + ChequeNoNew + "&SessionUserCode=" + "1001" + "&ReasonforCancelation=" + "" + "&PaymentUID=" + PaymentUIDNew + "&PaymentMode=" + PaymentModeNew + "&DocumentType=" + DocumentTypeNew + "&CustomerName=" + CustomerNameNew);
                            }
                            if (response.StatusCode == 400)
                            {
                                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["badrequest"]);

                            }
                            if (response.StatusCode == 500)
                            {
                                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["data_missing"]);
                            }
                        }
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["selected_records_are_already_settled/reversed/voided"]);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

    }
}
