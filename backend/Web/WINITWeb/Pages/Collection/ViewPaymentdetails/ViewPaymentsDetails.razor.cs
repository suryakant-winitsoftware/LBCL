using Microsoft.AspNetCore.Components;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Collection.CreatePayment;

namespace WinIt.Pages.Collection.ViewPaymentdetails
{
    public partial class ViewPaymentsDetails
    {

        private List<AccCollectionAllotment> _viewDetailsList { get; set; } = new List<AccCollectionAllotment>();
        private List<DataGridColumn> ViewPaymentDetailsColumns;
        private string ReceiptNumber { get; set; } = "";
        private string UID { get; set; } = "";
        private string CollectedDate { get; set; } = "";
        private string CustomerName { get; set; } = "";
        private string PaymentType { get; set; } = "";
        private string Status { get; set; } = "";
        private string Amount { get; set; } = "";
        private string DiscountAmount { get; set; } = "";
        private static readonly Dictionary<string, string> Users = new Dictionary<string, string>
    {
        { "G & M Paterson" ,    "9E6E8694-A193-41F5-BBDF-F1AA7A61BC23" },
          {"Remuera - G & M Paterson", "976BBC27-3810-4D3E-9E42-A7CA7A99CFDC" },
          {"Taranaki Milk Limited" , "26306609-966C-443A-BB7D-25D976653EDA"},
          {"Peter Milk Limited" , "0EDD3D28-0C02-48CF-B98D-2529DA691F68"},
          {"Milk Central Ltd" ,"3297D102-70C2-4CCE-A557-0C73B903C5F3"},
          {"MM Distributors  Ltd" , "AD0DE4AC-332E-47E0-9117-5A11DA46AC87"},
            { "MAK Distribution Ltd" , "20F1BE6C-236B-47E2-8C25-F33EA6B2C7FC"}
    };

        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        private List<Winit.Modules.Store.Model.Classes.Store> CustomerCode { get; set; } = new List<Winit.Modules.Store.Model.Classes.Store>();
        private static Dictionary<string, string> storeData = new Dictionary<string, string>();
        public bool IsInitialised { get; set; }

        MultiCurrencyPopUp _multi { get; set; }

        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View Payment Details",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="View Payments",IsClickable=true, URL="viewpayments"},
                new BreadCrumModel(){SlNo=2,Text="View Payment Details" },
            }
        };
       
        protected override async void OnInitialized()
        {
            _loadingService.ShowLoading();
            LoadResources(null, _languageService.SelectedCulture);
            await GetCustomerCodeName();
            UID = GetParameterValueFromURL("UID");
            CollectedDate = GetParameterValueFromURL("CollectedDate");
            CustomerName = BidirectionalLookup(GetParameterValueFromURL("CustomerName"));
            PaymentType = GetParameterValueFromURL("Category");
            Status = GetParameterValueFromURL("Status");
            Amount = GetParameterValueFromURL("Amount");
            await SetHeaderName();
            columns();
            await _viewPaymentsViewModel.ViewReceiptDetails(UID);
            _viewDetailsList = _viewPaymentsViewModel._viewDetailsList;
            DiscountAmount = _viewDetailsList.FirstOrDefault().EarlyPaymentDiscountAmount.ToString();
            ReceiptNumber = GetParameterValueFromURL("Receipt");
            IsInitialised = true;
            StateHasChanged();
            _loadingService.HideLoading();
        }

        public async Task GetCustomerCodeName()
        {
            Winit.Shared.Models.Common.ApiResponse<List<Winit.Modules.Store.Model.Classes.Store>> responsed = await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Classes.Store>>($"{_appConfigs.ApiBaseUrl}CollectionModule/GetCustomerCode?CustomerCode=" + "", HttpMethod.Get, null);
            if (responsed != null && responsed.Data != null)
            {
                CustomerCode = responsed.Data;
            }
            foreach (var store in CustomerCode)
            {
                storeData[store.UID] = store.Name;
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
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["view_payments"], IsClickable = true, URL = "viewpayments" });
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["payment_details"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["payment_details"];
            await CallbackService.InvokeAsync(_IDataService);
        }

        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_navigate.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }
        public void columns()
        {
            //add columns
            ViewPaymentDetailsColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["document_type"], GetValue = s => ((AccCollectionAllotment)s).TargetType  },
                new DataGridColumn { Header = @Localizer["document_no"], GetValue = s => ((AccCollectionAllotment)s).ReferenceNumber },
                new DataGridColumn { Header = @Localizer["currency"], GetValue = s => ((AccCollectionAllotment)s).CurrencyUID },
                new DataGridColumn { Header = @Localizer["amount"], GetValue = s => ((AccCollectionAllotment)s).TargetType.Contains("OA") ? (((AccCollectionAllotment)s).CurrencyUID.Contains("USD") ?
                ((AccCollectionAllotment)s).DefaultCurrencyAmount / 80 : ((AccCollectionAllotment)s).DefaultCurrencyAmount ): ((AccCollectionAllotment)s).TargetType.Contains("CREDITNOTE") ?
                ((AccCollectionAllotment)s).Amount*-1 : ((AccCollectionAllotment)s).CurrencyUID.Contains("USD") ? ((AccCollectionAllotment)s).Amount / 80 + ((AccCollectionAllotment)s).EarlyPaymentDiscountAmount :
                ((AccCollectionAllotment)s).Amount + ((AccCollectionAllotment)s).EarlyPaymentDiscountAmount  },
            };
        }
        private List<IExchangeRate> ViewMultiCurrencyDetails { get; set; } = new List<IExchangeRate>();
        private List<IAccCollectionCurrencyDetails> ConvertMultiCurrencyDetails { get; set; } = new List<IAccCollectionCurrencyDetails>();
        public bool IsShowMultiCurrency { get; set; } = false;
        private async Task View()
        {
            try
            {
                ViewMultiCurrencyDetails.Clear();
                ConvertMultiCurrencyDetails = await _createPaymentViewModel.GetMultiCurrencyDetails(UID);
                for (int i = 0; i < ConvertMultiCurrencyDetails.Count; i++)
                {
                    ViewMultiCurrencyDetails.Add(new ExchangeRate()); // Example: Replace ExchangeRate with your concrete implementation of IExchangeRate
                }
                for (int i = 0; i < ConvertMultiCurrencyDetails.Count; i++)
                {
                    ViewMultiCurrencyDetails[i].FromCurrencyUID = ConvertMultiCurrencyDetails[i].currency_uid;
                    ViewMultiCurrencyDetails[i].Rate = (decimal)ConvertMultiCurrencyDetails[i].default_currency_exchange_rate;
                    ViewMultiCurrencyDetails[i].CurrencyAmount = (decimal)ConvertMultiCurrencyDetails[i].amount;
                    ViewMultiCurrencyDetails[i].ConvertedAmount = (decimal)ConvertMultiCurrencyDetails[i].default_currency_amount;
                    ViewMultiCurrencyDetails[i].ToCurrencyUID = ConvertMultiCurrencyDetails[i].default_currency_uid;
                }
                IsShowMultiCurrency = !IsShowMultiCurrency;
                if (IsShowMultiCurrency)
                {
                    await _multi.OnInit(PaymentType);
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task CloseMultiCurrency()
        {
            try
            {
                IsShowMultiCurrency = !IsShowMultiCurrency;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
