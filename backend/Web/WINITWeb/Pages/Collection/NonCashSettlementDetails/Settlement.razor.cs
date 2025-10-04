using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components;
using Nest;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Practice;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using Winit.Modules.Base.BL.Helper.Classes;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Constants;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Collection.CashierSettlementDetails;
using static WinIt.Pages.Collection.ViewPaymentdetails.ViewPayments;

namespace WinIt.Pages.Collection.NonCashSettlementDetails
{
    public partial class Settlement
    {
        bool fixed_header = true;
        bool fixed_footer = false;
        private bool bordered = false;
        private AccCollectionPaymentMode[] elem { get; set; } = new AccCollectionPaymentMode[0];
        private AccCollectionPaymentMode[] elemen { get; set; } = new AccCollectionPaymentMode[0];
        private AccCollectionPaymentMode[] elemt { get; set; } = new AccCollectionPaymentMode[0];
        private AccCollectionPaymentMode[] elemtRej { get; set; } = new AccCollectionPaymentMode[0];
        private AccCollectionPaymentMode[] elemtBounc { get; set; } = new AccCollectionPaymentMode[0];
        public List<AccCollectionPaymentMode> TotalList { get; set; } = new List<AccCollectionPaymentMode>();
        public List<AccCollectionPaymentMode> GridDataSource { get; set; } = new List<AccCollectionPaymentMode>();
        public List<AccCollectionPaymentMode> _elemPending { get; set; } = new List<AccCollectionPaymentMode>();
        public List<AccCollectionPaymentMode> _elemSettled { get; set; } = new List<AccCollectionPaymentMode>();
        public List<AccCollectionPaymentMode> _elemApproved { get; set; } = new List<AccCollectionPaymentMode>();
        public List<AccCollectionPaymentMode> _elemApprovedCopy { get; set; } = new List<AccCollectionPaymentMode>();
        public List<AccCollectionPaymentMode> _elemReversed { get; set; } = new List<AccCollectionPaymentMode>();
        public List<AccCollectionPaymentMode> _elemRejected { get; set; } = new List<AccCollectionPaymentMode>();
        public List<AccCollectionPaymentMode> _elemBounced { get; set; } = new List<AccCollectionPaymentMode>();
        private string searchString = "";
        private bool dense = false;
        private bool hover = true;
        private bool striped = false;
        private bool bordered1 = false;
        private string searchString1 = "";
        private decimal amount = 0;
        private string Message = "";
        private string button = "";
        private string Pending { get; set; } = "Pending";
        private string Settled { get; set; } = "Settled";
        private string Approved { get; set; } = "Approved";
        private string SessionUserCode { get; set; } = "";
        [Parameter] public int Count { get; set; } = 0;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private Bank selectedItem1 = null;
        private HashSet<Bank> selectedItems = new HashSet<Bank>();

