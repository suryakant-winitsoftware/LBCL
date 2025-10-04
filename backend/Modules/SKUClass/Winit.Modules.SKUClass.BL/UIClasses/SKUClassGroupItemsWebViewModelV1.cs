using Newtonsoft.Json;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Mapping.Model.Classes;
using Winit.Modules.Mapping.Model.Constants;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKUClass.Model.Classes;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.BL.UIClasses;

public class SKUClassGroupItemsWebViewModelV1 : SKUClassGroupItemsBaseViewModelV1
{
    private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private readonly Winit.Modules.Base.BL.ApiService _apiService;
    public SKUClassGroupItemsWebViewModelV1(IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        IListHelper listHelper,
        IAppUser appUser,
        IAppSetting appSetting,
        Shared.Models.Common.IAppConfig appConfigs,
        Base.BL.ApiService apiService,
        IAddProductPopUpDataHelper addProductPopUpDataHelper) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, addProductPopUpDataHelper)
    {
        _appConfigs = appConfigs;
        _apiService = apiService;
    }



    protected override async Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupMaster?> GetSKUClassGroupMaster(string skuClassGroupUID)
    {
        try
        {
            ApiResponse<SKUClassGroupDTO> apiResponse =
                await _apiService.FetchDataAsync<Winit.Modules.SKUClass.Model.Classes.SKUClassGroupDTO>(
                $"{_appConfigs.ApiBaseUrl}SKUClassGroup/GetSKUClassGroupMaster?sKUClassGroupUID={skuClassGroupUID}",
                HttpMethod.Post);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ISKUClassGroupMaster sKUClassGroupMaster = new SKUClassGroupMaster();
                sKUClassGroupMaster.SKUClassGroup = apiResponse.Data.SKUClassGroup;
                if (apiResponse.Data.SKUClassGroupItems is not null)
                    sKUClassGroupMaster.SKUClassGroupItems = apiResponse.Data.SKUClassGroupItems.OfType<Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView>().ToList();
                return sKUClassGroupMaster;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return default;
    }
    protected async override Task<List<IOrg>> GetOrgs(List<FilterCriteria> filterCriterias)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = filterCriterias;
            ApiResponse<PagedResponse<Winit.Modules.Org.Model.Classes.Org>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Org.Model.Classes.Org>>(
                $"{_appConfigs.ApiBaseUrl}Org/GetOrgDetails",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data.PagedData.OfType<Winit.Modules.Org.Model.Interfaces.IOrg>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new();
    }

    protected async override Task<List<ISKUMaster>?> GetSKUMasters(List<string> orgs)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.PageSize = 10;
            pagingRequest.PageNumber = 1;
            ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>> apiResponse = await _apiService.FetchDataAsync
                <PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>>($"{_appConfigs.ApiBaseUrl}SKU/GetAllSKUMasterData", HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                List<ISKUMaster> sKUMastersFromApi = new List<ISKUMaster>();
                foreach (var skumaster in apiResponse.Data.PagedData.ToList())
                {
                    if (skumaster != null)
                    {
                        sKUMastersFromApi.Add(new SKUMaster()
                        {
                            SKU = skumaster.SKU,
                            SKUAttributes = (skumaster.SKUAttributes != null) ? skumaster.SKUAttributes.Cast<ISKUAttributes>().ToList() : new(),
                            SKUUOMs = skumaster.SKUUOMs != null ? skumaster.SKUUOMs.Cast<ISKUUOM>().ToList() : new(),
                            ApplicableTaxUIDs = skumaster.ApplicableTaxUIDs,
                            SKUConfigs = skumaster.SKUConfigs != null ? skumaster.SKUConfigs.OfType<ISKUConfig>().ToList() : new(),
                        });
                        ;
                    }
                }
                return sKUMastersFromApi;
            }
            return default;
        }
        catch (Exception)
        {
            throw;
        }
    }
    protected override async Task<ApiResponse<string>> CUD_SKUClassGroupMaster(ISKUClassGroupMaster sKUClassGroupMaster)
    {
        try
        {

            ApiResponse<string> apiResponse =
                await _apiService.FetchDataAsync<string>(
                $"{_appConfigs.ApiBaseUrl}SKUClassGroup/CUD_SKUClassGroupMaster",
                HttpMethod.Post, sKUClassGroupMaster);

            return apiResponse;

        }
        catch (Exception)
        {
            throw;
        }
    }
    protected override async Task GetBroadClassificationHeaderDetails(PagingRequest pagingRequest)
    {
        ApiResponse<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>> apiResponse =
            await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>>
            ($"{_appConfigs.ApiBaseUrl}BroadClassificationHeader/GetBroadClassificationHeaderDetails",
            HttpMethod.Post, pagingRequest);
        if (apiResponse != null && apiResponse.IsSuccess & apiResponse.Data != null && apiResponse.Data?.PagedData != null)
        {
            BroadClassificationSelectionItems.Clear();
            foreach (var item in apiResponse.Data.PagedData)
            {
                ISelectionItem selectionItem = new SelectionItem()
                {
                    UID = item.UID,
                    Code = item.Name,
                    Label = item.Name,
                };
                BroadClassificationSelectionItems.Add(selectionItem);
            }
        }
    }
    public override async Task GetAllChannelPartner()
    {
        Winit.Shared.Models.Common.ApiResponse<List<Winit.Modules.Store.Model.Interfaces.IStore>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Interfaces.IStore>>(
        $"{_appConfigs.ApiBaseUrl}Store/GetChannelPartner?jobPositionUid={_appUser.SelectedJobPosition.UID}",
        HttpMethod.Get);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        {
            ChannelPartners.Clear();
            foreach (var s in apiResponse.Data)
            {
                ISelectionItem selectionItem = new SelectionItem()
                {
                    UID = s.UID,
                    Code = s.Code,
                    Label = $"[{s.Code}] {s.Name}",
                };
                ChannelPartners.Add(selectionItem);
            }
        }
    }
    public async Task<List<IStore>> GetApplicableToCustomers()
    {
        if (SelectedCP.Count() > 0 || SelectedBC.Count() > 0 || SelectedBranches.Count() > 0)
        {
            ApplicableToCustomerRequestBody applicableToCustomerRequestBody = new()
            {
                Stores = SelectedCP.Select(p => p.UID).ToList(),
                BroadClassifications = SelectedBC.Select(b => b.UID).ToList(),
                Branches = SelectedBranches.Select(b => b.UID).ToList()
            };
            Winit.Shared.Models.Common.ApiResponse<List<Winit.Modules.Store.Model.Interfaces.IStore>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Interfaces.IStore>>(
            $"{_appConfigs.ApiBaseUrl}Store/GetApplicableToCustomers",
            HttpMethod.Post, applicableToCustomerRequestBody);

            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null ? apiResponse.Data : [];
        }
        return null;
    }
    protected override async Task GetBranchDetails()
    {
        Winit.Shared.Models.Common.ApiResponse<List<IBranch>> apiResponse = await _apiService.FetchDataAsync<List<IBranch>>(
        $"{_appConfigs.ApiBaseUrl}Branch/GetBranchByJobPositionUid?jobPositionUid={_appUser.SelectedJobPosition.UID}",
        HttpMethod.Get);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
        {
            foreach (IBranch b in apiResponse.Data)
            {
                BranchDdlSelectionItems.Add(new SelectionItem()
                {
                    UID = b.UID,
                    Code = b.Code,
                    Label = b.Name,
                });
            }
        }
    }
    public override async Task<ISelectionMapMaster?> GetSelectionMapMasterByLinkedItemUID(string linkedItemUID)
    {
        try
        {
            ApiResponse<SelectionMapMasterDTO> apiResponse = await _apiService.FetchDataAsync<SelectionMapMasterDTO>(
            $"{_appConfigs.ApiBaseUrl}Mapping/GetSelectionMapMasterByLinkedItemUID?linkedItemUID={linkedItemUID}",
            HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ISelectionMapMaster selectionMapMaster = new SelectionMapMaster
                {
                    SelectionMapCriteria = apiResponse.Data.SelectionMapCriteria,
                    SelectionMapDetails = apiResponse.Data?.SelectionMapDetails?.ToList<ISelectionMapDetails>(),
                };
                return selectionMapMaster;
            }
            return default;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public override async Task DataPreparation()
    {
        Task.Run(async () =>
        {

            DataPreparation_PrepareLinkedItemUIDByStore();
            DataPreparation_PrepareSkuClassGroupItems();
        }
            );
    }
    async Task DataPreparation_PrepareLinkedItemUIDByStore()
    {
        PrepareLinkedItemUIDModel prepareLinkedItemUIDModel = new PrepareLinkedItemUIDModel()
        {
            LinkedItemType = GroupConstant.SKUClassGroup,
        };
        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
           $"{_appConfigs.ApiBaseUrl}DataPreparation/PrepareLinkedItemUIDByStore",
           HttpMethod.Post, prepareLinkedItemUIDModel);

    }
    async Task DataPreparation_PrepareSkuClassGroupItems()
    {
        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
           $"{_appConfigs.ApiBaseUrl}DataPreparation/PrepareSkuClassGroupItems",
           HttpMethod.Post);

    }
    public override async Task<bool> SaveMappings()
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
            $"{_appConfigs.ApiBaseUrl}Mapping/CUDSelectiomMapMaster",
            HttpMethod.Post, SelectionMapMaster);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
