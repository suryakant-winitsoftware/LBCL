using Azure;
using Microsoft.AspNetCore.Components;
using Nest;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common;
using Winit.UIModels.Web.SKU;
using WinIt.BreadCrum.Interfaces;

using WinIt.Pages.Base;

namespace WinIt.Pages.SalesManagement.IndividualPrice
{
    public partial class IndividualPriceList : BaseComponentBase
    {
        private string priceListUID = string.Empty;
        public int _currentPage { get; set; } = 1;

        public int _pageSize = 20;

        public int _totalItems = 0;
        public string SortLabel { get; set; }
        public string Message;
        public SortDirection SortDirection { get; set; }
        public Winit.UIModels.Web.SKU.SKUPriceView _data { get; set; }

        private bool isLoad = false;
        private bool isNewPriceList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> sKUPriceList { get; set; } = new();


        PriceListComponent _priceListComponent;
        protected override void OnInitialized()
        {
            _viewModel.IsIndividualPricelist = true;
            base.OnInitialized();
        }
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            await _viewModel.PopulateViewmodel(Winit.Shared.Models.Constants.UIDType.PriceList_UIDType +
                _commonFunctions.GetParameterValueFromURL("UID"));
            //await GetDataFromAPIAsync();
            //await SetHeaderName();
            isLoad = true;
            LoadResources(null, _languageService.SelectedCulture);
            _loadingService.HideLoading();
        }


