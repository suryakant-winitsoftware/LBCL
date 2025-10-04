using ICSharpCode.SharpZipLib.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.PriceLadder.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKUClass.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITServices.Interfaces.CacheHandler;
using WINITSharedObjects.Enums;

namespace WINITAPI.Controllers.SKU;

[ApiController]
[Route("api/[Controller]")]
//[Authorize]
public class SKUController : WINITBaseController
{
    private readonly Winit.Modules.SKU.BL.Interfaces.ISKUBL _sKUBL;
    private readonly ISortHelper _sortHelper;
    private readonly CommonFunctions commonFunctions = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly DataPreparationController _dataPreparationController;
    private readonly ISKUPriceLadderingBL _sKUPriceLadderingBL;
    private readonly ISKUClassGroupItemsBL _sKUClassGroupItemsBL;

    public SKUController(IServiceProvider serviceProvider,
        Winit.Modules.SKU.BL.Interfaces.ISKUBL sKUBL,
         ISortHelper sortHelper, DataPreparationController dataPreparationController,
         ISKUPriceLadderingBL sKUPriceLadderingBL, ISKUClassGroupItemsBL sKUClassGroupItemsBL)
        : base(serviceProvider)
    {
        _sKUBL = sKUBL;
        _sortHelper = sortHelper;
        _serviceProvider = serviceProvider;
        _dataPreparationController = dataPreparationController;
        _sKUPriceLadderingBL = sKUPriceLadderingBL;
        _sKUClassGroupItemsBL = sKUClassGroupItemsBL;
    }

