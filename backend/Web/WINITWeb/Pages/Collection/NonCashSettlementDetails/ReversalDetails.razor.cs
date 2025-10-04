using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Practice;
using System.Globalization;
using System.Resources;
using Winit.Modules.CollectionModule.Model.Classes;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.Collection.NonCashSettlementDetails
{
    public partial class ReversalDetails
    {
        private static string Reversal { get; set; } = "Reversal";
        private static string Void { get; set; } = "Void";
        private static string msg { get; set; } = "";
        private string ReceiptNumber { get; set; } = "";
        private string Customer { get; set; } = "";
        private string Date { get; set; } = "";
        private string Comments { get; set; } = "";
        private string Salesman { get; set; } = "";
        private string SessionUserCode { get; set; } = "";
        private string ReasonforCancelation { get; set; } = "";
        private string sampletext { get; set; } = "";
        private string Amount { get; set; } = "";
        private string ChequeNo { get; set; } = "";
        private string Route { get; set; } = "";
        private string PaymentMode { get; set; } = "";
        private string DocumentType { get; set; } = "";
        private string CustomerName { get; set; } = "";
        private string Button { get; set; } = "";
        private string Button1 { get; set; } = "";
        private string Button2 { get; set; } = "";
        private static string IsReversed { get; set; } = "";
        private static string IsSettled { get; set; } = "";
        private static string IsVoid { get; set; } = "";
        private bool showAlert = false;
        private bool showAlert1 = false;
        private bool showAlert2 = false;
        private bool ShowModalPopUp = false;
        private bool buttonVisible = true;
        public List<AccCollection> UIData = new List<AccCollection>();
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _loadingService.ShowLoading();
                ReceiptNumber = GetParameterValueFromURL("ReceiptNumber");
                Customer = GetParameterValueFromURL("Customer");
                Date = GetParameterValueFromURL("Date");
                Salesman = GetParameterValueFromURL("Salesman");
                Amount = GetParameterValueFromURL("Amount");
                ChequeNo = GetParameterValueFromURL("ChequeNo");
                SessionUserCode = GetParameterValueFromURL("SessionUserCode");
                ReasonforCancelation = GetParameterValueFromURL("ReasonforCancelation");
                Route = GetParameterValueFromURL("Route");
                Comments = GetParameterValueFromURL("Comments");
                ReceiptNumber = GetParameterValueFromURL("ReceiptNumber");
                PaymentMode = GetParameterValueFromURL("PaymentMode");
                DocumentType = GetParameterValueFromURL("DocumentType");
                CustomerName = GetParameterValueFromURL("CustomerName");
                IsReversed = GetParameterValueFromURL("IsReversed");
                IsSettled = GetParameterValueFromURL("IsSettled");
                IsVoid = GetParameterValueFromURL("IsVoid");
                Conditions();
                LoadResources(null, _languageService.SelectedCulture);
                await SetHeaderName();
                _loadingService.HideLoading();
            }
            catch (Exception ex)
            {

            }

        }

        private void Conditions()
        {
            if (IsReversed == "Collected")
            {
                Button = "Void";
                Button1 = "Collected";
            }
            if (IsReversed == "Settled")
            {
                Button = "Reversal";
                Button1 = "Settled";
            }
            if (IsReversed == "Voided")
            {
                Button1 = "Voided";
            }
            if (IsReversed == "Reversed")
            {
                Button1 = "Reversed";
            }
        }

        private void Close()
        {
            ShowModalPopUp = false;
        }
        private async Task Reverse()
        {
            try
            {
                ShowModalPopUp = false;
                if (msg == "Reversal")
                {
                    if (!string.IsNullOrEmpty(ReasonforCancelation))
                    {
                        Winit.Shared.Models.Common.ApiResponse<string> response = await _cashSettlementViewModel.ReceiptReverseByCash(ReceiptNumber, Amount, ChequeNo, ReasonforCancelation);
                        if (response.StatusCode == 201)
                        {
                            await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["reversed_successfully"]);
                            buttonVisible = false;
                            //_NavigationManager.NavigateTo("payment?PaymentUID=" + PaymentUID + "&ReceiptNumber=" + ReceiptNumber + "&PaymentMode=" + PaymentMode + "&DocumentType=" + DocumentType + "&CustomerName=" + CustomerName);
                            _NavigationManager.NavigateTo("cash");
                        }
                        else
                        {
                            await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["error"]);
                        }
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_fill_mandatory_field(s)"]);
                    }
                }
                if (msg == "Void")
                {
                    if (!string.IsNullOrEmpty(ReasonforCancelation))
                    {
                        Winit.Shared.Models.Common.ApiResponse<string> response = await _cashSettlementViewModel.ReceiptVOIDByCash(ReceiptNumber, Amount, ChequeNo, ReasonforCancelation);
                        if (response.StatusCode == 201)
                        {
                            await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["voided_successfully"]);
                            buttonVisible = false;
                            if (!ReceiptNumber.Contains("OA -"))
                            {
                                bool result = await _createPaymentViewModel.UpdateCollectionLimit(Convert.ToDecimal(Amount), _iAppUser.Emp.UID, 1);
                            }
                            //_NavigationManager.NavigateTo("payment?PaymentUID=" + PaymentUID + "&ReceiptNumber=" + ReceiptNumber + "&PaymentMode=" + PaymentMode + "&DocumentType=" + DocumentType + "&CustomerName=" + CustomerName);
                            _NavigationManager.NavigateTo("cash");
                        }
                        else
                        {
                            await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["error"]);

                        }
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_fill_mandatory_field(s)"]);
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }


        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }

        protected async Task Clicked(string butt)
        {
            try
            {
                msg = butt;
                if (!string.IsNullOrEmpty(ReasonforCancelation))
                {
                    ShowModalPopUp = true;
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_fill_mandatory_field(s)"]);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 3, Text = @Localizer["cashier_settlement"], IsClickable = true, URL = "cash" });
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 3, Text = @Localizer["details"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["details"];
            await CallbackService.InvokeAsync(_IDataService);
        }
    }
}