        //[CascadingParameter]
        //public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Maintain Customer", IsClickable = true, URL = "ManageCustomers" });
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = "Add/Edit Customer Price ", IsClickable = false, URL = "Store" });
        //    _IDataService.HeaderText = "Add/Edit Customer Price ";
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        //private async Task GetDataFromAPIAsync()
        //{

        //    PagingRequest pagingRequest = new PagingRequest();
        //    pagingRequest.SortCriterias = new List<Winit.Shared.Models.Enums.SortCriteria>();
        //    pagingRequest.PageNumber = _currentPage;
        //    pagingRequest.PageSize = _pageSize;
        //    pagingRequest.FilterCriterias = null;
        //    pagingRequest.IsCountRequired = true;
        //    try
        //    {
        //        Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
        //            $"{_appConfigs.ApiBaseUrl}SKUPrice/SelectSKUPriceViewByUID?UID={priceListUID}",
        //            HttpMethod.Post, pagingRequest);


        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        //        {
        //            string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);

        //            Winit.Shared.Models.Common.PagedResponse<Winit.UIModels.Web.SKU.SKUPriceView> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.UIModels.Web.SKU.SKUPriceView>>(data);
        //            if (pagedResponse != null)
        //            {
        //                if (pagedResponse.PagedData != null)
        //                {
        //                    _data = pagedResponse.PagedData.FirstOrDefault();
        //                    if (_data.SKUPriceGroup == null)
        //                    {
        //                        isNewPriceList = true;
        //                        _data.SKUPriceGroup = _serviceProvider.CreateInstance<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList>();
        //                    }
        //                    else
        //                    {
        //                        if (_data.SKUPriceList != null)
        //                        {
        //                            sKUPriceList = _data.SKUPriceList;
        //                        }
        //                    }

        //                }
        //            }

        //        }
        //        else
        //        {
        //            _totalItems = 0;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _totalItems = 0;
        //    }
        //}
        private void DefaultFields()
        {
            _viewModel.SKUPriceView.SKUPriceGroup.Code = "CustomerPriceListCode";
            _viewModel.SKUPriceView.SKUPriceGroup.Name = "CustomerPriceListName";
            _viewModel.SKUPriceView.SKUPriceGroup.ValidFrom = DateTime.Now;
            _viewModel.SKUPriceView.SKUPriceGroup.ValidUpto = DateTime.MaxValue;
            _viewModel.SKUPriceView.SKUPriceGroup.Status = Winit.UIModels.Web.SKUConstant.SkuPriceListConstant.Published;
        }
        protected async Task SaveOrUpdate()
        {

            int count = 0;
            if (_viewModel.IsNew)
                DefaultFields();

            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await
                _viewModel.SaveSKUPrices(_priceListComponent.GetModifiedSKUPricesList(Winit.UIModels.Web.SKUConstant.SkuPriceListConstant.Published));
            if (apiResponse != null)
            {
                if (apiResponse.StatusCode == 200)
                {
                    count = Winit.Shared.CommonUtilities.Common.CommonFunctions.GetIntValue(apiResponse.Data);
                    if (count > 0)
                    {
                        ShowSuccessSnackBar(@Localizer["success"], @Localizer["saved_successfully"]);
                    }
                    else
                    {

                        ShowErrorSnackBar(@Localizer["error"], apiResponse.ErrorMessage);
                    }
                }
                else
                {
                    ShowErrorSnackBar(@Localizer["error"], apiResponse.ErrorMessage);
                }



            }
            if (count > 0)
            {
                _navigationManager.NavigateTo("ManageCustomers");
            }

        }
        protected async Task Save()
        {
            //List<Winit.UIModels.Web.SKU.SKUPrice> skuPriceLis = items.GetModifiedSKUPricesList();
            _priceListComponent.GetValidated(out bool isValidated, out string errorMessage);
            if (_priceListComponent.ModifiedSkuPriceList == null || _priceListComponent.ModifiedSkuPriceList.Count == 0)
            {
                await _alertService.ShowErrorAlert("Error", "please select atleast one sku before confirming!");
                return;
            }
            object obj;
            string methodName = "";
            HttpMethod method;
            _data.SKUPriceList = _priceListComponent.ModifiedSkuPriceList;


            if (isNewPriceList)
            {
                _data.SKUPriceGroup = _serviceProvider.CreateInstance<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList>();
                _data.SKUPriceGroup.UID = priceListUID;
               
                _data.SKUPriceGroup.CreatedBy = _iAppUser.Emp.UID;
                _data.SKUPriceGroup.ModifiedBy = _iAppUser.Emp.UID;

                _data.SKUPriceGroup.CreatedTime = DateTime.Now;
                _data.SKUPriceGroup.ModifiedTime = DateTime.Now;
                _data.SKUPriceGroup.ServerAddTime = DateTime.Now;
                _data.SKUPriceGroup.ServerModifiedTime = DateTime.Now;

                methodName = "CreateSKUPriceView";
                method = HttpMethod.Post;
                obj = _data;
            }
            else
            {
                methodName = "UpdateSKUPriceList";
                method = HttpMethod.Put;
                obj = _data.SKUPriceList;
            }
            bool isConfirm = true;
            if (_data.SKUPriceList.Any())
            {
                isConfirm = await _alertService.ShowConfirmationReturnType("Confirm", "Are you sure you want to Confirm");
            }
            if (isConfirm)
            {

                Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                       $"{_appConfigs.ApiBaseUrl}SKUPrice/{methodName}",
                       method, obj);
                string response;
                int count;
                if (apiResponse != null)
                {
                    if (apiResponse.StatusCode == 200)
                    {
                        response = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                        count = Winit.Shared.CommonUtilities.Common.CommonFunctions.GetIntValue(response);
                        if (count > 0)
                        {
                            Message = "Saved Successfully !";
                            _navigationManager.NavigateTo("ManageCustomers");
                            await _alertService.ShowSuccessAlert("Success", Message);
                        }
                        else
                        {
                            Message = response;
                            await _alertService.ShowErrorAlert("Error", Message);
                        }
                    }
                    else
                    {
                        Message = apiResponse.ErrorMessage;
                        await _alertService.ShowErrorAlert("Error", Message);
                    }

                }
            }
            else
            {
                //  _navigationManager.NavigateTo("ManageCustomers");
            }

        }

    }
}
