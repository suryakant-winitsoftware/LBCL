using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Classes
{
    public class AddEditMaintainWarehouseWebViewModel : AddEditMaintainWarehouseBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public AddEditMaintainWarehouseWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
             IAppUser appUser,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
          ) : base(serviceProvider, filter, sorter, appUser, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;

        }
        public override async Task PopulateViewModel()
        {
            await base.PopulateViewModel();
            if (WareHouseTypeDDList != null && WareHouseTypeDDList.Any())
            {
                WareHouseTypeSelectionItems.Clear();
                WareHouseTypeSelectionItems.AddRange(ConvertOrgToSelectionItem(WareHouseTypeDDList));
            }
        }
        #region Business Logics 
        private List<ISelectionItem> ConvertOrgToSelectionItem(List<Winit.Modules.Org.Model.Interfaces.IOrgType> warehouseItemView)
        {
            List<ISelectionItem> selectionItems = new List<ISelectionItem>();
            foreach (var org in warehouseItemView)
            {
                SelectionItem si = new SelectionItem();
                // si.Code = org.Code;
                si.Label = org.WarehouseType;
                si.UID = org.UID;
                selectionItems.Add(si);
            }
            return selectionItems;
        }
        private void AddCreateFields(IBaseModel baseModel, bool IsUIDRequired)
        {

            baseModel.CreatedBy = _appUser.Emp.UID;
            baseModel.ModifiedBy = _appUser.Emp.UID;
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
        }
        private void AddUpdateFields(IBaseModel baseModel)
        {
            baseModel.ModifiedBy = _appUser.Emp.UID;
            baseModel.ModifiedTime = DateTime.Now;
        }
        #endregion
        #region Database or Services Methods
        public override async Task<Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView> GetMaintainWarehouseEditDetails(string warehouseuid)
        {
            return await GetMaintainWarehouseEditDetailsDataFromAPIAsync(warehouseuid);
        }
        public override async Task<List<Winit.Modules.Org.Model.Interfaces.IOrgType>> GetWarehouseTypeDropdown()
        {
            return await GetWarehouseTypeDropdownDataFromAPIAsync();
        }
        public override async Task<List<IOrgType>> GetWarehouseTypeDropdownByUser(string oRG_type)
        {
            return await GetWarehouseTypeDropdownByUserFromAPIAsync(oRG_type);
        }

        

        public override async Task<bool> CreateUpdateWareHouseData(Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView warehouseItem, bool IsCreate)
        {
            return await CreateUpdateWareHouseDataFromAPIAsync(warehouseItem, IsCreate);
        }
        #endregion
        #region Api Calling Methods
        private async Task<Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView> GetMaintainWarehouseEditDetailsDataFromAPIAsync(string warehouseuid)
        {
            try
            {
                ApiResponse<Winit.Modules.Org.Model.Classes.EditWareHouseItemView> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Org.Model.Classes.EditWareHouseItemView>(
                    $"{_appConfigs.ApiBaseUrl}Org/ViewFranchiseeWarehouseByUID?UID={warehouseuid}",
                    HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<bool> CreateUpdateWareHouseDataFromAPIAsync(Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView warehouseItem, bool IsCreate)
        {
            try
            {
                ApiResponse<string>? apiResponse = null;
                if (IsCreate)
                {
                    AddCreateFields(warehouseItem, false);
                    warehouseItem.UID = $"{_appUser.SelectedJobPosition.OrgUID}_{warehouseItem.WarehouseCode}";
                    warehouseItem.LinkedItemUID = warehouseItem.UID;
                    string jsonBody = JsonConvert.SerializeObject(warehouseItem);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Org/CreateViewFranchiseeWarehouse", HttpMethod.Post, warehouseItem);
                }
                else
                {
                    AddUpdateFields(warehouseItem);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Org/UpdateViewFranchiseeWarehouse", HttpMethod.Put, warehouseItem);
                }

                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<List<Winit.Modules.Org.Model.Interfaces.IOrgType>> GetWarehouseTypeDropdownDataFromAPIAsync()
        {            
            //try
            //{
            //    PagingRequest pagingRequest = new PagingRequest();
            //    pagingRequest.PageNumber = 1;
            //    pagingRequest.PageSize = int.MaxValue;
            //    pagingRequest.FilterCriterias = new List<FilterCriteria>();
            //    pagingRequest.IsCountRequired = true;
            //    pagingRequest.FilterCriterias.Add(new FilterCriteria("UID", "FRWH", FilterType.NotEqual));
            //    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            //        $"{_appConfigs.ApiBaseUrl}Org/GetOrgTypeDetails",
            //        HttpMethod.Post, pagingRequest);
            //    if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            //    {
            //        string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
            //        PagedResponse<Winit.Modules.Org.Model.Classes.OrgType> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Org.Model.Classes.OrgType>>(data);
            //        if (selectionORGs.PagedData != null)
            //        {
            //            return selectionORGs.PagedData.OfType<IOrgType>().ToList();
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
            //return null;

            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 1;
                pagingRequest.PageSize = int.MaxValue;
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.IsCountRequired = true;
                pagingRequest.FilterCriterias.Add(new FilterCriteria("UID", "FRWH", FilterType.NotEqual));

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Org/GetOrgTypeDetails",
                    HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && !string.IsNullOrEmpty(apiResponse.Data))
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (!string.IsNullOrEmpty(data))
                    {
                        var selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Org.Model.Classes.OrgType>>(data);
                        if (selectionORGs?.PagedData != null)
                        {
                            return selectionORGs.PagedData.OfType<IOrgType>().ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                Console.WriteLine($"Error in GetWarehouseTypeDropdownDataFromAPIAsync: {ex.Message}");
            }

            // Return empty list instead of null
            return new List<IOrgType>();
        }
        private async Task<List<IOrgType>> GetWarehouseTypeDropdownByUserFromAPIAsync(string oRG_type)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 1;
                pagingRequest.PageSize = int.MaxValue;
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.IsCountRequired = true;
                if (_appUser.Emp.Code == "ADMIN")
                {
                    pagingRequest.FilterCriterias.Add(new FilterCriteria("UID", "WH", FilterType.Equal));
                    //string ORG_type = "WH";
                    //WareHouseTypeDDList = await GetWarehouseTypeDropdownByUser(ORG_type);
                }
                else
                {
                    //string ORG_type = "";
                    //WareHouseTypeDDList = await GetWarehouseTypeDropdownByUser(ORG_type);
                    pagingRequest.FilterCriterias.Add(new FilterCriteria("UID", "WH", FilterType.NotEqual));
                }
                //pagingRequest.FilterCriterias.Add(new FilterCriteria("UID", "FRWH", FilterType.Equal));
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Org/GetWarehouseTypeDropdownByUser?ORG_type={oRG_type}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.Org.Model.Classes.OrgType> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Org.Model.Classes.OrgType>>(data);
                    if (selectionORGs.PagedData != null)
                    {
                        return selectionORGs.PagedData.OfType<IOrgType>().ToList();
                    }
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
