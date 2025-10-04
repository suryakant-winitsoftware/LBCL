using Azure;
using MathNet.Numerics;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Mapping.Model.Classes;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Web.SalesManagement.PriceManagement;
using Winit.UIModels.Common.GridState;
using Winit.UIModels.Web.SKU;
using WinIt.BreadCrum.Interfaces;

using WinIt.Pages.Base;

namespace WinIt.Pages.SalesManagement.PriceManagement
{
    public partial class AddEditCustomerGroupPriceList : BaseComponentBase
    {
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService { get; set; }


        private bool _addProducts = false;

        private string _defaultSortColumn = "Code";
        private Winit.Shared.Models.Enums.SortDirection _defaultSortDirection = Winit.Shared.Models.Enums.SortDirection.Asc;
        int slNo = 0;
        public List<Winit.Shared.Models.Enums.FilterCriteria> _FilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>();
        public Winit.UIModels.Web.SKU.SKUPriceView _data;

        List<SKU> sKU = new List<SKU>();
        public bool isLoad = false;
        public bool IsCustomerselected { get; set; }
        public bool DisplayMappingComponent { get; set; }
        MappingComponent? mappingComponent { get; set; }
        List<IMappingItemView>? mappingItemViews { get; set; }
        List<DataGridColumn>? dataGridColumns { get; set; }

        bool IsDisabled = true;

        PriceListComponent _priceListComponent;

