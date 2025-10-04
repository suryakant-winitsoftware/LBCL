using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Practice;
using System.Globalization;
using System.Resources;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.Collection.NonCashSettlementDetails
{
    public partial class Settled
    {
        private string DocumentType { get; set; } = "";
        private string UID { get; set; } = "";
        private string TargetUID { get; set; } = "";
        private string sampletext { get; set; } = "";
        private string SessionUserCode { get; set; } = "";
        private string BankName { get; set; } = "";
        private string BranchName { get; set; } = "";
        private string ChequeNo { get; set; } = "";
        private DateTime? ChequeDate { get; set; } = (DateTime.Now).Date;
        private string Status { get; set; } = "";
        private string Comments { get; set; } = "";
        private string Comments1 { get; set; } = "";
        private string Approved { get; set; } = "Approved";
        private string Bounced { get; set; } = "Bounced";
        private string Message { get; set; } = "";
        private string ReceiptNumber { get; set; } = "";
        private string ChequeNo1 { get; set; } = "";
        private decimal UnSettledAmount { get; set; } = 0;
        private string Amount { get; set; } = "";
        private AccCollectionPaymentMode[] bank { get; set; } = new AccCollectionPaymentMode[0];
        private bool showAlert = false;
        private bool showAlert1 = false;
        private bool IsInitialised { get; set; } = false;
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _loadingService.ShowLoading();
                UID = GetParameterValueFromURL("UID");
                Amount = GetParameterValueFromURL("Amount");
                TargetUID = GetParameterValueFromURL("TargetUID");
                SessionUserCode = GetParameterValueFromURL("SessionUserCode");
                ReceiptNumber = GetParameterValueFromURL("ReceiptNumber");
                ChequeNo1 = GetParameterValueFromURL("ChequeNo");
                await _settlePaymentViewModel.GetChequeDetails(UID, TargetUID);
                bank = _settlePaymentViewModel.Bank;
                LoadResources(null, _languageService.SelectedCulture);
                foreach (var list in bank)
                {
                    BankName = list.BankUID;
                    BranchName = list.Branch;
                    ChequeDate = list.ChequeDate;
                    ChequeNo = list.ChequeNo;
                    Status = list.Status;
                    Comments = list.Comments;
                }
                await SetHeaderName();
                IsInitialised = true;
                _loadingService.HideLoading();
            }
            catch (Exception ex)
            {

            }

        }
      
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["non-cash_settlement"], IsClickable = true, URL = "settlement" });
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 3, Text = @Localizer["settled_collections"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["settled_collections"];
            await CallbackService.InvokeAsync(_IDataService);
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }
        public async Task UpdateFields()
        {
            try
            {
                bool IsUpdated = await _settlePaymentViewModel.UpdateFields_Data(bank.FirstOrDefault().UID, bank.FirstOrDefault().BankUID, bank.FirstOrDefault().Branch, bank.FirstOrDefault().ChequeNo);
                if (IsUpdated)
                {
                    await _alertService.ShowSuccessAlert(@Localizer["success"], "Bank details updated successfully");
                }
                else
                {
                    await _alertService.ShowSuccessAlert(@Localizer["failure"], "Bank details failed to update");
                }
            }
            catch (Exception ex)
            {

            }
        }
        protected async Task Clicked(string butt)
        {
            try
            {
                Message = butt;
                SessionUserCode = "1001";
                Comments1 = sampletext;
                if (sampletext != "" && sampletext != null)
                {
                    if (butt == "Approved")
                    {
                        Winit.Shared.Models.Common.ApiResponse<string> response = await _settlePaymentViewModel.OnClickApproveReject(UID, butt, Comments1, SessionUserCode, ReceiptNumber, ChequeNo1);
                        if (response.StatusCode == 201)
                        {
                            await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["approved_successfully"] );
                            _NavigationManager.NavigateTo("settlement?SessionUserCode=" + SessionUserCode);
                        }
                        else
                        {
                            await _alertService.ShowErrorAlert(@Localizer["failure"], @Localizer["approve_failed"] );
                        }
                    }
                    if (butt == "Bounced")
                    {
                        Winit.Shared.Models.Common.ApiResponse<string> response = await _settlePaymentViewModel.OnClickApproveReject(UID, butt, Comments1, SessionUserCode, ReceiptNumber, ChequeNo1);
                        if (response.StatusCode == 201)
                        {
                            await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["bounced_successfully"] );
                            _NavigationManager.NavigateTo("settlement?SessionUserCode=" + SessionUserCode);
                        }
                        else
                        {
                            await _alertService.ShowErrorAlert(@Localizer["failure"], @Localizer["bounce_failed"] );
                        }
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_fill_comments"] );
                }
            }
            catch (Exception ex)
            {

            }
        }








    }
}
