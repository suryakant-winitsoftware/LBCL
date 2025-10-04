using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Classes
{
    public class ViewWareHouse_VanStockWebViewModel : ViewWareHouse_VanStockBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public ViewWareHouse_VanStockWebViewModel(IServiceProvider serviceProvider,
             IFilterHelper filter,
             ISortHelper sorter,
             IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService)
         : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            Warehouse_VanSalablestockList = new List<IWarehouseStockItemView>();
            Warehouse_VanFocstockList = new List<IWarehouseFOCStockItemView>();
            FilterCriterias = new List<FilterCriteria>();
            WareHouseSelectionItems = new List<ISelectionItem>();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public override async Task PopulateViewModelForSalableVan()
        {
            await base.PopulateViewModelForSalableVan();
        }
        public override async Task PopulateViewModelForFocVan()
        {
            await base.PopulateViewModelForFocVan();
        }
        public override async Task PopulateViewModelForSalableWareHouse()
        {
            await base.PopulateViewModelForSalableWareHouse();
        }
        public override async Task PopulateViewModelForFocWareHouse()
        {
            await base.PopulateViewModelForFocWareHouse();
        }

        #region Business Logics  

        #endregion
        #region Database or Services Methods
        public override async Task<List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>> GetWarehouse_VanNormal_SaleableStockData()
        {
            return await GetWarehouse_VanNormal_SaleableStockDataFromAPIAsync();
        }
        public override async Task<List<Winit.Modules.Org.Model.Interfaces.IWarehouseFOCStockItemView>> GetWarehouse_VanFocStockData()
        {
            return await GetWarehouse_VanFocStockDataFromAPIAsync();
        }
        public override async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetWarehouseDropdownDataDD(string OrgTypeUID)
        {
            return await GetWarehouseDropdownDataFromAPIAsync(OrgTypeUID);
        }
        public override async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetSubWarehouseDropdownDataDD(string OrgTypeUID, string wareHouseDDSelectedItem, string branchUID)
        {
            return await GetSubWarehouseDropdownDataFromAPIAsync(OrgTypeUID, wareHouseDDSelectedItem, branchUID);
        }
        public override async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetVanDropdownDataDD(string OrgTypeUID)
        {
            return await GetVanDropdownDataFromAPIAsync(OrgTypeUID);
        }
        public override async Task<List<Winit.Modules.Org.Model.Interfaces.IWareHouseStock>> GridDataForMaintainWareHouseStock(List<string> organisationalDDSelectedItems, string wareHouseDDSelectedItem, List<string> subWareHouseDDSelectedItems)
        {
            return await GridDataForMaintainWareHouseStockFromAPI(organisationalDDSelectedItems, wareHouseDDSelectedItem, subWareHouseDDSelectedItems);
        }
        public override async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> GetWarehouseFilterDropdownDataDD(string starRating)
        {
            return await GetWarehouseFilterDropdownDataDDFromApiAsync(starRating);
        }



        #endregion
        #region Api Calling Methods
        private async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetWarehouseDropdownDataFromAPIAsync(string orgTypeUID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                //pagingRequest.PageNumber = PageNumber;
                // pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.SortCriterias = new List<SortCriteria>
                {
                    new SortCriteria("ModifiedTime",SortDirection.Desc)
                };
                pagingRequest.IsCountRequired = true;
                ApiResponse<IEnumerable<Winit.Modules.Org.Model.Classes.Org>> apiResponse =
                     await _apiService.FetchDataAsync<IEnumerable<Winit.Modules.Org.Model.Classes.Org>>(
                     $"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID?OrgTypeUID={orgTypeUID}",
                     HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.OrderBy(o => o.Name).ToHashSet<IOrg>().ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return null;
        }

        private async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetSubWarehouseDropdownDataFromAPIAsync(string orgTypeUID, string wareHouseDDSelectedItem, string branchUID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                //pagingRequest.PageNumber = PageNumber;
                // pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.SortCriterias = new List<SortCriteria>
                {
                    new SortCriteria("ModifiedTime",SortDirection.Desc)
                };
                pagingRequest.IsCountRequired = true;
                ApiResponse<IEnumerable<Winit.Modules.Org.Model.Classes.Org>> apiResponse =
                     await _apiService.FetchDataAsync<IEnumerable<Winit.Modules.Org.Model.Classes.Org>>(
                     $"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID?OrgTypeUID={orgTypeUID}&parentUID={wareHouseDDSelectedItem}&branchUID={branchUID}",
                     HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.OrderBy(o => o.Name).ToHashSet<IOrg>().ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        //private async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetSubWarehouseDropdownDataFromAPIAsync(string orgTypeUID, string wareHouseDDSelectedItem, string branchUID)
        //{
        //    try
        //    {
        //        PagingRequest pagingRequest = new PagingRequest
        //        {
        //            FilterCriterias = new List<FilterCriteria>(),
        //            SortCriterias = new List<SortCriteria>
        //    {
        //        new SortCriteria("ModifiedTime", SortDirection.Desc)
        //    },
        //            IsCountRequired = true
        //        };

        //        string queryUrl = $"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID" +
        //                $"?OrgTypeUID={Uri.EscapeDataString(orgTypeUID ?? string.Empty)}" +
        //                $"&parentUID={Uri.EscapeDataString(wareHouseDDSelectedItem ?? string.Empty)}" +
        //                $"&branchUID={Uri.EscapeDataString(branchUID ?? string.Empty)}";

        //        ApiResponse<IEnumerable<Winit.Modules.Org.Model.Classes.Org>> apiResponse =
        //            await _apiService.FetchDataAsync<IEnumerable<Winit.Modules.Org.Model.Classes.Org>>(
        //                queryUrl, HttpMethod.Get);

        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        //        {
        //            return apiResponse.Data.OrderBy(o => o.Name).ToHashSet<IOrg>().ToList();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return null;
        //}


        private async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetVanDropdownDataFromAPIAsync(string orgTypeUID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 1;
                pagingRequest.PageSize = int.MaxValue;
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.IsCountRequired = true;
                ApiResponse<IEnumerable<Winit.Modules.Org.Model.Classes.Org>> apiResponse =
                      await _apiService.FetchDataAsync<IEnumerable<Winit.Modules.Org.Model.Classes.Org>>(
                      $"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID?OrgTypeUID={orgTypeUID}",
                      HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToHashSet<IOrg>().ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        public async Task<List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>> GetWarehouse_VanNormal_SaleableStockDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumberSalableWarehouse_Van;
                pagingRequest.PageSize = PageSizeSalableWarehouse_Van;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.SortCriterias = this.SortCriterias;
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Org/GetWarehouseStockDetails?WarehouseUID={selectedwarehouseUID}&StockType=Salable&FranchiseeOrgUID={FranchiseeOrgUID}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Org.Model.Classes.WarehouseStockItemView>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Org.Model.Classes.WarehouseStockItemView>>>(apiResponse.Data);
                    TotalItemsCountSalableWarehouse_Van = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>().ToList();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        public async Task<List<Winit.Modules.Org.Model.Interfaces.IWarehouseFOCStockItemView>> GetWarehouse_VanFocStockDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumberFocWarehouse_Van;
                pagingRequest.PageSize = PageSizeFocWarehouse_Van;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Org/GetWarehouseStockDetails?WarehouseUID={selectedwarehouseUID}&StockType=FOCStock&FranchiseeOrgUID={FranchiseeOrgUID}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Org.Model.Classes.WarehouseFOCStockItemView>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Org.Model.Classes.WarehouseFOCStockItemView>>>(apiResponse.Data);
                    TotalItemsCountFocWarehouse_Van = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Org.Model.Interfaces.IWarehouseFOCStockItemView>().ToList();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }

        private async Task<List<IWareHouseStock>> GridDataForMaintainWareHouseStockFromAPI(List<string> organisationalDDSelectedItems, string wareHouseDDSelectedItem,
                                                                                            List<string> subWareHouseDDSelectedItems)
        {
            try
            {

                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.IsCountRequired = true;
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.SortCriterias = this.SortCriterias;
                FilterCriteria? filterCriteria = FilterCriterias.Find(e => e.Name == "OUCode");
                if (filterCriteria != null)
                {
                    FilterCriterias.Remove(filterCriteria);
                }
                FilterCriteria? filterCriterias = FilterCriterias.Find(e => e.Name == "WarehouseCode");
                if (filterCriterias != null)
                {
                    FilterCriterias.Remove(filterCriterias);
                }
                if (subWareHouseDDSelectedItems.Any())
                {
                    pagingRequest.FilterCriterias.Add(new FilterCriteria("SubWarehouseCode", subWareHouseDDSelectedItems.ToArray(), FilterType.In));
                }
                else
                {
                    FilterCriteria? filterCriteriaWarehouseCode = FilterCriterias.Find(e => e.Name == "WarehouseCode");
                    if (filterCriteriaWarehouseCode != null)
                    {
                        FilterCriterias.Remove(filterCriteriaWarehouseCode);
                    }
                }
                pagingRequest.FilterCriterias.Add(new FilterCriteria("WarehouseCode", wareHouseDDSelectedItem, FilterType.Equal));
                pagingRequest.FilterCriterias.Add(new FilterCriteria("OUCode", organisationalDDSelectedItems.ToArray(), FilterType.In));
                if (FilterCriterias.Count > 0)
                {
                    pagingRequest.FilterCriterias = FilterCriterias;
                }
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Org/GetAllWareHouseStock",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Org.Model.Classes.WareHouseStock>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Org.Model.Classes.WareHouseStock>>>(apiResponse.Data);
                    TotalItemsCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Org.Model.Interfaces.IWareHouseStock>().ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return null;
        }


        private async Task<List<ISKUGroup>> GetWarehouseFilterDropdownDataDDFromApiAsync(string skuGroupTypeUid)
        {
            try
            {
                ApiResponse<IEnumerable<Winit.Modules.SKU.Model.Classes.SKUGroup>> apiResponse =
                      await _apiService.FetchDataAsync<IEnumerable<Winit.Modules.SKU.Model.Classes.SKUGroup>>(
                      $"{_appConfigs.ApiBaseUrl}Org/GetSkuGroupBySkuGroupTypeUID?skuGroupTypeUid={skuGroupTypeUid}",
                      HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToHashSet<ISKUGroup>().ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        #endregion
    }
}
