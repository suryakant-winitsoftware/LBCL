using Azure;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.Common.BL;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.Tax.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Common.GridState;
using WinIt.BreadCrum.Classes;
using WinIt.BreadCrum.Interfaces;

using WinIt.Pages.Base;
using WinIt.Pages.SalesManagement.Distributor;



namespace WinIt.Pages.SalesManagement.PriceManagement
{
    /// <summary>
    /// Manages customer price list functionality including viewing, filtering and managing price lists.
    /// </summary>
    public partial class ManageCustomerPriceList : BaseComponentBase
    {
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService { get; set; }
        private List<Winit.Modules.SKU.Model.Classes.SKUPriceList> _data = new();

        public string Status = Winit.UIModels.Web.SKUConstant.SkuPriceListConstant.Published;
        List<DataGridColumn> productColumns { get; set; }

        TableState state = new Winit.UIModels.Common.GridState.TableState();
        private bool isShowPopup = false;
        private bool isLoaded = false;
        private string Message = "";
        private bool IsDrafttabselected = false;
        private bool IsActivetabselected = true;
        private bool IsArchievedtabselected = false;
        private int _totalItems = 0;
        private string _searchString = null;
        private string Name = "", Code = "", Type = "";
        private int _currentPage = 1;
        private int _pageSize = 20;
        private string _defaultSortColumn = "Code";
        SelectionManager TabSM { get; set; }
        List<ISelectionItem> TabItems { get; set; } = new List<ISelectionItem>
        {
            new SelectionItem { Label = "Draft", UID = Winit.UIModels.Web.SKUConstant.SkuPriceListConstant.Draft, IsSelected = false },
            new SelectionItem { Label = "Active", UID = Winit.UIModels.Web.SKUConstant.SkuPriceListConstant.Published, IsSelected = true },
            new SelectionItem { Label = "Archived", UID = Winit.UIModels.Web.SKUConstant.SkuPriceListConstant.Archieved, IsSelected = false }
        };
        //    private SortDirection _defaultSortDirection = SortDirection.Asc;
        int slNo = 0;
        string response;
        public List<Winit.Shared.Models.Enums.FilterCriteria> _FilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>();
        private static NavigationManager navigationManager;
        PagingRequest PagingRequest = new PagingRequest()
        {
            IsCountRequired = true,
            PageNumber = 1,
            PageSize = 20,
            FilterCriterias = new List<FilterCriteria>(),
            SortCriterias = new()
            {
                new SortCriteria("Modifiedtime",SortDirection.Desc)
            }
        };
        protected override void OnInitialized()
        {
            slNo = 0;
            TabSM = new SelectionManager(TabItems, SelectionMode.Single);
            LoadResources(null, _languageService.SelectedCulture);
            SetHeaderName();
            FilterInitialized();
            SetTableHeaderNames();

        }
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            //await GetPriceLists(Winit.UIModels.Web.SKUConstant.SkuPriceListConstant.Published);
            await StateChageHandler();
            isLoaded = true;
            _loadingService.HideLoading();
        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("ManageCustomerPriceList", ref ColumnsForFilter, out PageState pageState);

            ///only work with filters
            ///
            Status = stateRestored && pageState != null && pageState.SelectedTabUID != null ? pageState.SelectedTabUID :
                Winit.UIModels.Web.SKUConstant.SkuPriceListConstant.Published;
            //if tabs also there should use this also

