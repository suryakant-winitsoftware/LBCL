using Microsoft.AspNetCore.Components;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;


namespace WinIt.Pages.Collection.ViewPaymentdetails
{
    public partial class ViewPayments : BaseComponentBase
    {
        private List<DataGridColumn> showPaymentDetailsColumns { get; set; } = new List<DataGridColumn>();
        private static Dictionary<string, string> storeData { get; set; } = new Dictionary<string, string>();
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsInitialised { get; set; }
        private Winit.UIComponents.Web.Filter.Filter filterRef { get; set; }
        public List<FilterModel> ColumnsForFilter { get; set; }
        public List<PaymentModes> paymentModes { get; set; } = new List<PaymentModes>();
        public List<PaymentModes> status { get; set; } = new List<PaymentModes>();
        public EventCallback<int> OnStringChanged { get; set; }
        public class PaymentModes
        {
            public string Mode { get; set; }
        }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View Payments",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="View Payments"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            LoadResources(null, _languageService.SelectedCulture);
            await SetHeaderName();
            _viewPaymentsViewModel.FilterCriterias.Clear();
            paymentModes.Add(new PaymentModes { Mode = "Cash" });
            paymentModes.Add(new PaymentModes { Mode = "Cheque" });
            paymentModes.Add(new PaymentModes { Mode = "POS" });
            paymentModes.Add(new PaymentModes { Mode = "Online" });
            status.Add(new PaymentModes { Mode = "Collected" });
            status.Add(new PaymentModes { Mode = "Submitted" });
            status.Add(new PaymentModes { Mode = "Reversed" });
            status.Add(new PaymentModes { Mode = "Voided" });
            FilterInitialized();
            await _viewPaymentsViewModel.PopulateViewModel();
            await _viewPaymentsViewModel.GetCustomerCodeName();
            storeData = _viewPaymentsViewModel.storeData;
            GridColumns();
            IsInitialised = true;
            _loadingService.HideLoading();
            StateHasChanged();
        }

        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["view_payments"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["view_payments"];
            await CallbackService.InvokeAsync(_IDataService);
        }
        public async Task ShowFilter()
        {
            filterRef.ToggleFilter();
        }
        private async void OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            _viewPaymentsViewModel.FilterCriterias.Clear();
            await _viewPaymentsViewModel.OnFilterApply(keyValuePairs, "ViewPayments");
            StateHasChanged();

        }
        public void FilterInitialized()
        {
            List<ISelectionItem> PaymentModes = Winit.Shared.CommonUtilities.Common.CommonFunctions.ConvertToSelectionItems(paymentModes, new List<string> { "Mode", "Mode", "Mode" });
            List<ISelectionItem> Status = Winit.Shared.CommonUtilities.Common.CommonFunctions.ConvertToSelectionItems(status, new List<string> { "Mode", "Mode", "Mode" });
            ColumnsForFilter = new List<FilterModel>
            {
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["receiptnumber"],ColumnName = "ReceiptNumber"},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, ColumnName = "FromDate", Label =@Localizer["from_date"] },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, ColumnName = "ToDate", Label = @Localizer["to_date"]},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues=PaymentModes, ColumnName = "Category", Label = @Localizer["payment_type"], SelectionMode= Winit.Shared.Models.Enums.SelectionMode.Multiple},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues=Status, ColumnName = "Status", Label = @Localizer["status"], SelectionMode= Winit.Shared.Models.Enums.SelectionMode.Multiple}
            };
        }
        public void GridColumns()
        {
            showPaymentDetailsColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["receipt_number"], GetValue = s => ((AccCollection)s).ReceiptNumber,  IsSortable = true, SortField = "ReceiptNumber"  },
                new DataGridColumn { Header = @Localizer["collected_date"], GetValue = s => ((AccCollection)s).CollectedDate == null || ((AccCollection)s).CollectedDate == null ? "NA" : ((AccCollection)s).CollectedDate,  IsSortable = true, SortField = "CollectedDate"  },
                new DataGridColumn { Header = @Localizer["customer_name"], GetValue = s => BidirectionalLookup(((AccCollection)s).StoreUID),  IsSortable = true, SortField = "StoreUID" },
                new DataGridColumn { Header = @Localizer["payment_type"], GetValue = s => ((AccCollection)s).Category },
                new DataGridColumn { Header = @Localizer["amount"], GetValue = s => CommonFunctions.RoundForSystem(((AccCollection)s).Amount)+ "(" + ((AccCollection)s).CurrencyUID + ")" },
                new DataGridColumn { Header = @Localizer["status"], GetValue = s => ((AccCollection)s).Status == "Approved" ? "Collected" : ((AccCollection)s).Status, IsSortable = true, SortField = "Status" },
                new DataGridColumn { Header = @Localizer["collected_by"], GetValue = s => ((AccCollection)s).ModifiedBy},
                new DataGridColumn
                {
                    Header = @Localizer["action"],
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            Text = "✎",
                            Action = async item => await HandleAction1_Product((AccCollection)item)
                        },
                    }
                }
            };
        }
        private async void Product_OnSort(SortCriteria sortCriteria)
        {
            await _viewPaymentsViewModel.OnSortApply(sortCriteria);
            StateHasChanged();
        }
        private async Task HandleAction1_Product(AccCollection item)
        {
            try
            {
                _navigate.NavigateTo("viewpaymentdetails?Receipt=" + item.ReceiptNumber + "&CollectedDate=" + item.CollectedDate + "&CustomerName=" + item.StoreUID + "&Category=" + item.Category + "&Status=" + item.Status + "&Amount=" + item.DefaultCurrencyAmount + "&UID=" + item.UID);
            }
            catch (Exception ex)
            {

            }
        }
        static string BidirectionalLookup(string input)
        {
            // Check if the input is a key in the dictionary
            if (storeData.TryGetValue(input, out string value))
            {
                return value; // Return the value (customer name or GUID) if found
            }

            // Check if the input is a value in the dictionary
            foreach (var pair in storeData)
            {
                if (pair.Value == input)
                {
                    return pair.Key; // Return the key (customer name) if found
                }
            }
            return "";
        }
        private async void Product_OnPageChange(int pageNumber)
        {
            try
            {
                _viewPaymentsViewModel.PageNumber = pageNumber;
                await _viewPaymentsViewModel.PopulateViewModel();
                StateHasChanged();
            }
            catch (Exception ex)
            {


            }
        }
        public decimal CalculateTotalAmount()
        {
            try
            {
                decimal CollectedAmount = _viewPaymentsViewModel.Payments.Where(p => p.Status.Contains("Collected")).Sum(p => p.Amount);
                decimal SubmittedAmount = _viewPaymentsViewModel.Payments.Where(p => p.Status.Contains("Submitted")).Sum(p => p.Amount);
                return CollectedAmount + SubmittedAmount;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
