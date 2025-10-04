using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.CustomSKUField.Model.Classes;
using Winit.Modules.CustomSKUField.Model.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.UOM.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes
{
    public class AddEditMaintainSKUWebViewModel : AddEditMaintainSKUBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private List<string> _propertiesToSearch = new List<string>();
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        private readonly IDataManager _dataManager;
        private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public AddEditMaintainSKUWebViewModel(IServiceProvider serviceProvider,
              IFilterHelper filter,
              ISortHelper sorter,
              IListHelper listHelper,
              IAppUser appUser,
              IAppSetting appSetting,
              IDataManager dataManager,
              IAppConfig appConfigs,
              Base.BL.ApiService apiService
            ) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            _appConfigs = appConfigs;
            _apiService = apiService;
        }
        public override async Task PopulateViewModel()
        {
            await base.PopulateViewModel();
            SupplierOrg = await GetSupplierDropdownDataFromAPIAsync();
            if (SupplierOrg != null && SupplierOrg.Any())
            {
                ORGSupplierSelectionItems.Clear();
                ORGSupplierSelectionItems.AddRange(ConvertOrgToSelectionItem(SupplierOrg));
            }
        }
        //SupplierDropdown
        private List<ISelectionItem> ConvertOrgToSelectionItem(IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg> orgSupplier)
        {
            List<ISelectionItem> selectionItems = new List<ISelectionItem>();
            foreach (var org in orgSupplier)
            {
                SelectionItem si = new SelectionItem();
                // si.Code = org.Code;
                si.Label = org.Name;
                si.UID = org.UID;
                selectionItems.Add(si);
            }
            return selectionItems;
        }
        #region Business Logics  
        #endregion
        #region Database or Services Methods
        public override async Task<List<Winit.Modules.SKU.Model.Classes.SKUGroupView>> GetSKUGroupAttributeData()
        {
            return await GetSKUGroupAttributeDataFromAPIAsync();
        }
        public override async Task<List<Winit.Modules.SKU.Model.Classes.SKUGroupTypeView>> GetSKUGroupTypeAttributeData()
        {
            return await GetSKUGroupTypeAttributeDataFromAPIAsync();
        }
        public override async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetDistributionMappingData()
        {
            return await GetDistributionMappingDataFromAPIAsync();
        }
        public override async Task<List<Winit.Modules.UOM.Model.Interfaces.IUOMType>> GetSKUUOMData()
        {
            return await GetSKUUOMDataFromAPIAsync();
        }
        public override async Task<List<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetSKUUOMVolumeUnitData(string code)
        {
            return await GetSKUUOMVolume_GrossUnitDataFromAPIAsync(code);
        }
        public override async Task<List<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetSKUUOMGrossWeightUnitData(string code)
        {
            return await GetSKUUOMVolume_GrossUnitDataFromAPIAsync(code);
        }
        public override async Task<List<Winit.Modules.CustomSKUField.Model.Classes.CustomField>> GetCustomSkuFieldsDynamicData()
        {
            return await GetCustomSkuFieldsDynamicDataFromAPIAsync();
        }
        public override async Task<bool> CreateUpdateCustomSKuField(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField)
        {
            return await CreateUpdateCustomSKuFieldFromAPIAsync(customSKUField);
        }
        public override async Task<bool> CreateUpdateSKUAttributeData(Winit.Modules.SKU.Model.Interfaces.ISKUAttributes skuattributes, bool IsCreate)
        {
            return await CreateUpdateSKUAttributeDataFromAPIAsync(skuattributes, IsCreate);
        }
        public override async Task<bool> CreateUpdateSKUData(Winit.Modules.SKU.Model.Interfaces.ISKU sku, bool IsCreate)
        {
            return await CreateUpdateSKUDataFromAPIAsync(sku, IsCreate);
        }
        public override async Task<bool> CreateUpdateSKUUOMData(Winit.Modules.SKU.Model.Interfaces.ISKUUOM skuuom, bool IsCreate)
        {
            return await CreateUpdateSKUUOMDataFromAPIAsync(skuuom, IsCreate);
        }
        public override async Task<bool> SaveDistributionData(Winit.Modules.SKU.Model.Interfaces.ISKUConfig skuconfig, bool IsCreate)
        {
            return await SaveDistributionDataFromAPIAsync(skuconfig, IsCreate);
        }
        public override async Task<Winit.Modules.SKU.Model.Interfaces.ISKUMaster> GetSKUDetailsData(string orgUID)
        {
            return await GetSKUDetailsDataFromAPIAsync(orgUID);
        }
        public override async Task<List<CommonUIDResponse>> CreateUpdateFileSysData(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSys, bool IsCreate)
        {
            return await SaveFileSysDataFromAPIAsync(fileSys, IsCreate);
        }
        protected async override Task<List<SKUAttributeDropdownModel>> GetSKUAttribute_Data()
        {
            try
            {

                ApiResponse<List<SKUAttributeDropdownModel>> apiResponse =
                   await _apiService.FetchDataAsync<List<SKUAttributeDropdownModel>>(
                   $"{_appConfigs.ApiBaseUrl}SKUAttributes/GetSKUGroupTypeForSKuAttribute",
                   HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return new();
        }
        protected async override Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string? skuGroupTypeUID, string? parentUID)
        {
            try
            {
                ApiResponse<List<SKUGroupSelectionItem>> apiResponse =
                   await _apiService.FetchDataAsync<List<SKUGroupSelectionItem>>(
                   $"{_appConfigs.ApiBaseUrl}SKUGroup/GetSKUGroupSelectionItemBySKUGroupTypeUID?skuGroupTypeUID={skuGroupTypeUID}&parentUID={parentUID}",
                   HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return new();
        }
        protected async override Task<bool> CUDSKUAttributes_API(List<ISKUAttributes> sKUAttributes)
        {
            try
            {
                ApiResponse<string> apiResponse =
                   await _apiService.FetchDataAsync(
                   $"{_appConfigs.ApiBaseUrl}SKUAttributes/CUDBulkSKUAttributes",
                   HttpMethod.Post, sKUAttributes);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        #endregion
        #region Api Calling Methods
        private async Task<List<CommonUIDResponse>> SaveFileSysDataFromAPIAsync(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> skufilesys, bool IsCreate)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(skufilesys);
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    apiResponse = await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}FileSys/CreateFileSysForBulk", HttpMethod.Post, skufilesys);
                }
                else
                {
                    apiResponse = await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}FileSys/CreateFileSysForBulk", HttpMethod.Put, skufilesys);
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    // Assuming CommonUIDResponse is a class with a property 'UID'
                    List<CommonUIDResponse> commonUIDResponses = JsonConvert.DeserializeObject<List<CommonUIDResponse>>(data);
                    return commonUIDResponses;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null; // Or handle the case when no response is received
        }

        private async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetSupplierDropdownDataFromAPIAsync()
        {
            try
            {

                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                // pagingRequest.FilterCriterias.Add(new FilterCriteria("OrgTypeUID", "Supplier", FilterType.Equal));

                ApiResponse<IEnumerable<Winit.Modules.Org.Model.Classes.Org>> apiResponse =
                   await _apiService.FetchDataAsync<IEnumerable<Winit.Modules.Org.Model.Classes.Org>>(
                   $"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID?OrgTypeUID=Supplier",
                   HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        private async Task<bool> CreateUpdateSKUDataFromAPIAsync(Winit.Modules.SKU.Model.Interfaces.ISKU sku, bool IsCreate)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(sku);
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    AddCreateFields(sku, true);
                    //ISKUV1 sku1 = sku as ISKUV1;
                    //sku1.HSNCode = "HSNCode1";
                    //remove later
                    Type type = typeof(SKUV1);
                    type.GetProperty("HSNCode").SetValue(sku, "HSNCode");
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}SKU/CreateSKU", HttpMethod.Post, sku);
                }
                else
                {
                    AddUpdateFields(sku);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}SKU/UpdateSKU", HttpMethod.Put, sku);
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<List<Winit.Modules.SKU.Model.Classes.SKUGroupView>> GetSKUGroupAttributeDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = new List<FilterCriteria>();

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SKUGroup/SelectSKUGroupView",
                    HttpMethod.Get, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    List<Winit.Modules.SKU.Model.Classes.SKUGroupView> selectionORGs = JsonConvert.DeserializeObject<List<Winit.Modules.SKU.Model.Classes.SKUGroupView>>(data);
                    if (selectionORGs != null)
                    {
                        return selectionORGs.OfType<SKUGroupView>().ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        private async Task<List<Winit.Modules.SKU.Model.Classes.SKUGroupTypeView>> GetSKUGroupTypeAttributeDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = new List<FilterCriteria>();

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SKUGroupType/SelectSKUGroupTypeView",
                    HttpMethod.Get, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    List<Winit.Modules.SKU.Model.Classes.SKUGroupTypeView> selectionORGs = JsonConvert.DeserializeObject<List<Winit.Modules.SKU.Model.Classes.SKUGroupTypeView>>(data);
                    if (selectionORGs != null)
                    {
                        return selectionORGs.OfType<SKUGroupTypeView>().ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        private async Task<bool> CreateUpdateSKUAttributeDataFromAPIAsync(Winit.Modules.SKU.Model.Interfaces.ISKUAttributes skuattributes, bool IsCreate)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(skuattributes);
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    AddCreateFields(skuattributes, true);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}SKUAttributes/CreateSKUAttributes", HttpMethod.Post, skuattributes);
                }
                else
                {
                    AddUpdateFields(skuattributes);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}SKUAttributes/UpdateSKUAttributes", HttpMethod.Put, skuattributes);
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<List<Winit.Modules.UOM.Model.Interfaces.IUOMType>> GetSKUUOMDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}UOMType/SelectAllUOMTypeDetails",
                    HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.UOM.Model.Classes.UOMType> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.UOM.Model.Classes.UOMType>>(data);
                    if (selectionORGs.PagedData != null)
                    {
                        return selectionORGs.PagedData.OfType<IUOMType>().ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        private async Task<bool> CreateUpdateSKUUOMDataFromAPIAsync(Winit.Modules.SKU.Model.Interfaces.ISKUUOM skuuom, bool IsCreate)
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    AddCreateFields(skuuom, true);
                    string jsonBody = JsonConvert.SerializeObject(skuuom);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}SKUUOM/CreateSKUUOM", HttpMethod.Post, skuuom);
                }
                else
                {
                    AddUpdateFields(skuuom);
                    string jsonBody = JsonConvert.SerializeObject(skuuom);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}SKUUOM/UpdateSKUUOM", HttpMethod.Put, skuuom);
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<bool> SaveDistributionDataFromAPIAsync(Winit.Modules.SKU.Model.Interfaces.ISKUConfig skuconfig, bool IsCreate)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(skuconfig);
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    AddCreateFields(skuconfig, true);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}SKUConfig/CreateSKUConfig", HttpMethod.Post, skuconfig);
                }
                else
                {
                    AddUpdateFields(skuconfig);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}SKUConfig/UpdateSKUConfig", HttpMethod.Put, skuconfig);
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<List<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetSKUUOMVolume_GrossUnitDataFromAPIAsync(string code)
        {
            try
            {
                Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest = new Winit.Modules.ListHeader.Model.Classes.ListItemRequest();
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;
                listItemRequest.Codes = new List<string>() { code };
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                    HttpMethod.Post, listItemRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.ListHeader.Model.Interfaces.IListItem>().ToList();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }
            return null;
        }
        private async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetDistributionMappingDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                // pagingRequest.FilterCriterias.Add(new FilterCriteria("OrgTypeUID", "Supplier", FilterType.Equal));

                ApiResponse<IEnumerable<Winit.Modules.Org.Model.Classes.Org>> apiResponse =
                   await _apiService.FetchDataAsync<IEnumerable<Winit.Modules.Org.Model.Classes.Org>>(
                   $"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID?OrgTypeUID=DC",
                   HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }
            return null;
        }
        //private async Task<bool> CreateUpdateCustomfieldsDataFromAPIAsync(Winit.Modules.SKU.Model.Interfaces.ICustomSKUFields SKUCUSTOM, bool IsCreate)
        //{
        //    try
        //    {
        //        string jsonBody = JsonConvert.SerializeObject(SKUCUSTOM);
        //        ApiResponse<string> apiResponse = null;
        //        if (IsCreate)
        //        {
        //            AddCreateFields(SKUCUSTOM, true);
        //            apiResponse = await _apiService.FetchDataAsync(
        //    $"{_appConfigs.ApiBaseUrl}SKU/CreateCustomSKUField", HttpMethod.Post, SKUCUSTOM);
        //        }
        //        else
        //        {
        //            AddUpdateFields(SKUCUSTOM);
        //            apiResponse = await _apiService.FetchDataAsync(
        //    $"{_appConfigs.ApiBaseUrl}SKU/UpdateCustomSKUField", HttpMethod.Put, SKUCUSTOM);
        //        }

        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        //        {
        //            string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
        //            if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return false;
        //}
        private async Task<Winit.Modules.SKU.Model.Interfaces.ISKUMaster> GetSKUDetailsDataFromAPIAsync(string orgUID)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SKU/SelectSKUMasterByUID?UID={orgUID}",
                    HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>>(apiResponse.Data);
                    if (pagedResponse != null && pagedResponse.IsSuccess)
                    {
                        var sKUMasterData = pagedResponse.Data;
                        ISKUMaster sKUMaster = new SKUMaster();
                        sKUMaster.SKU = sKUMasterData.SKU;
                        sKUMaster.SKUAttributes = sKUMasterData.SKUAttributes.OfType<ISKUAttributes>().ToList();
                        sKUMaster.SKUUOMs = sKUMasterData.SKUUOMs.OfType<ISKUUOM>().ToList();
                        sKUMaster.ApplicableTaxUIDs = sKUMasterData.ApplicableTaxUIDs;
                        sKUMaster.SKUConfigs = sKUMasterData.SKUConfigs.OfType<ISKUConfig>().ToList();
                        sKUMaster.FileSysList = sKUMasterData.FileSysList.OfType<IFileSys>().ToList();
                        sKUMaster.CustomSKUFields = sKUMasterData.CustomSKUFields.OfType<ICustomSKUFields>().ToList();
                        List<CustomField> dbData = null;
                        foreach (CustomSKUFields custom in sKUMaster.CustomSKUFields)
                        {
                            dbData = JsonConvert.DeserializeObject<List<CustomField>>(custom.CustomField);
                        }
                        if (dbData != null)
                        {
                            sKUMaster.DbDataList = dbData;
                        }
                        return sKUMaster;
                        //sKUMaster.customSKUFields = sKUMasterData.customSKUFields?.OfType<ICustomSKUFields>().ToList();

                        //List<CustomField> dbData = new List<CustomField>(); // Initialize with an empty list

                        //if (sKUMaster.customSKUFields != null)
                        //{
                        //    foreach (CustomSKUFields custom in sKUMaster.customSKUFields)
                        //    {
                        //        if (!string.IsNullOrEmpty(custom.CustomField))
                        //        {
                        //            List<CustomField> deserializedData = JsonConvert.DeserializeObject<List<CustomField>>(custom.CustomField);
                        //            if (deserializedData != null)
                        //            {
                        //                dbData.AddRange(deserializedData);
                        //            }
                        //        }
                        //    }
                        //}

                        sKUMaster.DbDataList = dbData;
                        return sKUMaster;

                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
            return null;
        }
        private async Task<List<Winit.Modules.CustomSKUField.Model.Classes.CustomField>> GetCustomSkuFieldsDynamicDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 1;
                pagingRequest.PageSize = int.MaxValue;
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.IsCountRequired = true;
                ApiResponse<List<Winit.Modules.CustomSKUField.Model.Classes.CustomField>> apiResponse =
                    await _apiService.FetchDataAsync<List<Winit.Modules.CustomSKUField.Model.Classes.CustomField>>(
                    $"{_appConfigs.ApiBaseUrl}CustomSKUField/SelectAllCustomFieldsDetails",
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
        private async Task<bool> CreateUpdateCustomSKuFieldFromAPIAsync(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField)
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                AddCreateFields(customSKUField, false);
                string jsonBody = JsonConvert.SerializeObject(customSKUField);
                apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}CustomSKUField/CUDCustomSKUFields", HttpMethod.Post, customSKUField);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        #endregion
    }
}