    [HttpGet]
    [Route("SelectSKUByUID")]
    public async Task<ActionResult> SelectSKUByUID([FromQuery] string UID)
    {
        try
        {
            Winit.Modules.SKU.Model.Interfaces.ISKU sKUList = await _sKUBL.SelectSKUByUID(UID);
            return sKUList != null ? CreateOkApiResponse(sKUList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUList with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }
    [HttpPost]
    [Route("CreateSKU")]
    public async Task<ActionResult> CreateSKU([FromBody] ISKU sku)
    {
        try
        {
            if (sku is ISKUV1 skuV1)
            {
                skuV1.ServerAddTime = sku.ServerModifiedTime = DateTime.Now;
                int ratValue = await _sKUBL.CreateSKU(skuV1);

                if (ratValue > 0)
                {
                    // Directly update cache without relying on PrepareSKUMaster
                    if (_cacheService != null)
                    {
                        try
                        {
                            // Fetch the newly created SKU from database to get all fields
                            var createdSku = await _sKUBL.SelectSKUByUID(skuV1.UID);
                            if (createdSku != null)
                            {
                                Log.Information("[CREATE] Step 1: Fetched SKU from DB - UID: {UID}, OrgUID: {OrgUID}, Name: {Name}", 
                                    createdSku.UID, createdSku.OrgUID, createdSku.Name);
                                
                                // Update the main SKU cache
                                _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.SKU, createdSku.UID, createdSku, 
                                    WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                                Log.Information("[CREATE] Step 2: Updated SKU hash cache with key: {HashKey}, field: {Field}", 
                                    Winit.Shared.Models.Constants.CacheConstants.SKU, createdSku.UID);
                                
                                // Verify the cache was actually updated
                                var verifyCache = _cacheService.HGet<Winit.Modules.SKU.Model.Interfaces.ISKU>(
                                    Winit.Shared.Models.Constants.CacheConstants.SKU, createdSku.UID);
                                if (verifyCache != null)
                                {
                                    Log.Information("[CREATE] Step 3: Verified SKU in cache - Name: {Name}", verifyCache.Name);
                                }
                                else
                                {
                                    Log.Error("[CREATE] Step 3: FAILED - SKU not found in cache after HSet!");
                                }
                                
                                // CRITICAL: Update the filter cache so SKU appears in searches
                                if (!string.IsNullOrEmpty(createdSku.OrgUID))
                                {
                                    string filterKey = $"{Winit.Shared.Models.Constants.CacheConstants.FilterSKUOrgUID}{createdSku.OrgUID}_{createdSku.UID}";
                                    _cacheService.Set<string>(filterKey, createdSku.UID, 
                                        WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                                    
                                    Log.Information("[CREATE] Step 4: Created filter key: {FilterKey} with value: {Value}", filterKey, createdSku.UID);
                                    
                                    // Verify the filter key was created
                                    var verifyFilter = _cacheService.Get<string>(filterKey);
                                    if (verifyFilter != null)
                                    {
                                        Log.Information("[CREATE] Step 5: Verified filter key exists with value: {Value}", verifyFilter);
                                    }
                                    else
                                    {
                                        Log.Error("[CREATE] Step 5: FAILED - Filter key not found after Set!");
                                    }
                                }
                                
                                // Call PrepareSKUMaster synchronously to ensure it completes
                                try
                                {
                                    Log.Information("[CREATE] Step 6: Calling PrepareSKUMaster for complete cache update");
                                    PrepareSKURequestModel prepareSKURequestModel = new()
                                    {
                                        SKUUIDs = new List<string> { createdSku.UID },
                                        OrgUIDs = null,  // Don't filter by Org, let it find the SKU
                                        DistributionChannelUIDs = null,
                                        AttributeTypes = null
                                    };
                                    var masterResult = await _dataPreparationController.PrepareSKUMaster(prepareSKURequestModel);
                                    
                                    if (masterResult != null && masterResult.Value != null)
                                    {
                                        Log.Information("[CREATE] Step 7: PrepareSKUMaster completed successfully");
                                    }
                                    else
                                    {
                                        Log.Warning("[CREATE] Step 7: PrepareSKUMaster returned null/empty - cache was updated directly anyway");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Warning(ex, "[CREATE] PrepareSKUMaster failed but cache was already updated directly");
                                }
                            }
                        }
                        catch (Exception cacheEx)
                        {
                            Log.Error(cacheEx, "Failed to update cache after creating SKU");
                        }
                    }
                    
                    return CreateOkApiResponse(ratValue);
                }
                else
                {
                    throw new Exception("Create failed");
                }
            }
            else
            {
                throw new Exception("Create failed");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create SKU details");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }
    [HttpPut]
    [Route("UpdateSKU")]
    public async Task<ActionResult> UpdateSKU([FromBody] Winit.Modules.SKU.Model.Classes.SKU sku)
    {
        try
        {
            ISKU existingDetails = await _sKUBL.SelectSKUByUID(sku.UID);
            if (existingDetails != null)
            {
                sku.ServerModifiedTime = DateTime.Now;
                int ratValue = await _sKUBL.UpdateSKU(sku);
                if (ratValue > 0)
                {
                    // Fetch the updated SKU from database to ensure we have latest data
                    var updatedSku = await _sKUBL.SelectSKUByUID(sku.UID);
                    
                    // Directly update cache without relying on PrepareSKUMaster
                    if (_cacheService != null && updatedSku != null)
                    {
                        try
                        {
                            // Get the old SKU to check if OrgUID changed
                            var oldFilterKeys = _cacheService.GetKeyByPattern($"{Winit.Shared.Models.Constants.CacheConstants.FilterSKUOrgUID}*_{sku.UID}");
                            if (oldFilterKeys != null && oldFilterKeys.Any())
                            {
                                // Remove old filter keys
                                foreach (var oldKey in oldFilterKeys)
                                {
                                    _cacheService.Remove(oldKey);
                                }
                            }
                            
                            // Update the main SKU cache
                            _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.SKU, updatedSku.UID, updatedSku, 
                                WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                            
                            // CRITICAL: Update the filter cache so SKU appears in searches
                            if (!string.IsNullOrEmpty(updatedSku.OrgUID))
                            {
                                string filterKey = $"{Winit.Shared.Models.Constants.CacheConstants.FilterSKUOrgUID}{updatedSku.OrgUID}_{updatedSku.UID}";
                                _cacheService.Set<string>(filterKey, updatedSku.UID, 
                                    WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                                
                                Log.Information("Updated SKU cache entries - Hash: {UID}, Filter: {FilterKey}", updatedSku.UID, filterKey);
                            }
                            
                            // Also try to update via PrepareSKUMaster (but don't wait for it)
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    PrepareSKURequestModel prepareSKURequestModel = new()
                                    {
                                        SKUUIDs = new List<string> { updatedSku.UID },
                                        OrgUIDs = new List<string> { updatedSku.OrgUID }
                                    };
                                    await _dataPreparationController.PrepareSKUMaster(prepareSKURequestModel);
                                }
                                catch (Exception ex)
                                {
                                    Log.Warning(ex, "Background PrepareSKUMaster failed but cache was updated directly");
                                }
                            });
                        }
                        catch (Exception cacheEx)
                        {
                            Log.Error(cacheEx, "Failed to update cache after updating SKU");
                        }
                    }
                    
                    return CreateOkApiResponse(ratValue);
                }
                else
                {
                    throw new Exception("Update failed");
                }
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating SKU details");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }
    [HttpDelete]
    [Route("DeleteSKU")]
    public async Task<ActionResult> DeleteSKU([FromQuery] string UID)
    {
        try
        {
            // Get the SKU details before deletion for cache cleanup
            var skuToDelete = await _sKUBL.SelectSKUByUID(UID);
            
            List<string> skuUIDs = new List<string>();
            skuUIDs.Add(UID);
            int result = await _sKUBL.DeleteSKU(UID);
            if (result > 0)
            {
                // Clear cache after successful deletion
                if (_cacheService != null)
                {
                    try
                    {
                        // Remove SKU from cache - using synchronous Remove method since HDelAsync doesn't exist
                        // Note: This removes the entire hash key, not just the field
                        // Consider implementing HDel in ICacheService if field-level deletion is needed
                        _cacheService.InvalidateCache(Winit.Shared.Models.Constants.CacheConstants.SKU, UID);
                        _cacheService.InvalidateCache(Winit.Shared.Models.Constants.CacheConstants.SKUConfig, UID);
                        _cacheService.InvalidateCache(Winit.Shared.Models.Constants.CacheConstants.SKUUOM, UID);
                        _cacheService.InvalidateCache(Winit.Shared.Models.Constants.CacheConstants.SKUAttributes, UID);
                        _cacheService.InvalidateCache(Winit.Shared.Models.Constants.CacheConstants.TaxSKUMap, UID);
                        _cacheService.InvalidateCache(Winit.Shared.Models.Constants.CacheConstants.SKUMaster, UID);
                        
                        // Remove filter cache entry if we have OrgUID
                        if (skuToDelete != null && !string.IsNullOrEmpty(skuToDelete.OrgUID))
                        {
                            _cacheService.Remove($"{Winit.Shared.Models.Constants.CacheConstants.FilterSKUOrgUID}{skuToDelete.OrgUID}_{UID}");
                        }
                        
                        Log.Information("Cache cleared after deleting SKU: {UID}", UID);
                    }
                    catch (Exception cacheEx)
                    {
                        Log.Warning(cacheEx, "Failed to clear cache after deleting SKU, but deletion succeeded");
                    }
                }
                return CreateOkApiResponse(result);
            }
            throw new Exception("Delete failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting SKU");
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }
    [HttpPost]
    [Route("RefreshSKUCache")]
    public async Task<ActionResult> RefreshSKUCache()
    {
        try
        {
            if (_cacheService != null)
            {
                // Clear all SKU-related caches
                _cacheService.Remove(Winit.Shared.Models.Constants.CacheConstants.SKU);
                _cacheService.Remove(Winit.Shared.Models.Constants.CacheConstants.SKUConfig);
                _cacheService.Remove(Winit.Shared.Models.Constants.CacheConstants.SKUUOM);
                _cacheService.Remove(Winit.Shared.Models.Constants.CacheConstants.SKUAttributes);
                _cacheService.Remove(Winit.Shared.Models.Constants.CacheConstants.TaxSKUMap);
                _cacheService.Remove(Winit.Shared.Models.Constants.CacheConstants.SKUMaster);
                
                // Clear all filter keys
                var filterKeys = _cacheService.GetKeyByPattern($"{Winit.Shared.Models.Constants.CacheConstants.FilterSKUOrgUID}*");
                if (filterKeys != null && filterKeys.Any())
                {
                    foreach (var key in filterKeys)
                    {
                        _cacheService.Remove(key);
                    }
                }
                
                Log.Information("Successfully cleared all SKU caches");
                return CreateOkApiResponse("SKU cache refreshed successfully");
            }
            return BadRequest("Cache service not available");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to refresh SKU cache");
            return CreateErrorResponse("An error occurred while refreshing cache: " + ex.Message);
        }
    }
    
    [HttpPost]
    [Route("SelectAllSKUDetails")]
    public async Task<ActionResult> SelectAllSKUDetails([FromBody] PagingRequest pagingRequest)
    {
        try
        {
            PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKU> pagedResponseSKUList = null;

            if (pagingRequest == null || pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid request data");
            }

            int startIndex = (pagingRequest.PageNumber - 1) * pagingRequest.PageSize;
            int endIndex = startIndex + pagingRequest.PageSize - 1;
            List<Winit.Modules.SKU.Model.Interfaces.ISKU> skuList = new();
            List<string> searchPatternsOrgUID = new();
            string nameFilter = string.Empty;
            string codeFilter = string.Empty;
            string storeUIDFilter = string.Empty;
            string broadClassificationFilter = string.Empty;
            string branchUIDFilter = string.Empty;
            List<string> supplierOrgUids = [];
            DateTime dateFilter = DateTime.Now;
            List<string> uidFilter = null;

            if (pagingRequest.FilterCriterias != null && pagingRequest.FilterCriterias.Count > 0)
            {
                var orgUIDFilter = pagingRequest.FilterCriterias.FirstOrDefault(fc => fc.Name == "OrgUID");
                if (orgUIDFilter != null)
                {
                    List<string> orgUIDs = JsonConvert.DeserializeObject<List<string>>(orgUIDFilter.Value.ToString());
                    if (orgUIDs != null && orgUIDs.Any())
                    {
                        foreach (var orgUID in orgUIDs)
                        {
                            searchPatternsOrgUID.Add($"{Winit.Shared.Models.Constants.CacheConstants.FilterSKUOrgUID}{orgUID}_*");
                        }
                    }
                    else
                    {
                        searchPatternsOrgUID.Add($"{Winit.Shared.Models.Constants.CacheConstants.FilterSKUOrgUID}*");
                    }
                    pagingRequest.FilterCriterias.Remove(orgUIDFilter);
                }

                var nameCriteria = pagingRequest.FilterCriterias.FirstOrDefault(fc => "Name".Equals(fc.Name, StringComparison.OrdinalIgnoreCase));
                var codeCriteria = pagingRequest.FilterCriterias.FirstOrDefault(fc => "Code".Equals(fc.Name, StringComparison.OrdinalIgnoreCase));
                var uidCriteria = pagingRequest.FilterCriterias.FirstOrDefault(fc => "UID".Equals(fc.Name, StringComparison.OrdinalIgnoreCase));
                var storeUIDCriteria = pagingRequest.FilterCriterias.FirstOrDefault(fc => "StoreUID".Equals(fc.Name, StringComparison.OrdinalIgnoreCase));
                var supplierOrgUidsCriteria = pagingRequest.FilterCriterias.FirstOrDefault(fc => "SupplierOrgUIDs".Equals(fc.Name, StringComparison.OrdinalIgnoreCase));
                var dateCriteria = pagingRequest.FilterCriterias.FirstOrDefault(fc => "Date".Equals(fc.Name, StringComparison.OrdinalIgnoreCase));
                var broadClassificationCriteria = pagingRequest.FilterCriterias.FirstOrDefault(fc => "BroadClassification".Equals(fc.Name, StringComparison.OrdinalIgnoreCase));
                var branchCriteria = pagingRequest.FilterCriterias.FirstOrDefault(fc => "BranchUID".Equals(fc.Name, StringComparison.OrdinalIgnoreCase));

                if (nameCriteria != null)
                {
                    nameFilter = nameCriteria.Value.ToString();
                    pagingRequest.FilterCriterias.Remove(nameCriteria);
                }

                if (codeCriteria != null)
                {
                    codeFilter = codeCriteria.Value.ToString();
                    pagingRequest.FilterCriterias.Remove(codeCriteria);
                }

                if (uidCriteria != null)
                {
                    uidFilter = JsonConvert.DeserializeObject<List<string>>(uidCriteria.Value.ToString());
                    pagingRequest.FilterCriterias.Remove(uidCriteria);
                }

                if (storeUIDCriteria != null)
                {
                    storeUIDFilter = storeUIDCriteria.Value.ToString();
                    pagingRequest.FilterCriterias.Remove(storeUIDCriteria);
                }
                if (branchCriteria != null)
                {
                    branchUIDFilter = branchCriteria.Value.ToString();
                    pagingRequest.FilterCriterias.Remove(branchCriteria);
                }
                if (broadClassificationCriteria != null)
                {
                    broadClassificationFilter = broadClassificationCriteria.Value.ToString();
                    pagingRequest.FilterCriterias.Remove(broadClassificationCriteria);
                }

                if (supplierOrgUidsCriteria != null)
                {
                    supplierOrgUids.AddRange(JsonConvert.DeserializeObject<List<string>>(supplierOrgUidsCriteria.Value.ToString()));
                    pagingRequest.FilterCriterias.Remove(supplierOrgUidsCriteria);
                }

                if (dateCriteria != null)
                {
                    dateFilter = dateCriteria == null ? DateTime.Now : DateTime.Parse(dateCriteria.Value.ToString());
                    pagingRequest.FilterCriterias.Remove(dateCriteria);
                }
            }

            List<string> searchedKeysByOrgUID = new();
            if (searchPatternsOrgUID.Any())
            {
                foreach (var pattern in searchPatternsOrgUID)
                {
                    var keys = _cacheService.GetKeyByPattern(pattern);
                    if (keys != null)
                    {
                        searchedKeysByOrgUID.AddRange(keys);
                    }
                }
            }
            else
            {
                var keys = _cacheService.GetKeyByPattern($"{Winit.Shared.Models.Constants.CacheConstants.FilterSKUOrgUID}*");
                if (keys != null)
                {
                    searchedKeysByOrgUID.AddRange(keys);
                }
            }

            List<string> filteredSKUUIDByOrgUID = new();
            if (searchedKeysByOrgUID.Any())
            {
                var multipleResult = _cacheService.GetMultiple<string>(searchedKeysByOrgUID);
                if (multipleResult?.Values != null)
                {
                    filteredSKUUIDByOrgUID.AddRange(multipleResult.Values);
                }
            }

            if (uidFilter != null && uidFilter.Any())
            {
                filteredSKUUIDByOrgUID.RemoveAll(e => !uidFilter.Contains(e));
            }

            if (filteredSKUUIDByOrgUID.Any())
            {
                var skuData = _cacheService.HGet<Winit.Modules.SKU.Model.Interfaces.ISKU>($"{Winit.Shared.Models.Constants.CacheConstants.SKU}", filteredSKUUIDByOrgUID);
                if (skuData != null && skuData.Any())
                {
                    skuList.AddRange(skuData);
                }
                else
                {
                    // If cache is empty, fetch from database and update cache
                    Log.Warning("Cache empty for SKU UIDs, fetching from database: {UIDs}", string.Join(",", filteredSKUUIDByOrgUID.Take(5)));
                    
                    // Fetch missing SKUs from database
                    foreach (var skuUid in filteredSKUUIDByOrgUID)
                    {
                        var dbSku = await _sKUBL.SelectSKUByUID(skuUid);
                        if (dbSku != null)
                        {
                            skuList.Add(dbSku);
                            
                            // Update cache with the fetched SKU
                            _cacheService.HSet(Winit.Shared.Models.Constants.CacheConstants.SKU, skuUid, dbSku, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                        }
                    }
                    
                    if (skuList.Any())
                    {
                        Log.Information("Fetched {Count} SKUs from database and updated cache", skuList.Count);
                    }
                }
            }

            skuList.RemoveAll(p => p is ISKUV1 v1 && (string.IsNullOrEmpty(v1.HSNCode) || !v1.IsActive));
            if (!string.IsNullOrEmpty(nameFilter))
            {
                skuList = skuList.Where(sku => sku.Name != null && sku.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(codeFilter))
            {
                skuList = skuList.Where(sku => sku.Code != null && sku.Code.Contains(codeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(storeUIDFilter))
            {
                List<string> skuClassGroupUIDs = _cacheService.HGet<List<string>>(CacheConstants.StoreLinkedItemUIDMapping, storeUIDFilter);
                if (skuClassGroupUIDs != null && skuClassGroupUIDs.Any())
                {
                    var storeLinkedAllowedSKU = _cacheService.GetMultiple<List<string>>(skuClassGroupUIDs);
                    if (storeLinkedAllowedSKU?.Values != null)
                    {
                        var distinctAllowedSKU = storeLinkedAllowedSKU.Values
                            .SelectMany(list => list ?? Enumerable.Empty<string>())  // Handle null lists
                            .Where(uid => !string.IsNullOrEmpty(uid))  // Filter out null/empty UIDs
                            .Distinct()
                            .ToList();

                        if (distinctAllowedSKU.Any())
                        {
                            skuList = skuList.FindAll(sku => sku is ISKUV1 skuV1 && distinctAllowedSKU.Contains(skuV1.UID));
                        }
                    }
                }

                List<int> productCategoryIds = await _sKUPriceLadderingBL.GetProductCategoryIdsByStoreUID(storeUIDFilter, dateFilter);
                if (productCategoryIds != null && productCategoryIds.Any())
                {
                    skuList = skuList.FindAll(sku => sku is ISKUV1 skuV1 && productCategoryIds.Contains(skuV1.ProductCategoryId) && skuV1.IsActive);
                }
            }

            if (!string.IsNullOrEmpty(branchUIDFilter) || !string.IsNullOrEmpty(broadClassificationFilter))
            {
                List<int> productCategoryIds = await _sKUPriceLadderingBL.GetProductCategoryIdsByStoreUID(null, dateFilter, broadClassificationFilter, branchUIDFilter);
                if (productCategoryIds != null && productCategoryIds.Any())
                {
                    skuList = skuList.FindAll(sku => sku is ISKUV1 skuV1 && productCategoryIds.Contains(skuV1.ProductCategoryId) && skuV1.IsActive);
                }
            }

            if (supplierOrgUids != null && supplierOrgUids.Any())
            {
                skuList = skuList.Where(sku => sku?.SupplierOrgUID != null && supplierOrgUids.Contains(sku.SupplierOrgUID)).ToList();
            }

            if (pagingRequest.FilterCriterias != null && pagingRequest.FilterCriterias.Any())
            {
                var rmSKUList = new List<ISKU>();
                foreach (var sKU in skuList)
                {
                    if (sKU is ISKUV1 skuV1 && skuV1.FilterKeys != null)
                    {
                        foreach (Winit.Shared.Models.Enums.FilterCriteria filter in pagingRequest.FilterCriterias)
                        {
                            var filtervalue = JsonConvert.DeserializeObject<List<object>>(filter.Value?.ToString());
                            if (filtervalue != null && !filtervalue.Any(value => skuV1.FilterKeys.Contains(value)))
                            {
                                rmSKUList.Add(sKU);
                                break;
                            }
                        }
                    }
                }
                skuList.RemoveAll(e => rmSKUList.Contains(e));
            }

            List<Winit.Shared.Models.Enums.SortCriteria> sortCriteriaList = pagingRequest.SortCriterias != null && pagingRequest.SortCriterias.Count > 0
                ? pagingRequest.SortCriterias.Select(sort => new Winit.Shared.Models.Enums.SortCriteria(sort.SortParameter, sort.Direction)).ToList()
                : new List<Winit.Shared.Models.Enums.SortCriteria> { new Winit.Shared.Models.Enums.SortCriteria("Name", Winit.Shared.Models.Enums.SortDirection.Asc) };

            skuList = await _sortHelper.Sort(skuList, sortCriteriaList);

            List<Winit.Modules.SKU.Model.Interfaces.ISKU> pagingData = null;
            if (skuList != null)
            {
                pagingData = commonFunctions.GetDataInRange<Winit.Modules.SKU.Model.Interfaces.ISKU>(skuList, startIndex, endIndex);
            }

            pagedResponseSKUList = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKU>
            {
                PagedData = pagingData ?? new List<Winit.Modules.SKU.Model.Interfaces.ISKU>(),
                TotalCount = skuList?.Count ?? 0
            };

            return CreateOkApiResponse(pagedResponseSKUList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve SKU Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost]
    [Route("GetAllSKUMasterData")]
    public async Task<ActionResult> GetAllSkuMasterData(SKUMasterRequest sKUMasterRequest = null)
    {
        try
        {
            PagedResponse<ISKUMaster> pagedResponseSkuMasterList = null;
            Dictionary<string, ISKUMaster> cachedData = new Dictionary<string, ISKUMaster>();
            if (sKUMasterRequest != null && sKUMasterRequest.SKUUIDs != null && sKUMasterRequest.SKUUIDs.Any())
            {
                foreach (string skuUID in sKUMasterRequest.SKUUIDs)
                {
                    cachedData.Add(skuUID, _cacheService.HGet<ISKUMaster>($"{Winit.Shared.Models.Constants.CacheConstants.SKUMaster}", skuUID));
                }
            }
            else
            {
                cachedData = _cacheService.HGetAll<ISKUMaster>($"{Winit.Shared.Models.Constants.CacheConstants.SKUMaster}");
            }
            pagedResponseSkuMasterList = new PagedResponse<ISKUMaster>();

            if (cachedData != null && cachedData.Count > 0)
            {
                //foreach (ISKUMaster sKUMasterData in cachedData.Values)
                //{
                //    sKUMasterlist.Add(sKUMasterData);
                //}

                pagedResponseSkuMasterList.PagedData = cachedData.Values; ;
                pagedResponseSkuMasterList.TotalCount = cachedData.Count;
                return CreateOkApiResponse(pagedResponseSkuMasterList);
            }
            else
            {
                List<ISKUMaster> sKUMasterlist = await _sKUBL.PrepareSKUMaster(sKUMasterRequest.OrgUIDs,
                    sKUMasterRequest.DistributionChannelUIDs,
                    sKUMasterRequest.SKUUIDs, sKUMasterRequest.AttributeTypes);

                pagedResponseSkuMasterList.PagedData = sKUMasterlist;
                pagedResponseSkuMasterList.TotalCount = sKUMasterlist.Count;
                return CreateOkApiResponse(pagedResponseSkuMasterList);

                //return CreateOkApiResponse(await _sKUBL.PrepareSKUMaster(sKUMasterRequest.OrgUIDs,
                //    sKUMasterRequest.DistributionChannelUIDs,
                //    sKUMasterRequest.SKUUIDs, sKUMasterRequest.AttributeTypes));
            }
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
    [HttpPost]
    [Route("SelectAllSKUDetailsWebView")]
    public async Task<ActionResult> SelectAllSKUDetailsWebView(PagingRequest pagingRequest)
    {
        try
        {
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }

            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUListView> pagedResponseList = null;


            pagedResponseList = await _sKUBL.SelectAllSKUDetailsWebView(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
            return pagedResponseList == null ? NotFound() : CreateOkApiResponse(pagedResponseList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve SKUPrice Type  Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpGet]
    [Route("SelectSKUMasterByUID")]
    public async Task<ActionResult> SelectSKUMasterByUID(string UID)
    {
        try
        {
            Winit.Modules.SKU.Model.Interfaces.ISKUMaster sKUMasterList = await _sKUBL.SelectSKUMasterByUID(UID);

            return sKUMasterList != null ? CreateOkApiResponse(sKUMasterList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUMasterList with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
        }
    }
}