            await OnFilterApply(_pageStateHandler._currentFilters);
            TabItems.ForEach(p => p.IsSelected = (p.UID == Status));
            //if tabs also there should use this also



        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("ManageCustomerPriceList", TabSM.GetSelectedSelectionItems().FirstOrDefault()?.UID ?? "");
        }
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        private bool showFilterComponent = false;
        public async void ShowFilter()
        {
            showFilterComponent = !showFilterComponent;
            filterRef.ToggleFilter();
        }
        public async Task GetPriceLists(string status)
        {
            _loadingService.ShowLoading();
            this.Status = status;

            //PagingRequest.FilterCriterias = new();
            //foreach (FilterCriteria filterModel in SearchFilter)
            //{
            //    PagingRequest.FilterCriterias.Add(new FilterCriteria(filterModel.Name, filterModel.Value, filterModel.Type));
            //}
            FilterCriteria? filter = PagingRequest.FilterCriterias!.Find(e => e.Name == "Status");
            if (filter != null)
                filter.Value = status;
            else
                PagingRequest.FilterCriterias!.Add(new FilterCriteria("Status", status, FilterType.Equal));
            await GetDataFromAPIAsync();
            _loadingService.HideLoading();
        }
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            try
            {
                _loadingService.ShowLoading();
                _pageStateHandler._currentFilters = filterCriteria;
                PagingRequest.FilterCriterias!.Clear();
                foreach (var item in filterCriteria)
                {
                    FilterCriteria filter = new FilterCriteria(item.Key, item.Value, FilterType.Like);
                    PagingRequest.FilterCriterias.Add(filter);
                }
                PagingRequest.PageNumber = 1;
                await GetPriceLists(Status);
            }
            catch (Exception ex)
            {
                _tost.Add(@Localizer["error"], ex.Message);
            }
            finally
            {
                _loadingService.HideLoading();
            }
        }
        async Task OnTabClick(ISelectionItem selectionItem)
        {
            if (!selectionItem.IsSelected)
            {
                TabSM?.Select(selectionItem);
                await GetPriceLists(selectionItem.UID);
            }
        }
        public void FilterInitialized()
        {

            ColumnsForFilter = new List<FilterModel>
             {
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,ColumnName = "Code", Label = @Localizer["code"]},
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, ColumnName = "Name", Label = @Localizer["name"]},
             };
        }
        protected void SetTableHeaderNames()
        {
            productColumns = new List<DataGridColumn>
        {
            new DataGridColumn { Header = @Localizer["code"], GetValue = s => ((Winit.Modules.SKU.Model.Classes.SKUPriceList)s).Code, IsSortable = false, SortField = "Code" },
            new DataGridColumn { Header = @Localizer["name"], GetValue = s => ((Winit.Modules.SKU.Model.Classes.SKUPriceList)s).Name, IsSortable = false, SortField = "Name" },
            new DataGridColumn { Header = @Localizer["priority"], GetValue = s => ((Winit.Modules.SKU.Model.Classes.SKUPriceList)s).Priority, IsSortable = false, SortField = "ArabicName" },
            new DataGridColumn { Header = @Localizer["valid_from"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((Winit.Modules.SKU.Model.Classes.SKUPriceList)s).ValidFrom) },
            new DataGridColumn { Header = @Localizer["valid_upto"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((Winit.Modules.SKU.Model.Classes.SKUPriceList)s).ValidUpto) },
            new DataGridColumn
            {
                Header = @Localizer["action"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        Text = @Localizer["edit"],
                        Action = item => ViewEditCustomerPriceList((Winit.Modules.SKU.Model.Classes.SKUPriceList)item)
                    },

                }
            }
        };
        }
        protected void getsingleRow(Winit.Modules.SKU.Model.Classes.SKUPriceList sKUPriceList)
        {

        }

        private async void SearchCustomer()
        {
            _FilterCriterias = new();
            _loadingService.ShowLoading();
            _currentPage = 1;
            if (!string.IsNullOrEmpty(Name))
                _FilterCriterias.Add(new Winit.Shared.Models.Enums.FilterCriteria("name", Name, Winit.Shared.Models.Enums.FilterType.Like));
            if (!string.IsNullOrEmpty(Code))
                _FilterCriterias.Add(new Winit.Shared.Models.Enums.FilterCriteria("code", Code, Winit.Shared.Models.Enums.FilterType.Like));
            PagingRequest.FilterCriterias = _FilterCriterias;
            await GetDataFromAPIAsync();
            _loadingService.HideLoading();
            StateHasChanged();
        }
        private async void ResetCustomer()
        {
            _loadingService.ShowLoading();
            _currentPage = 1;
            Name = string.Empty; Type = string.Empty; _FilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>();
            await GetDataFromAPIAsync();
            _loadingService.HideLoading();
            StateHasChanged();
        }

        [CascadingParameter]
        public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public void SetHeaderName()
        {
            dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
                {
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text= @Localizer["maintain_customer_price_list"], IsClickable = false, URL= "ManageCustomerPriceList" }
                },
                HeaderText = @Localizer["maintain_customer_price_list"]
            };
        }
        private void GetExcelData(Dictionary<string, List<string>> excel)
        {

        }
        private async Task GetDataFromAPIAsync()
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                   $"{_appConfigs.ApiBaseUrl}SKUPriceList/SelectAllSKUPriceListDetails",
                    HttpMethod.Post, PagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);

                    PagedResponse<Winit.Modules.SKU.Model.Classes.SKUPriceList> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUPriceList>>(data);
                    if (pagedResponse != null)
                    {

                        if (pagedResponse.TotalCount > 0)
                        {
                            _data = pagedResponse.PagedData.ToList();
                            _totalItems = pagedResponse.TotalCount;
                        }
                        else if (pagedResponse.TotalCount == 0)
                        {
                            _data = new();
                            _totalItems = 0;
                        }
                    }
                }
                else
                {
                    _totalItems = 0;
                }
            }
            catch (Exception ex)
            {
                _totalItems = 0;
            }
        }

        private async Task DeleteSKUPriceList(string UID)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                  $"{_appConfigs.ApiBaseUrl}SKUPriceList/DeleteSKUPriceList?UID={UID}",
                   HttpMethod.Delete);

                if (apiResponse != null && apiResponse.StatusCode == 200)
                {
                    response = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                    if (response == "1")
                    {
                        Message = @Localizer["deleted_sucessfully"];
                    }
                    else
                    {
                        Message = response;
                    }
                    isShowPopup = true;
                }

            }
            catch (Exception ex) { }
        }
        private void navigate(string Uid)
        {

        }


        private async Task OnPageChanged(int newPage)
        {
            _currentPage = newPage;

        }

        private void ViewEditCustomerPriceList(Winit.Modules.SKU.Model.Classes.SKUPriceList sKUPrice)
        {
            _navigationManager.NavigateTo($"AddEditPrice?PageType=Edit&UID={sKUPrice.UID}");
        }


    }
}