        protected override void OnInitialized()
        {
            LoadResources(null, _languageService.SelectedCulture);
            SetHeaderName();
            dataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn
                {
                    Header = @Localizer["sl_no"],
                    GetValue = s =>((IMappingItemView)s).SNO
                },
                new DataGridColumn
                {
                    Header = @Localizer["type"],
                    GetValue = s =>((IMappingItemView)s)?.Type
                },
                new DataGridColumn
                {
                    Header = @Localizer["value"],
                    GetValue = s =>((IMappingItemView)s)?.Value
                },

            };
            base.OnInitialized();
        }
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            try
            {
                await _viewModel.PopulateViewmodel(_commonFunctions.GetParameterValueFromURL("UID"));
                await mappingViewModel.PopulateViewModel(_viewModel.SKUPriceView.SKUPriceGroup.UID, "CustomerPriceList");
            }
            catch (Exception ex) { }
            finally
            {
                HideLoader();
                isLoad = true;
                StateHasChanged();
            }
        }


        //{

        //    _IDataService.BreadcrumList = new List<IBreadCrum>();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_customer_price_list"], IsClickable = true, URL = "ManageCustomerPriceList"});
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = isNew == true ? @Localizer["add_new_customer_price_list"] : @Localizer["edit_customer_price_list"], IsClickable = false, URL = "" });
        //    _IDataService.HeaderText = isNew == true ? @Localizer["add_new_customer_price_list"] : @Localizer["edit_customer_price_list"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        public void SetHeaderName()
        {
            dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
          {
              new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_customer_price_list"], IsClickable = true, URL = "ManageCustomerPriceList"} ,
              new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 2, Text = _viewModel.IsNew == true ? @Localizer["add_new_customer_price_list"] : @Localizer["edit_customer_price_list"], IsClickable = false, URL = ""} ,
          },
                HeaderText = _viewModel.IsNew == true ? Localizer["add_new_customer_price_list"] : Localizer["edit_customer_price_list"]
            };
        }




        private async Task OnPageChanged(int newPage)
        {
            await _viewModel.OnPageChange(newPage);

        }
        protected async Task SaveOrUpdate(string status)
        {
            ShowLoader();
            try
            {
                _viewModel.SKUPriceView.SKUPriceGroup.Status = status;
                DatamanagerGeneric<ISKUPriceViewModel>.Set(nameof(_viewModel), _viewModel);
                DatamanagerGeneric<List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>>.Set(nameof(_viewModel.SKUPriceView.SKUPriceList), _viewModel.SKUPriceView.SKUPriceList);
                var viewModel = DatamanagerGeneric<ISKUPriceViewModel>.GetOrDefault(nameof(_viewModel));
                var list = DatamanagerGeneric<ISKUPriceViewModel>.GetOrDefault(nameof(_viewModel.SKUPriceView.SKUPriceList));
                var apiResponse =
                    _viewModel.SaveSKUPrices(_priceListComponent.GetModifiedSKUPricesList(status));
                var mapping = mappingViewModel.SaveMapping();
                await Task.WhenAll(apiResponse, mapping);
                bool isSaved = false;
                if (apiResponse.Result != null)
                {
                    if (apiResponse.Result.StatusCode == 200)
                    {
                        isSaved = true;
                        await iAlert.ShowSuccessAlert(@Localizer["success"], @Localizer["saved_successfully"]);
                    }
                    else
                    {
                        await iAlert.ShowErrorAlert(@Localizer["error"], apiResponse.Result.ErrorMessage);
                    }
                }
                if (isSaved)
                {
                    _navigationManager.NavigateTo("ManageCustomerPriceList");
                }
            }
            catch (Exception ex)
            {
                await iAlert.ShowErrorAlert(@Localizer["error"], ex.Message);
            }
            finally
            {
                HideLoader();
            }

        }


        protected async Task EditMappings(IMappingItemView itemView)
        {
            DisplayMappingComponent = true;
        }


        //public async Task<int> SaveItemdetails(List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> sKUPrices, HttpMethod httpMethod, string responseMethod)
        //{
        //    int count = 0;
        //    if (_priceListComponent.Validate(sKUPrices))
        //    {
        //        _data.SKUPriceGroup = SKUPriceGroup;
        //        _data.SKUPriceList = sKUPrices;// 


        //        string response = string.Empty;
        //        Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
        //              $"{_appConfigs.ApiBaseUrl}SKUPrice/{responseMethod}",
        //              httpMethod, _data);


        //    }

        //    return count;
        //}



        //protected async Task AddNewProducts(bool isNew)
        //{
        //    if (isNew)
        //    {
        //        await GetAllProducts();
        //        _addProducts = true;
        //    }
        //}

        //protected void GetSelectedItems(List<SKU> sKUs)
        //{
        //    if (sKUs != null)
        //    {
        //        //sKU = sKUs;
        //        foreach (var sku in sKUs)
        //        {
        //            sKUPriceList.Add(new Winit.Modules.SKU.Model.Classes.SKUPrice()
        //            {
        //                UID = Guid.NewGuid().ToString(),
        //                SKUUID = sku.UID,
        //                SKUPriceListUID = priceListUID,
        //                ActionType = Winit.Shared.Models.Enums.ActionType.Add,
        //                IsModified = true,
        //                //SKUPriceListUID = sku.PriceListUID,
        //                SKUCode = sku.Code,
        //                ValidFrom = DateTime.Now,
        //                TempValidFrom = DateTime.Now,
        //                ValidUpto = DateTime.MaxValue,
        //                TempValidUpto = DateTime.MaxValue,
        //                IsEdit = true,
        //                ISNew = true,
        //                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
        //                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
        //                CreatedTime = DateTime.Now,
        //                ModifiedTime = DateTime.Now,
        //                ServerAddTime = DateTime.Now,
        //                ServerModifiedTime = DateTime.Now,

        //            });
        //        }
        //    }
        //    _addProducts = false;
        //}
        public async Task GetAllProducts()
        {
            Winit.Shared.Models.Common.PagingRequest pagingRequest = new Winit.Shared.Models.Common.PagingRequest();
            pagingRequest.PageNumber = 1;
            pagingRequest.PageSize = 1000;
            pagingRequest.IsCountRequired = true;
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SKU/SelectAllSKUDetails",
                    HttpMethod.Post, pagingRequest);


            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);

                Winit.Shared.Models.Common.PagedResponse<SKU> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<SKU>>(data);
                if (pagedResponse != null)
                {

                    if (pagedResponse.PagedData != null)
                    {
                        sKU = (List<SKU>)pagedResponse.PagedData;

                    }
                }
            }
            else
            {
                //_totalItems = 0;
            }
        }




    }
}
