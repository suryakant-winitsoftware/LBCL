using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Winit.Shared.Models.Enums;
using Winit.Modules.SKU.Model.Classes;
using System.Linq;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.SKU.Model.Interfaces;
using Newtonsoft.Json;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.BL.Classes;
using System.Collections;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using System.util;
using System.Net.Http;
using System.Text;
using Winit.Modules.Setting.BL.Classes;
using Microsoft.Extensions.Options;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Modules.Setting.Model.Classes;
using Dapper;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKUClass.BL.Interfaces;
using System.Threading;

namespace WINITAPI.Controllers.SKU;

[ApiController]
[Route("api/[Controller]")]
// [Authorize]
public class DataPreparationController : WINITBaseController
{
    private readonly Winit.Modules.SKU.BL.Interfaces.ISKUBL _skuBL;
    private readonly Winit.Modules.Store.BL.Interfaces.IStoreBL _storeBL;
    private readonly Winit.Modules.SKU.BL.Interfaces.ISKUPriceListBL _skuPriceListBL;
    private readonly Winit.Modules.SKU.BL.Interfaces.ISKUGroupTypeBL _sKUGroupTypeBL;
    private readonly Winit.Modules.DropDowns.BL.Interfaces.IDropDownsBL _dropDownsBL;
    Winit.Modules.Promotion.BL.Interfaces.IPromotionBL _promotionBL;
    Winit.Modules.Location.BL.Interfaces.ILocationBL _locationBL;
    Winit.Modules.Setting.BL.Interfaces.ISettingBL _settingBL;
    private readonly ISettingBL _settingBl;
    private readonly ApiSettings _apiSettings;
    IServiceProvider _serviceProvider;
    private readonly ISKUClassGroupItemsBL _iSKUClassGroupItemsBL;
    private readonly ParallelOptions _parallelOptions;
    public DataPreparationController(IServiceProvider serviceProvider,
        Winit.Modules.SKU.BL.Interfaces.ISKUBL skuBL, Winit.Modules.Store.BL.Interfaces.IStoreBL storeBL,
        Winit.Modules.SKU.BL.Interfaces.ISKUPriceListBL skuPriceListBL,
        ISKUGroupTypeBL sKUGroupTypeBL, Winit.Modules.DropDowns.BL.Interfaces.IDropDownsBL dropDownsBL,
        Winit.Modules.Promotion.BL.Interfaces.IPromotionBL promotionBL, Winit.Modules.Location.BL.Interfaces.ILocationBL locationBL, IOptions<ApiSettings> apiSettings,
        Winit.Modules.Setting.BL.Interfaces.ISettingBL settingBL, ISKUClassGroupItemsBL iSKUClassGroupItemsBL) : base(serviceProvider)
    {
        _skuBL = skuBL;
        _storeBL = storeBL;
        _serviceProvider = serviceProvider;
        _skuPriceListBL = skuPriceListBL;
        _sKUGroupTypeBL = sKUGroupTypeBL;
        _dropDownsBL = dropDownsBL;
        _promotionBL = promotionBL;
        _locationBL = locationBL;
        _settingBL = settingBL;
        _apiSettings = apiSettings.Value;
        _iSKUClassGroupItemsBL = iSKUClassGroupItemsBL;
        //it is to define the parallel execution 
        _parallelOptions= new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 2) 
        };
    }
    [HttpPost]
    [Route("PrepareSKUMaster")]
    public async Task<ActionResult<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUMaster>>> PrepareSKUMaster([FromBody] PrepareSKURequestModel prepareSKURequestModel)
    {
        try
        {
            var sKUMasterList = await _skuBL.PrepareSKUMaster(prepareSKURequestModel.OrgUIDs, prepareSKURequestModel.DistributionChannelUIDs, prepareSKURequestModel.SKUUIDs, prepareSKURequestModel.AttributeTypes);
            if (sKUMasterList == null)
            {
                return NotFound();
            }
            if (sKUMasterList != null && sKUMasterList.Count > 0)
            {
                Parallel.ForEach(sKUMasterList, _parallelOptions, sKUMaster =>
                {
                    if (sKUMaster.SKU != null)
                    {
                        _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.SKU, sKUMaster.SKU.UID, sKUMaster.SKU, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                        _cacheService.Set<string>($"{Winit.Shared.Models.Constants.CacheConstants.FilterSKUOrgUID}{sKUMaster.SKU.OrgUID}_{sKUMaster.SKU.UID}", sKUMaster.SKU.UID, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                    }
                    if (sKUMaster.SKUConfigs != null && sKUMaster.SKUConfigs.Count > 0)
                    {
                        _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.SKUConfig, sKUMaster.SKU.UID, sKUMaster.SKUConfigs, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                    }
                    if (sKUMaster.SKUUOMs != null && sKUMaster.SKUUOMs.Count > 0)
                    {
                        _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.SKUUOM, sKUMaster.SKU.UID, sKUMaster.SKUUOMs, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                    }
                    if (sKUMaster.SKUAttributes != null && sKUMaster.SKUAttributes.Count > 0)
                    {
                        _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.SKUAttributes, sKUMaster.SKU.UID, sKUMaster.SKUAttributes, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                    }
                    if (sKUMaster.ApplicableTaxUIDs != null && sKUMaster.ApplicableTaxUIDs.Count > 0)
                    {
                        _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.TaxSKUMap, sKUMaster.SKU.UID, sKUMaster.ApplicableTaxUIDs, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                    }
                    if (sKUMasterList != null && sKUMaster.SKU != null)
                    {
                        _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.SKUMaster, sKUMaster.SKU.UID, sKUMaster);
                    }
                });
            }

            return CreateOkApiResponse("Success");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to store SKU Master Details in Cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }

    [HttpPost]
    [Route("PrepareLinkedItemUIDByStore")]
    public async Task<ActionResult> PrepareLinkedItemUIDByStore([FromBody] PrepareLinkedItemUIDModel prepareLinkedItemUIDModel)
    {
        try
        {
            Dictionary<string, List<string>> storeLinkedItemUIDs = await _skuBL.GetLinkedItemUIDByStore(prepareLinkedItemUIDModel.LinkedItemType, prepareLinkedItemUIDModel.StoreUIDs);
            if (storeLinkedItemUIDs == null || storeLinkedItemUIDs.Count == 0)
            {
                return NotFound();
            }
            foreach (var kvp in storeLinkedItemUIDs)
            {
                if (kvp.Value != null && kvp.Value.Count > 0)
                {
                    _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.StoreLinkedItemUIDMapping, kvp.Key, kvp.Value, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                }
            }
            return CreateOkApiResponse("Success");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to store LinkedItemUIDByStore details in Cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }
    private List<StandardListSource> PrepareStoreOrg(List<IStoreCredit> storeCredits)
    {
        List<StandardListSource> orgList = null;
        if (storeCredits != null && storeCredits.Count > 0)
        {
            List<string> orgs = storeCredits.Select(e => e.OrgUID).Distinct().ToList();
            if (orgs != null && orgs.Count > 0)
            {
                foreach (string org in orgs)
                {
                    StandardListSource standardListSource = new StandardListSource();
                    standardListSource.SourceUID = org;
                    if (orgs.Count > 1)
                    {
                        var Data = _cacheService.HGet<Winit.Modules.Org.Model.Classes.Org>(Winit.Shared.Models.Constants.CacheConstants.Org, org);
                        if (Data != null)
                        {
                            standardListSource.SourceLabel = Data.Name;
                        }
                    }
                    orgList.Add(standardListSource);
                }
            }
        }
        return orgList;
    }
    private List<StandardListSource> PrepareStoreDistributionChannel(List<IStoreCredit> storeCredits)
    {
        List<StandardListSource> distributionChannelList = null;
        if (storeCredits != null && storeCredits.Count > 0)
        {
            List<string> distributionChannels = storeCredits.Select(e => e.DistributionChannelUID).Distinct().ToList();
            if (distributionChannels != null && distributionChannels.Count > 0)
            {
                foreach (string distributionChannel in distributionChannels)
                {
                    StandardListSource standardListSource = new StandardListSource();
                    standardListSource.SourceUID = distributionChannel;
                    // standardListSource.SourceLabel = distributionChannel;
                    if (distributionChannels.Count > 1)
                    {
                        // Cache of Org
                        //standardListSource.SourceLabel = name from cache
                        var Data = _cacheService.HGet<Winit.Modules.Org.Model.Classes.Org>(Winit.Shared.Models.Constants.CacheConstants.Org, distributionChannel);
                        if (Data != null)
                        {
                            standardListSource.SourceLabel = Data.Name;
                        }
                    }
                    distributionChannelList.Add(standardListSource);
                }
            }
        }
        return distributionChannelList;
    }

    [HttpPost]
    [Route("PrepareStoreMaster")]
    public async Task<ActionResult<ApiResponse<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreMaster>>>> PrepareStoreMaster([FromBody] List<string> storeUIDs)
    {
        try
        {
            var storeMasterList = await _storeBL.PrepareStoreMaster(storeUIDs);
            if (storeMasterList == null)
            {
                return NotFound();
            }
            Parallel.ForEach(storeMasterList, _parallelOptions, storeMaster =>
            {
                if (storeMaster.Store != null)
                {
                    _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.Store, storeMaster.Store.UID, storeMaster.Store, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                    _cacheService.Set<string>($"{Winit.Shared.Models.Constants.CacheConstants.FilterStoreType}{storeMaster.Store.Type}_{storeMaster.Store.UID}", storeMaster.Store.UID, WINITServices.Classes.CacheHandler.ExpirationType.Absolute,
                    -1);
                }
                if (storeMaster.StoreAdditionalInfo != null)
                {
                    _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.StoreAdditionalInfo, storeMaster.Store.UID, storeMaster.StoreAdditionalInfo, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                if (storeMaster.storeCredits != null && storeMaster.storeCredits.Count > 0)
                {
                    _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.StoreCredit, storeMaster.Store.UID, storeMaster.storeCredits, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                }
                if (storeMaster.storeAttributes != null && storeMaster.storeAttributes.Count > 0)
                {
                    _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.StoreAttributes, storeMaster.Store.UID, storeMaster.storeAttributes, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                }
                if (storeMaster.Addresses != null && storeMaster.Addresses.Count > 0)
                {
                    _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.Addressses, storeMaster.Store.UID, storeMaster.Addresses, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                }
                if (storeMaster.Contacts != null && storeMaster.Contacts.Count > 0)
                {
                    _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.Contacts, storeMaster.Store.UID, storeMaster.Contacts, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                }
                if (storeMaster != null && storeMaster.Store != null)
                {
                    _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.StoreMaster, storeMaster.Store.UID, storeMaster, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                }
            });
            //foreach (StoreMaster storeMaster in storeMasterList)
            //{

            //}
            return CreateOkApiResponse("Success");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to store  Master Details in Cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }




    [HttpGet]
    [Route("GetAllOrgCurrencyMaster")]
    public async Task<ActionResult> GetAllOrgCurrencyMaster()
    {
        try
        {
            var cachedData = _cacheService.HGetAll<Object>(Winit.Shared.Models.Constants.CacheConstants.OrgCurrency);
            if (cachedData != null && cachedData.Any())
            {
                PagedResponse<Object> cacheResponseskuMasters = new();
                cacheResponseskuMasters.PagedData = cachedData.Values.ToList();
                cacheResponseskuMasters.TotalCount = cachedData.Values.Count();
                return CreateOkApiResponse(cacheResponseskuMasters);
            }
            else
            {
                return NotFound("Data not found in cache.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Org Currency Master Details from cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }

    [HttpGet]
    [Route("ValidateCustomerValidity")]
    public IActionResult ValidateCustomerValidity(Winit.Modules.Store.Model.Classes.StoreMaster storeMaster)
    {
        try
        {
            if (storeMaster.Store.IsActive == true && storeMaster.Store.IsBlocked == true)

            {
                return Ok("Validated Customer successfully.");
            }
            else
            {
                return BadRequest("Invalid customer.");
            }
        }
        catch (Exception ex)
        {
            return BadRequest("Failed to validate : " + ex.Message);
        }
    }
    [HttpGet]
    [Route("OnCustomerSelected")]
    public IActionResult GetStandardListSources(StoreMasterViewModel storeMasterViewModel)
    {
        var result = new List<StandardListSource>();

        var OrguidList = new List<object>();

        List<IStoreCredit> storeCredits = storeMasterViewModel.StoreMaster.storeCredits;
        List<string> orgs = storeCredits.Select(e => e.OrgUID).Distinct().ToList();
        if (orgs != null && orgs.Count > 0)
        {
            foreach (string org in orgs)
            {
                StandardListSource standardListSource = new StandardListSource();
                standardListSource.SourceUID = org;
                standardListSource.SourceLabel = org;
                if (orgs.Count > 1)
                {
                    // Cache of Org
                }
            }

        }
        //foreach (var storecredit in loadApplicableOrg.Values)
        //{
        //    foreach (var item in storecredit)
        //    {
        //        if (item.GetType().GetProperty("OrgUID") != null)
        //        {
        //            var orguidValue = item.GetType().GetProperty("OrgUID").GetValue(item, null);

        //            if (orguidValue != null )
        //            {
        //                OrguidList.Add(orguidValue);
        //                var standardList = new StandardListSource()
        //                {
        //                    SourceUID = orguidValue.ToString()
        //                };

        //                result.Add(standardList);
        //            }
        //        }
        //    }
        //}

        return Ok(result);
    }

    [HttpGet]
    [Route("OnCustomerSelected1")]
    public async Task<IActionResult> OnCustomerSelected1(StoreMasterViewModel storeMasterViewModel)
    {
        try
        {
            var loadApplicableOrg = _cacheService.HGetAll<List<Winit.Modules.Store.Model.Classes.StoreCredit>>(Winit.Shared.Models.Constants.CacheConstants.StoreCredit);

            var standardListSourceList = loadApplicableOrg.Values
                .SelectMany(storeCreditList =>
                    storeCreditList.Select(storeCredit => new StandardListSource
                    {
                        SourceUID = storeCredit.UID
                    }))
                .ToList();

            return Ok(standardListSourceList);


        }
        catch (Exception)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }


    [HttpGet]
    [Route("PopulateStoreCache")]
    public async Task<ActionResult> PopulateStoreCache()
    {
        try
        {
            for (int i = 1; i <= 10000; i++)
            {
                Winit.Modules.Store.Model.Interfaces.IStore store = _serviceProvider.CreateInstance<Winit.Modules.Store.Model.Interfaces.IStore>();
                store.UID = "ST000" + i;
                store.Code = "ST000" + i;
                store.Name = "Store " + i;
                _cacheService.HSet("TestStore", store.UID, store, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                _cacheService.Set<string>($"Filter_Store_Name_{store.Name}", store.UID,
                WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                //_cacheService.HSet<string>("Filter_Store_Name", store.Name, store.UID);
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }


    [HttpPost]
    [Route("PopulateBuyPrice")]
    public async Task<ActionResult> PopulateBuyPrice([FromBody] string OrgUID)
    {
        try
        {
            var buyPriceList = await _skuPriceListBL.PopulateBuyPrice(OrgUID);
            if (buyPriceList == null)
            {
                return NotFound();
            }
            if (buyPriceList != null && buyPriceList.ToList().Count > 0)
            {
                foreach (BuyPrice buyPrice in buyPriceList.ToList())
                {
                    if (buyPrice != null)
                    {
                        _cacheService.HSet($"BuyPrice_{OrgUID}", buyPrice.SKUCode, buyPrice, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                    }
                }
            }
            return CreateOkApiResponse("Success");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to BuyPrice Details in Cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }


    [HttpPost]
    [Route("PopulateSettings")]
    public async Task<ActionResult> PopulateSettings()
    {
        try
        {
            var settingsList = await _settingBL.SelectAllSettingDetails(null, 0, 0, null, true);

            if (settingsList == null || !settingsList.PagedData.Any())
            {
                return NotFound();
            }

            foreach (var setting in settingsList.PagedData)
            {
                if (setting != null)
                {
                    _cacheService.HSet($"{Winit.Shared.Models.Constants.CacheConstants.Setting}{setting.UID}", setting.UID, setting, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                }
            }
            _cacheService.Set($"{Winit.Shared.Models.Constants.CacheConstants.Setting}",
            settingsList, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);

            return CreateOkApiResponse("Success");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to cache Settings Details");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetSettingsData")]
    public async Task<ActionResult> GetSettingsData(string fieldName = null)
    {
        try
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                List<Winit.Modules.Setting.Model.Classes.Setting> settingsDataFromCache = _cacheService.Get<List<Winit.Modules.Setting.Model.Classes.Setting>>(Winit.Shared.Models.Constants.CacheConstants.Setting);
                var settingsList = settingsDataFromCache?.ToList<ISetting>() ?? new List<ISetting>();
                return CreateOkApiResponse(settingsList);
            }
            else
            {
                Winit.Modules.Setting.Model.Classes.Setting settingObjFromCache = _cacheService.HGet<Winit.Modules.Setting.Model.Classes.Setting>($"{Winit.Shared.Models.Constants.CacheConstants.Setting}{fieldName}", fieldName);
                return CreateOkApiResponse(settingObjFromCache);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Setting Details from cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }




    [HttpPost]
    [Route("CheckSynchronization")]
    public async Task<ActionResult> CheckSynchronization(int input)
    {
        try
        {
            return CreateOkApiResponse($"Success: {input}");
        }
        catch (Exception)
        {
            return CreateErrorResponse("An error occurred while processing the request.");
        }
    }


    [HttpPost]
    [Route("PrepareSKUAttributeDDL")]
    public async Task<ActionResult> PrepareSKUAttributeDDL()
    {
        try
        {
            var sKUAttributeLevelData = await _sKUGroupTypeBL.SelectSKUAttributeDDL();
            if (sKUAttributeLevelData == null)
            {
                return NotFound();
            }
            if (sKUAttributeLevelData.SKUGroupTypes != null)
            {
                _cacheService.Set(Winit.Shared.Models.Constants.CacheConstants.SKUAttributeLevelType, sKUAttributeLevelData.SKUGroupTypes, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
            }

            if (sKUAttributeLevelData.SKUGroups != null)
            {
                Parallel.ForEach(sKUAttributeLevelData.SKUGroups, skuGroup =>
                {
                    _cacheService.HSet(
                        Winit.Shared.Models.Constants.CacheConstants.SKUAttributeLevel,
                        skuGroup.Key,
                        skuGroup.Value,
                        WINITServices.Classes.CacheHandler.ExpirationType.Absolute,
                        -1
                    );
                });

                //foreach (var skuGroup in sKUAttributeLevelData.SKUGroups)
                //{
                //    _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.SKUAttributeLevel, skuGroup.Key, skuGroup.Value, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                //}
            }
            return CreateOkApiResponse("Success");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to store SKUAttributeDDL Details in Cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetAllSKUAttributeDDL")]
    public async Task<ActionResult> GetAllSKUAttributeDDL()
    {

        List<ISelectionItem> skuGroupTypeData = null;
        List<ISelectionItem> skuAttributeData = null;
        ISKUAttributeLevel sKUAttributeLevel = _serviceProvider.CreateInstance<Winit.Modules.SKU.Model.Interfaces.ISKUAttributeLevel>();
        try
        {
            List<SelectionItem> skuGroupTypeDataConcrete = _cacheService.Get<List<SelectionItem>>(Winit.Shared.Models.Constants.CacheConstants.SKUAttributeLevelType);
            skuGroupTypeData = skuGroupTypeDataConcrete.Cast<ISelectionItem>().ToList();
            sKUAttributeLevel.SKUGroupTypes = skuGroupTypeData;
            Dictionary<string, List<SelectionItem>> skuAttributeCachedData = _cacheService.HGetAll<List<SelectionItem>>(Winit.Shared.Models.Constants.CacheConstants.SKUAttributeLevel);
            if (skuAttributeCachedData != null && skuAttributeCachedData.Any())
            {
                sKUAttributeLevel.SKUGroups = new Dictionary<string, List<ISelectionItem>>();
                foreach (var kvp in skuAttributeCachedData)
                {
                    var selectionItems = kvp.Value.Select(item => (ISelectionItem)item).ToList();
                    sKUAttributeLevel.SKUGroups[kvp.Key] = selectionItems;
                }
            }
            return CreateOkApiResponse(sKUAttributeLevel);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUAttribute Details from cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }

    [HttpPost]
    [Route("PrepareGetEmpDropDown")]
    public async Task<ActionResult> PrepareGetEmpDropDown(string orgUID)
    {
        try
        {
            IEnumerable<ISelectionItem> selectionItemList = await _dropDownsBL.GetEmpDropDown(orgUID);
            if (selectionItemList == null)
            {
                return NotFound();
            }
            if (selectionItemList != null && selectionItemList.Any())
            {
                _cacheService.Set(Winit.Shared.Models.Constants.CacheConstants.EmpDropDown, selectionItemList, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
            }
            return CreateOkApiResponse("Success");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to store EmpDropDown Details in Cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetEmpDropDown")]
    public async Task<ActionResult> GetEmpDropDown()
    {

        List<ISelectionItem> empDropdownData = null;
        try
        {
            List<SelectionItem> empDropdownDataConcrete = _cacheService.Get<List<SelectionItem>>(Winit.Shared.Models.Constants.CacheConstants.EmpDropDown);
            empDropdownData = empDropdownDataConcrete.ToList<ISelectionItem>();

            return CreateOkApiResponse(empDropdownData);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve EmpDropDown Details from cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }


    [HttpPost]
    [Route("PreparePromotionData")]
    public async Task<ActionResult> PreparePromotionData()
    {
        try
        {
            IEnumerable<IPromotionData> promotionDataList = await _promotionBL.GetPromotionData();
            if (promotionDataList == null)
            {
                return NotFound();
            }
            _cacheService.Set(Winit.Shared.Models.Constants.CacheConstants.PromotionData, promotionDataList, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
            return CreateOkApiResponse("Success");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to store PromotionData  in Cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetPromotionData")]
    public async Task<ActionResult> GetPromotionData()
    {

        List<IPromotionData> promotionList = null;
        try
        {
            List<PromotionData> promotionDataCache = _cacheService.Get<List<PromotionData>>(Winit.Shared.Models.Constants.CacheConstants.PromotionData);
            promotionList = promotionDataCache.ToList<IPromotionData>();

            return CreateOkApiResponse(promotionList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve PromotionData Details from cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }

    [HttpPost]
    [Route("PrepareLocationData")]
    public async Task<ActionResult> PrepareLocationData(PagingRequest pagingRequest)
    {
        try
        {
            PagedResponse<ILocation> locationList = await _locationBL.SelectAllLocationDetails(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
            pagingRequest.IsCountRequired);
            if (locationList == null)
            {
                return NotFound();
            }
            _cacheService.Set(Winit.Shared.Models.Constants.CacheConstants.Location, locationList.PagedData, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
            return CreateOkApiResponse("Success");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to store Location  in Cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetAllLocations")]
    public async Task<ActionResult> GetAllLocations()
    {

        IEnumerable<ILocation> locationList = null;
        try
        {
            IEnumerable<Winit.Modules.Location.Model.Classes.Location> cacheLocation =
                _cacheService.Get<IEnumerable<Winit.Modules.Location.Model.Classes.Location>>(Winit.Shared.Models.Constants.CacheConstants.Location);
            locationList = cacheLocation.ToList<ILocation>();

            return CreateOkApiResponse(locationList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Location Details from cache");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }
    [HttpPut]
    [Route("UpdatePromotion")]
    public async Task<ActionResult> UpdatePromotion(Winit.Modules.Promotion.Model.Classes.PromotionView updatePromotionView)
    {


        try
        {
            var updCnt = await _promotionBL.UpdatePromotion(updatePromotionView);

            return CreateOkApiResponse(updCnt);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Update Promotion Details");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }
    [HttpPost]
    [Route("PrepareSkuClassGroupItems")]
    public async Task<IActionResult> PrepareSkuClassGroupItems([FromBody] List<string> linkedItemUIDs = null)
    {
        try
        {
            var skuClassItems = await _iSKUClassGroupItemsBL.PrepareSkuClassForCache(linkedItemUIDs);
            var groupedDictionary = skuClassItems.GroupBy(x => x.SKUClassGroupUID)
                                 .ToDictionary(g => g.Key, g => g.Select(x => x.SKUUID).ToList());

            foreach (var item in groupedDictionary)
            {
                _cacheService.Set(item.Key, item.Value, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
            }
            return CreateOkApiResponse("Success");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to store Sku class group Items");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }

    }

    #region unused methods
    //[HttpGet]
    //[Route("GetALLSKUMasterData")]
    //public async Task<ActionResult> GetALLSKUMasterData()
    //{
    //    List<SKUMaster> sKUMastersData = null;
    //    List<ISKUMaster> sKUMasters = null;
    //    try
    //    {
    //        var cacheSkuMaster = _cacheService.HGetAll<SKUMaster>(Winit.Shared.Models.Constants.CacheConstants.SKUMaster);
    //        if (cacheSkuMaster != null)
    //        {
    //            sKUMastersData.AddRange(cacheSkuMaster.Values);
    //        }
    //        sKUMasters = sKUMastersData.ToList<ISKUMaster>();

    //        return CreateOkApiResponse(sKUMasters);
    //    }
    //    catch (Exception ex)
    //    {
    //        Log.Error(ex, "Failed to retrieve sKUMasters Details from cache");
    //        return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
    //    }
    //}



    //[HttpGet]
    //[Route("GetALLSKUMaster")]
    //public async Task<ActionResult> GetALLSKUMaster()
    //{
    //    try
    //    {
    //        var cachedData = _cacheService.HGetAll<Object>(Winit.Shared.Models.Constants.CacheConstants.SKUMaster);
    //        if (cachedData != null && cachedData.Any())
    //        {
    //            PagedResponse<Object> cacheResponseskuMasters = new();
    //            cacheResponseskuMasters.PagedData = cachedData.Values.ToList();
    //            cacheResponseskuMasters.TotalCount = cachedData.Values.Count();
    //            return CreateOkApiResponse(cacheResponseskuMasters);
    //        }
    //        else
    //        {
    //            return NotFound("Data not found in cache.");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Log.Error(ex, "Failed to retrieve SKU Master Details from cache");
    //        return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
    //    }
    //}


    //[HttpGet]
    //[Route("GetAllStoreMaster")]
    //public async Task<ActionResult> GetAllStoreMaster()
    //{
    //    try
    //    {
    //        var cachedData = _cacheService.HGetAll<StoreMasterDTO>(Winit.Shared.Models.Constants.CacheConstants.StoreMaster);

    //        if (!cachedData.Any())
    //        {
    //            var storeUIDs = new List<string>();
    //           _=_storeBL.PrepareStoreMaster(storeUIDs);
    //            cachedData = _cacheService.HGetAll<StoreMasterDTO>(Winit.Shared.Models.Constants.CacheConstants.StoreMaster);
    //        }
    //        var responseToReturn = new PagedResponse<StoreMasterDTO>
    //        {
    //            PagedData = cachedData.Values.ToList(),
    //            TotalCount = cachedData.Count
    //        };

    //        return CreateOkApiResponse(responseToReturn);
    //    }
    //    catch (Exception ex)
    //    {
    //        Log.Error(ex, "Failed to retrieve Store Master Details from cache");
    //        return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
    //    }
    //}
    #endregion


}
