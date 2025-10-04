using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Practice;
using System.Globalization;
using System.Resources;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Shared.Models.Common;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.Collection.NonCashSettlementDetails
{
    public partial class Approved
    {
        private string DocumentType { get; set; } = "";
        private string UID { get; set; } = "";
        private string TargetUID { get; set; } = "";
        private string sampletext { get; set; } = "";
        private string SessionUserCode { get; set; } = "";
        private string BankName { get; set; } = "";
        private string BranchName { get; set; } = "";
        private string ReceiptNumber { get; set; } = "";
        private string ChequeNo { get; set; } = "";
        private DateTime? TransferDate { get; set; } = (DateTime.Now).Date;
        private string Status { get; set; } = "";
        private string Comments { get; set; } = "";
        private string ApproveComments { get; set; } = "";
        private string Amount { get; set; } = "";
        private string ReversalComments { get; set; } = "";
        private string Comments1 { get; set; } = "";
        private decimal UnSettledAmount { get; set; } = 0;
        private AccCollectionPaymentMode[] bank { get; set; } = new AccCollectionPaymentMode[0];
        private bool reverse = true;
        private bool Boolen = false;
        private decimal ChequeAmount = 0;
        private AccCollection[] ResponsedData { get; set; } = new AccCollection[0];
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }


        protected override async Task OnInitializedAsync()
        {
            try
            {
                UID = GetParameterValueFromURL("UID");
                Amount = GetParameterValueFromURL("Amount");
                ChequeAmount = Convert.ToDecimal(GetParameterValueFromURL("Amount"));
                TargetUID = GetParameterValueFromURL("TargetUID");
                ChequeNo = GetParameterValueFromURL("ChequeNo");
                SessionUserCode = GetParameterValueFromURL("SessionUserCode");
                ReceiptNumber = GetParameterValueFromURL("ReceiptNumber");
                Boolen = Convert.ToBoolean(GetParameterValueFromURL("Boolen"));
                await _approvePaymentViewModel.GetChequeDetails(UID, TargetUID);
                bank = _approvePaymentViewModel.Bank;
                LoadResources(null, _languageService.SelectedCulture);
                foreach (var list in bank)
                {
                    BankName = list.BankUID;
                    BranchName = list.Branch;
                    TransferDate = list.ChequeDate;
                    Status = list.Status;
                    Comments = list.Comments;
                    ApproveComments = list.ApproveComments;
                }
                await _approvePaymentViewModel.CheckReversalPossible(UID);
                ResponsedData = _approvePaymentViewModel.ReversalData;
                reverse = ResponsedData[0].Status.Contains("Reversed") ? false : true;
                ReversalComments = ResponsedData[0].Comments == null || ResponsedData[0].Comments == "" ? "N/A" : ResponsedData[0].Comments;
                await SetHeaderName();
            }
            catch (Exception ex)
            {

            }

        }
       
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["non-cash_settlement"], IsClickable = true, URL = "settlement" });
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 3, Text = @Localizer["approved_collections"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["approved_collections"];
            await CallbackService.InvokeAsync(_IDataService);
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }

        protected async Task Clicked()
        {
            try
            {
                if (reverse)
                {
                    if (!string.IsNullOrEmpty(sampletext))
                    {
                        SessionUserCode = "1001";
                        Comments1 = sampletext;
                        Winit.Shared.Models.Common.ApiResponse<string> response = await _approvePaymentViewModel.ReceiptReversal(UID, ChequeAmount, ChequeNo, SessionUserCode, sampletext);
                        if (response.StatusCode == 201)
                        {
                            await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["reversed_successfully"]);
                            _NavigationManager.NavigateTo("settlement?SessionUserCode=" + SessionUserCode);
                        }
                        else
                        {
                            await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["can't_reverse"]);
                        }
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_enter_comments"]);
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["can't_reverse,_as_it_is_already_reversed"]);
                }
            }
            catch (Exception ex)
            {

            }
        }


    }
}
