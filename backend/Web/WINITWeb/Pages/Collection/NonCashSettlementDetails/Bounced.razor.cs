using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Winit.Modules.CollectionModule.Model.Classes;
using System.Globalization;
using System.Resources;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.Collection.NonCashSettlementDetails
{
    public partial class Bounced
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
        private string Message { get; set; } = "";
        private string ReceiptNumber { get; set; } = "";
        private string ChequeNo1 { get; set; } = "";
        private decimal UnSettledAmount { get; set; } = 0;
        private string Amount { get; set; } = "";
        private AccCollectionPaymentMode[] bank { get; set; } = new AccCollectionPaymentMode[0];
        private bool showAlert = false;
        private bool showAlert1 = false;
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                UID = GetParameterValueFromURL("UID");
                Amount = GetParameterValueFromURL("Amount");
                TargetUID = GetParameterValueFromURL("TargetUID");
                SessionUserCode = GetParameterValueFromURL("SessionUserCode");
                ReceiptNumber = GetParameterValueFromURL("ReceiptNumber");
                ChequeNo1 = GetParameterValueFromURL("ChequeNo");
                await _bouncePaymentViewModel.GetChequeDetails(UID, TargetUID);
                bank = _bouncePaymentViewModel.Bank;
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
            }
            catch (Exception ex)
            {

            }
        }
       
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["non-cash_settlement"], IsClickable = true, URL = "settlement" });
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 3, Text = @Localizer["bounced_collections"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["bounced_collections"];
            await CallbackService.InvokeAsync(_IDataService);
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }


    }
}
