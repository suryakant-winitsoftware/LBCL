using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.CollectionModule.BL.Classes.CreatePayment;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using WinIt.Pages.Collection.CreatePayment;

namespace WinIt.Pages.Collection.CashCollectionDeposit
{
    public partial class ApproveOrRejectDepositRequest : ComponentBase
    {
        public string RequestNo { get; set; } = "";
        public List<IAccCollection> accCollections { get; set; } = new List<IAccCollection>();
        private List<DataGridColumn> Columns { get; set; } = new List<DataGridColumn>();
        bool IsInitialize { get; set; }

        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                IsInitialize = false;
                _loadingService.ShowLoading();
                RequestNo = GetParameterValueFromURL("RequestNo");
                GridColumns();
                accCollections = await _cashCollectionDepostViewModel.ViewReceipts(RequestNo);
                _cashCollectionDepostViewModel.CashCollectionDeposit.ApprovedByEmpUID = _iAppUser.Emp.UID;
            }
            catch (Exception exd)
            {

            }
            await SetHeaderName();
            IsInitialize = true;
            StateHasChanged();
            _loadingService.HideLoading();
        }
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = "MaintainCashCollectionDeposit", IsClickable = true, URL = "maintaincashcollectiondeposit" });
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = "ApproveOrRejectRequest", IsClickable = false });
            _IDataService.HeaderText = "ApproveOrRejectRequest";
            await CallbackService.InvokeAsync(_IDataService);
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_navigate.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }
        public void GridColumns()
        {
            Columns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Receipt Number", GetValue = s => ((AccCollection)s).ReceiptNumber },
                new DataGridColumn { Header = "Date", GetValue = s => CommonFunctions.GetDateTimeInFormat(((AccCollection)s).CreatedTime) },
                new DataGridColumn { Header = "Amount", GetValue = s => ((AccCollection)s).Amount},
            };
        }
        public async Task ApproveRequest()
        {
            try
            {
                if (!string.IsNullOrEmpty(_cashCollectionDepostViewModel.CashCollectionDeposit.Comments))
                {
                    string result = await _cashCollectionDepostViewModel.ApproveOrRejectDepositRequest(_cashCollectionDepostViewModel.CashCollectionDeposit, "Approved");
                    if(result != "1")
                    {
                        await _alertService.ShowErrorAlert("Error" ,"Approve Failed");
                    }
                    else
                    {
                        await _alertService.ShowSuccessAlert("Success", "Request Approved successfully");
                        bool Result = await _createPaymentViewModel.UpdateCollectionLimit(_cashCollectionDepostViewModel.CashCollectionDeposit.Amount, _iAppUser.Emp.UID, 0);
                    }
                    _navigate.NavigateTo("maintaincashcollectiondeposit");
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", "Please enter Comments");
                }
            }
            catch(Exception ex)
            {

            }
        }
        public async Task RejectRequest()
        {
            try
            {
                if (!string.IsNullOrEmpty(_cashCollectionDepostViewModel.CashCollectionDeposit.Comments))
                {
                    string result = await _cashCollectionDepostViewModel.ApproveOrRejectDepositRequest(_cashCollectionDepostViewModel.CashCollectionDeposit, "Rejected");
                    if (result != "2")
                    {
                        await _alertService.ShowErrorAlert("Error", "Rejection Failed");
                    }
                    else
                    {
                        await _alertService.ShowSuccessAlert("Success", "Request rejected successfully");
                    }
                    _navigate.NavigateTo("maintaincashcollectiondeposit");
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", "Please enter Comments");
                }
            }
            catch(Exception ex)
            {

            }
        }
        public async Task MoveToPending()
        {
            try
            {
                if (!string.IsNullOrEmpty(_cashCollectionDepostViewModel.CashCollectionDeposit.Comments))
                {
                    string result = await _cashCollectionDepostViewModel.ApproveOrRejectDepositRequest(_cashCollectionDepostViewModel.CashCollectionDeposit, "Action Required");
                    if (result != "3")
                    {
                        await _alertService.ShowErrorAlert("Error", "Moving to Action Failed");
                    }
                    else
                    {
                        await _alertService.ShowSuccessAlert("Success", "Request moved to Action successfully");
                    }
                    _navigate.NavigateTo("maintaincashcollectiondeposit");
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", "Please enter Comments");
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