        private static NavigationManager _navigate { get; set; }
        private readonly List<ISelectionItem> TabSelectionItems =
       [
           new SelectionItemTab{ Label=TabConstants.Pending, Code = TabConstants.Pending, UID="1", IsSelected=true},
            new SelectionItemTab{ Label=TabConstants.CashierApproved, Code = TabConstants.CashierApproved, UID="2"},
            new SelectionItemTab{ Label=TabConstants.BankApproved, Code=TabConstants.BankApproved, UID="3"},
            new SelectionItemTab{ Label=TabConstants.Rejected, Code=TabConstants.Rejected, UID="4"},
            new SelectionItemTab{ Label=TabConstants.Bounced, Code=TabConstants.Bounced, UID="5"},
        ];
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        private List<DataGridColumn> pendingColumns { get; set; } = new List<DataGridColumn>();
        private List<DataGridColumn> settleColumns { get; set; } = new List<DataGridColumn>();
        private List<DataGridColumn> approvedColumns { get; set; } = new List<DataGridColumn>();
        private List<DataGridColumn> rejectedColumns { get; set; } = new List<DataGridColumn>();
        private List<DataGridColumn> bouncedColumns { get; set; } = new List<DataGridColumn>();
        private List<DataGridColumn> reversedColumns { get; set; } = new List<DataGridColumn>();
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        public List<PaymentModes> paymentModes { get; set; } = new List<PaymentModes>();
        public List<PaymentModes> Banks { get; set; } = new List<PaymentModes>();
        public List<string> Modes { get; set; } = new List<string>() { "Cash", "Cheque", "POS", "Online" };
        public List<string> _bank { get; set; } = new List<string>() { "SBI Bank", "ICICI Bank", "Kotak Bank", "HDFC Bank", "Winit Bank" };
        public List<IAccCollection> accCollection = new List<IAccCollection>();
        public bool IsInitialised { get; set; } = false;
        public string TabName { get; set; } = "";
        private SelectionManager TabSelectionManager =>
        new(TabSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Non - Cash Settlement",
            BreadcrumList = new List<IBreadCrum>()
    {
        new BreadCrumModel(){SlNo=1,Text="Non - Cash Settlement"},
    }
        };
        protected override async Task OnInitializedAsync()
        {
            try
            {
                _loadingService.ShowLoading();
                _nonCashSettlementViewModel.filterCriterias.Clear();
                await _nonCashSettlementViewModel.GetCustomerCodeName();
                _navigate = _NavigationManager;
                paymentModes.Add(new PaymentModes { Mode = "Cash" });
                paymentModes.Add(new PaymentModes { Mode = "Cheque" });
                paymentModes.Add(new PaymentModes { Mode = "POS" });
                paymentModes.Add(new PaymentModes { Mode = "Online" });
                LoadResources(null, _languageService.SelectedCulture);
                FilterInitialized();
                columns();
                SessionUserCode = GetParameterValueFromURL("SessionUserCode");
                await OnTabSelect(TabSelectionItems.FirstOrDefault()!);
                await SetHeaderName();
                IsInitialised = true;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _loadingService.HideLoading();
            }

        }
       
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_collection"], IsClickable = true, URL = "showpaymentdetails" });
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["non-cash_settlement"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["non-cash_settlement"];
            await CallbackService.InvokeAsync(_IDataService);
        }
        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            try
            {
                ShowLoader();
                await CommonMethod();
                TabSelectionItems.ForEach(p => p.IsSelected = false);
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
                switch (TabName)
                {
                    case TabConstants.Pending:
                        GridDataSource = _elemPending;
                        Count = _nonCashSettlementViewModel.pendingCount;
                        break;
                    case TabConstants.CashierApproved:
                        GridDataSource = _elemSettled;
                        Count = _nonCashSettlementViewModel.settlementCount;
                        break;
                    case TabConstants.BankApproved:
                        GridDataSource = _elemApproved;
                        Count = _nonCashSettlementViewModel.approvedCount;
                        break;
                    case TabConstants.Rejected:
                        GridDataSource = _elemRejected;
                        Count = _nonCashSettlementViewModel.rejectedCount;
                        break;
                    case TabConstants.Bounced:
                        GridDataSource = _elemBounced;
                        Count = _nonCashSettlementViewModel.bouncedCount;
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
                await _nonCashSettlementViewModel.PageIndexChanged(pageNumber);
                HideLoader();
            });
        }
        public async void ShowFilter()
        {
            filterRef.ToggleFilter();
        }
        public void FilterInitialized()
        {
            List<ISelectionItem> Status = Winit.Shared.CommonUtilities.Common.CommonFunctions.ConvertToSelectionItems(_nonCashSettlementViewModel.BankNames.ToList(), new List<string> { "UID", "BankCode", "BankName" });
            ColumnsForFilter = new List<FilterModel>
        {
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["receiptnumber"],ColumnName = "ReceiptNumber"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues=Status, ColumnName = "BankUID", Label = @Localizer["bank"], SelectionMode= Winit.Shared.Models.Enums.SelectionMode.Multiple},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label =@Localizer["ref_no"] ,ColumnName = "ChequeNo"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, ColumnName = "FromDate", Label = @Localizer["from_date"]},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, ColumnName = "ToDate", Label = @Localizer["to_date"]}
        };
        }
        private async void OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            _nonCashSettlementViewModel.filterCriterias.Clear();
            await _nonCashSettlementViewModel.OnFilterApply(keyValuePairs,"NonCashSettlement");
            await CommonMethod();
        }
        private async Task CommonMethod()
        {
            try
            {
                await _nonCashSettlementViewModel.ShowAllTabssRecords(Count);
                Count = _nonCashSettlementViewModel.settlementCount;
                TotalList.AddRange(_elemPending = _nonCashSettlementViewModel._elemPending);
                TotalList.AddRange(_elemSettled = _nonCashSettlementViewModel._elemSettled);
                TotalList.AddRange(_elemRejected = _nonCashSettlementViewModel._elemRejected);
                TotalList.AddRange(_elemBounced = _nonCashSettlementViewModel._elemBounced);
                TotalList.AddRange(_elemApproved = _nonCashSettlementViewModel._elemApproved);
                StateHasChanged();
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
        private async Task HandleAction1_Product(AccCollectionPaymentMode item, string TabName)
        {
            try
            {
                await Clicked(item, TabName);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }

        public void columns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["receiptnumber"], GetValue = s => ((AccCollectionPaymentMode)s).ReceiptNumber },
                new DataGridColumn { Header = @Localizer["customer_name"], GetValue = s => BidirectionalLookup(((AccCollectionPaymentMode)s).StoreUID) },
                new DataGridColumn { Header =  @Localizer["bank"], GetValue = s => ((AccCollectionPaymentMode)s).BankUID },
                new DataGridColumn { Header =  @Localizer["branch"], GetValue = s => ((AccCollectionPaymentMode)s).Branch  },
                new DataGridColumn { Header =  @Localizer["payment_type"], GetValue = s => ((AccCollectionPaymentMode)s).Category  },
                new DataGridColumn { Header =  @Localizer["ref_no"], GetValue = s => ((AccCollectionPaymentMode)s).ChequeNo  },
                new DataGridColumn { Header =  @Localizer["date"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((AccCollectionPaymentMode)s).ChequeDate)},
                new DataGridColumn { Header =  @Localizer["amount"], GetValue = s => ((AccCollectionPaymentMode)s).DefaultCurrencyAmount  },
                new DataGridColumn
                {
                    Header =  @Localizer["actions"],
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            Text =  @Localizer["view"],
                            Action = async item => await HandleAction1_Product((AccCollectionPaymentMode)item, TabName)
                        },
                    }
                }
            };

            
        }

        
        string BidirectionalLookup(string input)
        {
            // Check if the input is a key in the dictionary
            if (_nonCashSettlementViewModel.storeData.TryGetValue(input, out string value))
            {
                return value; // Return the value (customer name or GUID) if found
            }

            // Check if the input is a value in the dictionary
            foreach (var pair in _nonCashSettlementViewModel.storeData)
            {
                if (pair.Value == input)
                {
                    return pair.Key; // Return the key (customer name) if found
                }
            }

            return "";
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }


        private bool FilterFunc1(Bank element) => FilterFunc(element, searchString1);

        private bool FilterFunc(Bank element, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            if (element.BankName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.BranchName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if ($"{element.ChequeNo} {element.PaymentType} {element.Comments}".Contains(searchString))
                return true;
            return false;
        }

        public async Task Clicked(AccCollectionPaymentMode context, string TabName)
        {
            try
            {
                amount = context.DefaultCurrencyAmount;
                switch (TabName)
                {
                    case TabConstants.Pending:
                        _navigate.NavigateTo("pending?UID=" + context.AccCollectionUID + "&TargetUID=" + context.ReceiptNumber + "&ReceiptNumber=" + context.ReceiptNumber + "&SessionUserCode=" + SessionUserCode + "&Amount=" + amount + "&ChequeNo=" + context.ChequeNo);
                        break;
                    case TabConstants.CashierApproved:
                        _navigate.NavigateTo("settled?UID=" + context.AccCollectionUID + "&TargetUID=" + context.ReceiptNumber + "&ReceiptNumber=" + context.ReceiptNumber + "&SessionUserCode=" + SessionUserCode + "&Amount=" + amount + "&ChequeNo=" + context.ChequeNo);
                        break;
                    case TabConstants.BankApproved:
                        _navigate.NavigateTo("approved?UID=" + context.AccCollectionUID + "&TargetUID=" + context.ReceiptNumber + "&ReceiptNumber=" + context.ReceiptNumber + "&SessionUserCode=" + SessionUserCode + "&Amount=" + amount + "&Boolen=" + true + "&ChequeNo=" + context.ChequeNo);
                        break;
                    case TabConstants.Rejected:
                        _navigate.NavigateTo("Rejected?UID=" + context.AccCollectionUID + "&TargetUID=" + context.ReceiptNumber + "&ReceiptNumber=" + context.ReceiptNumber + "&SessionUserCode=" + SessionUserCode + "&Amount=" + amount + "&Boolen=" + true + "&ChequeNo=" + context.ChequeNo);
                        break;
                    case TabConstants.Bounced:
                        _navigate.NavigateTo("Bounced?UID=" + context.AccCollectionUID + "&TargetUID=" + context.ReceiptNumber + "&ReceiptNumber=" + context.ReceiptNumber + "&SessionUserCode=" + SessionUserCode + "&Amount=" + amount + "&Boolen=" + true + "&ChequeNo=" + context.ChequeNo);
                        break;
                    default:
                        Console.WriteLine("Unknown function.");
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        //public async Task Product_OnSort(SortCriteria sortCriteria)
        //{
        //    ISortHelper sortHelper = new SortHelper();
        //    elemList = await sortHelper.Sort(elemList, sortCriteria);
        //}

        public class Bank
        {
            public int Id { get; set; }
            public string UID { get; set; }
            public string BankName { get; set; }
            public string PaymentType { get; set; }
            public string TargetUID { get; set; }
            public int Status { get; set; }
            public string BranchName { get; set; }
            public string ChequeNo { get; set; }
            public DateTime ChequeDate { get; set; }
            public decimal ChequeAmount { get; set; }
            public string Comments { get; set; }
            public DateTime CreatedTime { get; set; }
            public DateTime ModifiedTime { get; set; }
            public DateTime ServerAddTime { get; set; }
            public DateTime ServerModifiedTime { get; set; }
            public string ReceiptUID { get; set; }
        }
    }
}

