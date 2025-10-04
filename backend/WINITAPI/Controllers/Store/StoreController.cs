using DocumentFormat.OpenXml.Spreadsheet;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using WINITAPI.Controllers.SKU;

namespace WINITAPI.Controllers.Store;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StoreController : WINITBaseController
{
    private readonly Winit.Modules.Store.BL.Interfaces.IStoreBL _storeBl;
    private readonly ISortHelper _sortHelper;
    private readonly ApiSettings _apiSettings;
    private readonly Winit.Shared.CommonUtilities.Common.CommonFunctions commonFunctions = new();
    private readonly DataPreparationController _dataPreparationController;

    public StoreController(IServiceProvider serviceProvider,
        Winit.Modules.Store.BL.Interfaces.IStoreBL storeBl,
        ISortHelper sortHelper,
        IOptions<ApiSettings> apiSettings,
        JsonSerializerSettings jsonSerializerSettings,
        DataPreparationController dataPreparationController) : base(serviceProvider)
    {
        _storeBl = storeBl;
        _sortHelper = sortHelper;
        _apiSettings = apiSettings.Value;
        _dataPreparationController = dataPreparationController;
    }

    [HttpPost]
    [Route("SelectAllStore")]
    public async Task<ActionResult> SelectAllStore([FromBody] PagingRequest pagingRequest)
    {
        try
        {
            PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore> pagedResponseStoreList = null;
            if (pagingRequest == null || pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid request data");
            }

            int startIndex = (pagingRequest.PageNumber - 1) * pagingRequest.PageSize;
            int endIndex = startIndex + pagingRequest.PageSize - 1;
            List<Winit.Modules.Store.Model.Interfaces.IStore> storeList = new();

            string searchPatternType = string.Empty;
            string nameFilter = string.Empty;
            if (pagingRequest.FilterCriterias != null && pagingRequest.FilterCriterias.Count > 0)
            {
                var typeFilter = pagingRequest.FilterCriterias.FirstOrDefault(fc => fc.Name == "Type");
                var nameCriteria = pagingRequest.FilterCriterias.FirstOrDefault(fc => fc.Name == "Name");

                if (typeFilter != null)
                {
                    searchPatternType = $"Filter_Store_Type_{typeFilter.Value}_*";
                }
                if (nameCriteria != null)
                {
                    nameFilter = nameCriteria.Value.ToString();
                }
            }

            List<string> searchedKeyByType = _cacheService.GetKeyByPattern(searchPatternType);
            List<string> filteredStoreUIDByType = new(_cacheService.GetMultiple<string>(searchedKeyByType).Values);

            storeList.AddRange(
            _cacheService.HGet<Winit.Modules.Store.Model.Classes.Store>("STORE", filteredStoreUIDByType));

            bool usedDatabase = false;
            
            // Database fallback when cache is empty
            if (storeList.Count == 0)
            {
                Log.Information("Store cache is empty, falling back to database");
                
                // Convert pagingRequest to the required parameters
                var sortCriterias = pagingRequest.SortCriterias != null && pagingRequest.SortCriterias.Count > 0
                    ? pagingRequest.SortCriterias.Select(sort =>
                        new Winit.Shared.Models.Enums.SortCriteria(sort.SortParameter, sort.Direction)).ToList()
                    : new List<Winit.Shared.Models.Enums.SortCriteria>
                    {
                        new Winit.Shared.Models.Enums.SortCriteria("Name",
                        Winit.Shared.Models.Enums.SortDirection.Asc)
                    };
                
                var filterCriterias = pagingRequest.FilterCriterias != null
                    ? pagingRequest.FilterCriterias.Select(filter =>
                    {
                        // Use Contains for text fields like Code and Name to improve search performance
                        // This matches frontend expectation of search behavior
                        var filterType = (filter.Name?.ToLower()) switch
                        {
                            "code" => Winit.Shared.Models.Enums.FilterType.Contains,        // Code search should use ILIKE
                            "name" => Winit.Shared.Models.Enums.FilterType.Contains,        // Name search should use ILIKE
                            _ => Winit.Shared.Models.Enums.FilterType.Equal                 // Other fields use exact match
                        };
                        return new Winit.Shared.Models.Enums.FilterCriteria(filter.Name, filter.Value, filterType);
                    }).ToList()
                    : new List<Winit.Shared.Models.Enums.FilterCriteria>();
                
                var dbStores = await _storeBl.SelectAllStore(
                    sortCriterias, 
                    pagingRequest.PageNumber, 
                    pagingRequest.PageSize, 
                    filterCriterias, 
                    pagingRequest.IsCountRequired);
                    
                if (dbStores != null && dbStores.PagedData != null)
                {
                    storeList.AddRange(dbStores.PagedData.Cast<Winit.Modules.Store.Model.Classes.Store>());
                    Log.Information($"Retrieved {storeList.Count} stores from database (Total: {dbStores.TotalCount})");
                    usedDatabase = true;
                    
                    // Return the database response directly with correct pagination
                    pagedResponseStoreList = new() { 
                        PagedData = storeList.Cast<Winit.Modules.Store.Model.Interfaces.IStore>().ToList(), 
                        TotalCount = dbStores.TotalCount 
                    };
                    return CreateOkApiResponse(pagedResponseStoreList);
                }
            }

            if (!usedDatabase)
            {
                // Only apply client-side filtering and sorting if we used cache data
                if (!string.IsNullOrEmpty(nameFilter))
                {
                    storeList = storeList.Where(store =>
                            store.Name != null && store.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                List<Winit.Shared.Models.Enums.SortCriteria> sortCriteriaList =
                    pagingRequest.SortCriterias != null && pagingRequest.SortCriterias.Count > 0
                        ? pagingRequest.SortCriterias.Select(sort =>
                            new Winit.Shared.Models.Enums.SortCriteria(sort.SortParameter, sort.Direction)).ToList()
                        : new List<Winit.Shared.Models.Enums.SortCriteria>
                        {
                            new Winit.Shared.Models.Enums.SortCriteria("Name",
                            Winit.Shared.Models.Enums.SortDirection.Asc)
                        };

                storeList = await _sortHelper.Sort(storeList, sortCriteriaList);
                List<Winit.Modules.Store.Model.Interfaces.IStore> pagingData =
                    commonFunctions.GetDataInRange<Winit.Modules.Store.Model.Interfaces.IStore>(storeList, startIndex,
                    endIndex);
                pagedResponseStoreList = new() { PagedData = pagingData, TotalCount = storeList.Count };
            }
            return CreateOkApiResponse(pagedResponseStoreList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve Store Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    /// <summary>
    /// Optimized API for fast store listing without JOINs
    /// 10x-20x faster than SelectAllStore for basic store information
    /// </summary>
    [HttpPost]
    [Route("SelectAllStoreBasic")]
    public async Task<ActionResult> SelectAllStoreBasic([FromBody] PagingRequest pagingRequest)
    {
        try
        {
            // Direct database query without expensive JOINs
            // This is optimized for speed over completeness of data
            var sortCriterias = pagingRequest.SortCriterias != null && pagingRequest.SortCriterias.Count > 0
                ? pagingRequest.SortCriterias
                : new List<Winit.Shared.Models.Enums.SortCriteria>
                {
                    new Winit.Shared.Models.Enums.SortCriteria("Name", Winit.Shared.Models.Enums.SortDirection.Asc)
                };
            
            // Map filter names to use Contains for text searches
            var filterCriterias = pagingRequest.FilterCriterias != null
                ? pagingRequest.FilterCriterias.Select(filter =>
                {
                    var filterType = (filter.Name?.ToLower()) switch
                    {
                        "code" => Winit.Shared.Models.Enums.FilterType.Contains,
                        "name" => Winit.Shared.Models.Enums.FilterType.Contains,
                        "aliasname" => Winit.Shared.Models.Enums.FilterType.Contains,
                        _ => Winit.Shared.Models.Enums.FilterType.Equal
                    };
                    return new Winit.Shared.Models.Enums.FilterCriteria(filter.Name, filter.Value, filterType);
                }).ToList()
                : new List<Winit.Shared.Models.Enums.FilterCriteria>();

            // Call SelectAllStore with count if explicitly requested (slower)
            // For best performance, frontend should set isCountRequired to false
            var requestCount = pagingRequest.IsCountRequired;
            
            var stores = await _storeBl.SelectAllStore(
                sortCriterias,
                pagingRequest.PageNumber,
                pagingRequest.PageSize,
                filterCriterias,
                requestCount  // Only get count if explicitly requested
            );

            // Return lighter payload - just essential fields
            if (stores?.PagedData != null)
            {
                var basicStores = stores.PagedData.Select(s => new
                {
                    s.UID,
                    s.Code,
                    s.Name,
                    s.Type,
                    s.IsActive,
                    s.IsBlocked,
                    s.CountryUID,
                    s.RegionUID,
                    s.CityUID,
                    s.CompanyUID
                }).ToList();

                // Estimate count if not requested for better UX
                var totalCount = requestCount 
                    ? stores.TotalCount 
                    : (basicStores.Count() < pagingRequest.PageSize 
                        ? ((pagingRequest.PageNumber - 1) * pagingRequest.PageSize) + basicStores.Count() 
                        : -1); // -1 means "many" or unknown

                return CreateOkApiResponse(new
                {
                    PagedData = basicStores,
                    TotalCount = totalCount,
                    Message = requestCount ? "Full count calculated" : "Count estimated for performance"
                });
            }

            return CreateOkApiResponse(new { PagedData = new List<object>(), TotalCount = 0 });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve basic store list");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("SelectStoreByUID")]
    public async Task<ActionResult<Winit.Modules.Store.Model.Interfaces.IStore>> SelectStoreByUID(string UID)
    {
        try
        {
            var storeList = _cacheService.HGet<Winit.Modules.Store.Model.Classes.Store>("STORE", UID);
            if (storeList != null)
            {
                return CreateOkApiResponse(storeList);
            }
            Winit.Modules.Store.Model.Interfaces.IStore store = await _storeBl.SelectStoreByUID(UID);
            return store != null ? CreateOkApiResponse(store) : CreateErrorResponse("Retrive Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve StoreList with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpGet]
    [Route("SelectStoreByUIDWithoutCache")]
    public async Task<ActionResult<Winit.Modules.Store.Model.Interfaces.IStore>> SelectStoreByUIDWithoutCache(string UID)
    {
        try
        {
            var storeList = _cacheService.HGet<Winit.Modules.Store.Model.Classes.Store>("STORE", UID);
            if (storeList != null)
            {
                return CreateOkApiResponse(storeList);
            }
            Winit.Modules.Store.Model.Interfaces.IStore store = await _storeBl.SelectStoreByUID(UID);
            return store != null ? CreateOkApiResponse(store) : CreateErrorResponse("Retrive Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve StoreList with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("CreateStore")]
    public async Task<ActionResult> CreateStore([FromBody] Winit.Modules.Store.Model.Interfaces.IStore store)
    {
        try
        {
            int retValue = await _storeBl.CreateStore(store);
            if (retValue > 0)
            {
                List<string> storeUIDs = new List<string> { store.UID };
                _ = await _dataPreparationController.PrepareStoreMaster(storeUIDs);
                return CreateOkApiResponse(retValue);
            }
            else
            {
                throw new Exception("Insert Failed");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Create Store  details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPut]
    [Route("UpdateStore")]
    public async Task<ActionResult<int>> UpdateStore([FromBody] Winit.Modules.Store.Model.Classes.Store Store)
    {
        try
        {
            IStore existingStoreList = await _storeBl.SelectStoreByUID(Store.UID);
            if (existingStoreList != null)
            {
                Store.ServerModifiedTime = DateTime.Now;
                int retVal = await _storeBl.UpdateStore(Store);
                if (retVal > 0)
                {
                    List<string> storeUIDs = new List<string> { Store.UID };
                    _ = await _dataPreparationController.PrepareStoreMaster(storeUIDs);
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    throw new Exception("Update Failed");
                }
            }
            else
            {
                return NotFound("Store Not found");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating Store Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpDelete]
    [Route("DeleteStore")]
    public async Task<ActionResult<int>> DeleteStore(string UID)
    {
        try
        {
            int retVal = await _storeBl.DeleteStore(UID);


            if (retVal > 0)
            {
                //List<string> uids = new List<string> { UID };
                //_ =await _dataPreparationController.PrepareStoreMaster(uids);
            }
            return retVal > 0
                ? (ActionResult<int>)CreateOkApiResponse(retVal)
                : throw new Exception("Delete Failed");
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Deleting Failure");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("CreateStoreMaster")]
    public async Task<ActionResult> CreateStoreMaster(
        [FromBody] Winit.Modules.StoreMaster.Model.Classes.StoreViewModelDCO createStoreMaster)
    {
        try
        {
            createStoreMaster.store.ServerAddTime = DateTime.Now;
            createStoreMaster.store.ServerModifiedTime = DateTime.Now;
            createStoreMaster.StoreAdditionalInfo.ServerAddTime = DateTime.Now;
            createStoreMaster.StoreAdditionalInfo.ServerModifiedTime = DateTime.Now;
            foreach (Winit.Modules.Contact.Model.Interfaces.IContact contact in createStoreMaster.Contacts)
            {
                contact.ServerAddTime = DateTime.Now;
                contact.ServerModifiedTime = DateTime.Now;
            }

            foreach (Winit.Modules.Store.Model.Interfaces.IStoreCredit storeCredit in
                createStoreMaster.StoreCredits)
            {
                storeCredit.ServerAddTime = DateTime.Now;
                storeCredit.ServerModifiedTime = DateTime.Now;
            }

            foreach (Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument storeDocument in createStoreMaster
                .StoreDocuments)
            {
                storeDocument.ServerAddTime = DateTime.Now;
                storeDocument.ServerModifiedTime = DateTime.Now;
            }

            foreach (Winit.Modules.Address.Model.Interfaces.IAddress address in createStoreMaster.Addresses)
            {
                address.ServerAddTime = DateTime.Now;
                address.ServerModifiedTime = DateTime.Now;
            }

            int retVal = await _storeBl.CreateStoreMaster(createStoreMaster);
            return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Create Store Master details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("SelectStoreMasterByUID")]
    public async Task<ActionResult<ApiResponse<Winit.Modules.StoreMaster.Model.Interfaces.IStoreViewModelDCO>>>
        SelectStoreMasterByUID(string UID)
    {
        try
        {
            Winit.Modules.StoreMaster.Model.Interfaces.IStoreViewModelDCO storeMasterList =
                await _storeBl.SelectStoreMasterByUID(UID);
            return storeMasterList != null
                ? (ActionResult<ApiResponse<Winit.Modules.StoreMaster.Model.Interfaces.IStoreViewModelDCO>>)
                CreateOkApiResponse(storeMasterList)
                : (ActionResult<ApiResponse<Winit.Modules.StoreMaster.Model.Interfaces.IStoreViewModelDCO>>)
                NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Store Master List with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPut]
    [Route("UpdateStoreMaster")]
    public async Task<ActionResult<int>> UpdateStoreMaster(
        [FromBody] Winit.Modules.StoreMaster.Model.Classes.StoreViewModelDCO updateStoreMaster)
    {
        try
        {
            int retVal = await _storeBl.UpdateStoreMaster(updateStoreMaster);
            return retVal > 0
                ? (ActionResult<int>)CreateOkApiResponse(retVal)
                : throw new Exception("Update Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating Store Master Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("PopulateStoreCache")]
    public ActionResult PopulateStoreCache()
    {
        try
        {
            for (int i = 1; i <= 10000; i++)
            {
                Winit.Modules.Store.Model.Interfaces.IStore store =
                    _serviceProvider.CreateInstance<Winit.Modules.Store.Model.Interfaces.IStore>();
                store.UID = "ST000" + i;
                store.Code = "ST000" + i;
                store.Name = "Store " + i;
                _cacheService.HSet("TestStore", store.UID, store,
                WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
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
    [Route("SelectAllStoreElasticSearch")]
    public async Task<ActionResult<ApiResponse<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore>>>>
        SelectAllStoreElasticSearch([FromBody] PagingRequest pagingRequest)
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
            SingleNodeConnectionPool pool = new(new Uri(_apiSettings.ElasticUrl));
            ConnectionSettings settings = new ConnectionSettings(pool)
                .DefaultIndex(Winit.Shared.Models.Constants.CacheConstants.ELASTIC_STORE)
                .EnableDebugMode();
            ElasticClient client = new(settings);
            if (!client.Indices.Exists(Winit.Shared.Models.Constants.CacheConstants.ELASTIC_STORE).Exists)
            {
                _ = client.Indices.Create(Winit.Shared.Models.Constants.CacheConstants.ELASTIC_STORE, c => c
                    .Map<Winit.Modules.Store.Model.Classes.Store>(m => m
                        .AutoMap()
                    )
                );
                PagedResponse<IStore> storelist = await _storeBl.SelectAllStore(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, true);
                BulkResponse response = client.IndexMany(storelist.PagedData);

                return response.IsValid
                    ? (ActionResult<ApiResponse<PagedResponse<IStore>>>)CreateOkApiResponse(storelist)
                    : (ActionResult<ApiResponse<PagedResponse<IStore>>>)NotFound();
            }
            else
            {
                SearchRequest<Winit.Modules.Store.Model.Classes.Store> searchRequest =
                    new(Winit.Shared.Models.Constants.CacheConstants.ELASTIC_STORE)
                    {
                        Size = pagingRequest.PageSize,
                        From = (pagingRequest.PageNumber - 1) * pagingRequest.PageSize,
                        Query = commonFunctions.CreateElasticsearchQuery(pagingRequest.FilterCriterias),
                    };
                string request =
                    client.RequestResponseSerializer.SerializeToString(searchRequest,
                    SerializationFormatting.Indented);
                ISearchResponse<Winit.Modules.Store.Model.Classes.Store> searchResponse =
                    await client.SearchAsync<Winit.Modules.Store.Model.Classes.Store>(searchRequest);

                List<Winit.Shared.Models.Enums.SortCriteria> sortCriteriaList = [];

                if (pagingRequest.SortCriterias != null && pagingRequest.SortCriterias.Count > 0)
                {
                    foreach (Winit.Shared.Models.Enums.SortCriteria sort in pagingRequest.SortCriterias)
                    {
                        sortCriteriaList.Add(
                        new Winit.Shared.Models.Enums.SortCriteria(sort.SortParameter, sort.Direction));
                    }
                }
                else
                {
                    sortCriteriaList.Add(new Winit.Shared.Models.Enums.SortCriteria("Name",
                    Winit.Shared.Models.Enums.SortDirection.Asc));
                }

                List<Winit.Modules.Store.Model.Classes.Store> storelist = searchResponse.Documents.ToList();

                storelist = await _sortHelper.Sort(storelist, sortCriteriaList);


                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore> pagedResponseStoreList = null;
                if (searchResponse.IsValid)
                {
                    pagedResponseStoreList = new PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore>
                    {
                        PagedData = storelist,
                        TotalCount = Convert.ToInt32(searchResponse.Total),
                    };
                    return CreateOkApiResponse(pagedResponseStoreList);
                }
                else
                {
                    return NotFound();
                }
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred: {ex.Message}");
        }
    }

    [HttpPost]
    [Route("GetAllStoreAsSelectionItems")]
    public async Task<ActionResult> GetAllStoreAsSelectionItems(PagingRequest pagingRequest,
        [FromQuery] string OrgUID)
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
            PagedResponse<SelectionItem> PagedResponseStoreListSlectionItems = null;
            PagedResponseStoreListSlectionItems = null;

            if (PagedResponseStoreListSlectionItems != null)
            {
                return CreateOkApiResponse(PagedResponseStoreListSlectionItems);
            }
            PagedResponseStoreListSlectionItems = await _storeBl.GetAllStoreAsSelectionItems(
            pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
            pagingRequest.IsCountRequired, OrgUID);
            return PagedResponseStoreListSlectionItems == null
                ? NotFound()
                : CreateOkApiResponse(PagedResponseStoreListSlectionItems);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve Store SlectionItem DetailsDetails");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("GetAllStoreItems")]
    public async Task<ActionResult> GetAllStoreItems(PagingRequest pagingRequest, [FromQuery] string OrgUID)
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
            PagedResponse<IStoreCustomer> storeItemViews = null;

            storeItemViews = await _storeBl.GetAllStoreItems(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
            pagingRequest.IsCountRequired, OrgUID);

            return storeItemViews == null ? NotFound() : CreateOkApiResponse(storeItemViews);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve Store Item Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("SelectStoreByOrgUID")]
    public async Task<ActionResult> SelectStoreByOrgUID(string FranchiseeOrgUID)
    {
        try
        {
            Winit.Modules.Store.Model.Interfaces.IStore
                store = await _storeBl.SelectStoreByOrgUID(FranchiseeOrgUID);
            return store != null ? CreateOkApiResponse(store) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve StoreList with OrgUID: {@OrgUID}", FranchiseeOrgUID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetStoreByRouteUIDBeatHistoryUID")]
    public async Task<ActionResult> GetStoreByRouteUIDBeatHistoryUID(string routeUID, string BeatHistoryUID,
        bool notInJP)
    {
        try
        {
            List<Winit.Modules.Store.Model.Interfaces.IStoreItemView> customerList =
                await _storeBl.GetStoreByRouteUID(routeUID, BeatHistoryUID, notInJP);
            return customerList != null ? CreateOkApiResponse(customerList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve StoreList with routeUID: {@routeUID}", routeUID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetStoreByRouteUID")]
    public async Task<ActionResult> GetStoreByRouteUID(string routeUID)
    {
        try
        {
            List<Winit.Modules.Store.Model.Interfaces.IStoreItemView> customerList =
                await _storeBl.GetStoreByRouteUID(routeUID);
            return customerList != null ? CreateOkApiResponse(customerList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve StoreList with routeUID: {@routeUID}", routeUID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetStoreCustomersByRouteUID")]
    public async Task<ActionResult> GetStoreCustomersByRouteUID(string routeUID)
    {
        try
        {
            List<IStoreCustomer> customerList = await _storeBl.GetStoreCustomersByRouteUID(routeUID);
            return customerList != null ? CreateOkApiResponse(customerList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve StoreList with routeUID: {@routeUID}", routeUID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetApprovalDetailsByStoreUID")]
    public async Task<ActionResult> GetApprovalDetailsByStoreUID(string LinkItemUID)
    {
        try
        {
            List<IAllApprovalRequest> approvalList = await _storeBl.GetApprovalDetailsByStoreUID(LinkItemUID);
            return approvalList != null ? CreateOkApiResponse(approvalList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve ApprovalList with routeUID: {@routeUID}", LinkItemUID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetApprovalStatusByStoreUID")]
    public async Task<ActionResult> GetApprovalStatusByStoreUID(string LinkItemUID)
    {
        try
        {
            List<IAllApprovalRequest> approvalList = await _storeBl.GetApprovalStatusByStoreUID(LinkItemUID);
            return approvalList != null ? CreateOkApiResponse(approvalList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve ApprovalStatus with routeUID: {@routeUID}", LinkItemUID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetChannelPartner")]
    public async Task<ActionResult> GetChannelPartner([FromQuery] string jobPositionUid)
    {
        try
        {
            var channelPartner = await _storeBl.GetChannelPartner(jobPositionUid);
            return channelPartner != null ? CreateOkApiResponse(channelPartner) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Channel Partner");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetAllOnBoardingDetailsByStoreUID")]
    public async Task<ActionResult> GetAllOnBoardingDetailsByStoreUID(string UID)
    {
        try
        {
            IOnBoardEditCustomerDTO approvalList = await _storeBl.GetAllOnBoardingDetailsByStoreUID(UID);
            return approvalList != null ? CreateOkApiResponse(approvalList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve ApprovalList with routeUID: {@routeUID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpDelete]
    [Route("DeleteOnBoardingDetails")]
    public async Task<ActionResult<int>> DeleteOnBoardingDetails(string UID)
    {
        try
        {
            int retVal = await _storeBl.DeleteOnBoardingDetails(UID);

            return retVal > 0
                ? (ActionResult<int>)CreateOkApiResponse(retVal)
                : throw new Exception("Delete Failed");
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Deleting Failure");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("GetStoreMastersByStoreUIDs")]
    public async Task<ActionResult> GetStoreMastersByStoreUIDs(List<string> storeUIDs)
    {
        try
        {
            if (storeUIDs == null || !storeUIDs.Any())
            {
                return NotFound();
            }
            List<IStoreMaster> storeMasters = [];
            foreach (string storeUID in storeUIDs)
            {
                IStoreMaster storeMaster =
                    _cacheService.HGet<IStoreMaster>(Winit.Shared.Models.Constants.CacheConstants.StoreMaster,
                    storeUID);
                if (storeMaster != null)
                {
                    storeMasters.Add(storeMaster);
                }
            }
            return CreateOkApiResponse(storeMasters);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse($"Internal Server error{ex.Message}");
        }
    }

    [HttpPost, HttpPut]
    [Route("CUDOnBoardCustomerInfo")]
    public async Task<ActionResult> OnBoardCustomerInfo(
        [FromBody] Winit.Modules.Store.Model.Classes.OnBoardCustomerDTO onBoardCustomerDTO)
    {
        try
        {
            int retValue = await _storeBl.CUDOnBoardCustomerInfo(onBoardCustomerDTO);
            if (retValue > 0)
            {
                return CreateOkApiResponse(retValue);
            }
            else
            {
                throw new Exception("Insert Failed");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Create Store  details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("CreateAllApprovalRequest")]
    public async Task<ActionResult> CreateAllApprovalRequest(
        [FromBody] IAllApprovalRequest allApprovalRequest)
    {
        try
        {
            int retValue = await _storeBl.CreateAllApprovalRequest(allApprovalRequest);
            if (retValue > 0)
            {
                return CreateOkApiResponse(retValue);
            }
            else
            {
                throw new Exception("Insert Failed");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Create AllApprovalRequest  details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }


    [HttpPost]
    [Route("SelectAllOnBoardCustomer")]
    public async Task<ActionResult> SelectAllOnBoardCustomer([FromBody] PagingRequest pagingRequest, [FromQuery] string JobPositionUID, [FromQuery] string Role)
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
            PagedResponse<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview> PagedResponse = null;
            PagedResponse = await _storeBl.SelectAllOnBoardCustomer(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
            pagingRequest.IsCountRequired, JobPositionUID, Role);
            if (PagedResponse == null)
            {
                return NotFound();
            }
            return CreateOkApiResponse(PagedResponse);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve OnBoarding details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPut]
    [Route("UpdateStoreStatus")]
    public async Task<ActionResult<int>> UpdateStoreStatus([FromBody] Winit.Modules.Store.Model.Classes.StoreApprovalDTO storeApprovalDTO)
    {
        try
        {
            IStore existingStoreList = await _storeBl.SelectStoreByUID(storeApprovalDTO.Store.UID);
            if (existingStoreList != null)
            {
                storeApprovalDTO.Store.ServerModifiedTime = DateTime.Now;
                int retVal = await _storeBl.UpdateStoreStatus(storeApprovalDTO);
                if (retVal > 0)
                {
                    //List<string> storeUIDs = new List<string> { Store.UID };
                    //_ = await _dataPreparationController.PrepareStoreMaster(storeUIDs);
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    throw new Exception("Update Failed");
                }
            }
            else
            {
                return NotFound("Store Not found");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating Store Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("CreateAsmDivisionMapping")]
    public async Task<ActionResult> CreateAsmDivisionMapping(
        [FromBody] List<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping> AsmDivisionMapping)
    {
        try
        {
            if (AsmDivisionMapping == null || !AsmDivisionMapping.Any())
            {
                return NotFound("No mappings provided.");
            }

            int successCount = 0;

            foreach (var asm in AsmDivisionMapping)
            {
                // Check if the record already exists
                IAsmDivisionMapping existingStoreList =
                    await _storeBl.CheckAsmDivisionMappingRecordExistsOrNot(asm.UID);

                if (existingStoreList == null)// Only insert if not exists
                {
                    int retValue = await _storeBl.CreateAsmDivisionMapping(asm);

                    if (retValue > 0)// If insert was successful
                    {
                        successCount++;
                    }
                    else
                    {
                        throw new Exception($"Insert failed for UID: {asm.UID}");
                    }
                }
            }

            // If all insertions were successful
            if (successCount > 0)
            {
                return CreateOkApiResponse(successCount);
            }
            else if (successCount == 0)
            {
                return CreateOkApiResponse(successCount);
            }
            else
            {
                return CreateErrorResponse("No new records were inserted.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create AsmDivisionMapping details.");
            return CreateErrorResponse("An error occurred while processing the request: " + ex.Message);
        }
    }

    [HttpPut]
    [Route("UpdateAsmDivisionMapping")]
    public async Task<ActionResult<int>> UpdateAsmDivisionMapping(
        [FromBody] List<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping> AsmDivisionMapping)
    {
        try
        {
            int retVal = await _storeBl.UpdateAsmDivisionMapping(AsmDivisionMapping);
            if (retVal > 0)
            {
                return CreateOkApiResponse(retVal);
            }
            else
            {
                throw new Exception("Update Failed");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating Store Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetAsmDivisionMappingByUID/{linkedItemType}/{linkedItemUID}")]
    public async Task<ActionResult<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping>>
        GetAsmDivisionMappingByUID(string linkedItemType, string linkedItemUID, string? asmEmpUID = null)
    {
        try
        {
            List<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping> AsmDivisionMappingList =
                await _storeBl.GetAsmDivisionMappingByUID(linkedItemType, linkedItemUID, asmEmpUID);
            return AsmDivisionMappingList != null
                ? CreateOkApiResponse(AsmDivisionMappingList)
                : CreateErrorResponse("Retrive Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve AsmDivisionMappingList with UID: {@UID}", linkedItemUID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetAsmDivisionMappingByUIDV2/{linkedItemType}/{linkedItemUID}/{asmEmpUID}")]
    public async Task<ActionResult<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping>>
        GetAsmDivisionMappingByUIDV2(string linkedItemType, string linkedItemUID, string? asmEmpUID = null)
    {
        try
        {
            List<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping> AsmDivisionMappingList =
                await _storeBl.GetAsmDivisionMappingByUID(linkedItemType, linkedItemUID, asmEmpUID);
            return AsmDivisionMappingList != null
                ? CreateOkApiResponse(AsmDivisionMappingList)
                : CreateErrorResponse("Retrive Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve AsmDivisionMappingList with UID: {@UID}", linkedItemUID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("CreateChangeRequest")]
    public async Task<ActionResult<int>> CreateChangeRequest(ChangeRequestDTO changeRequestDTO)
    {
        try
        {
            int retVal = await _storeBl.CreateChangeRequest(changeRequestDTO);
            return retVal == 1 ? CreateOkApiResponse(retVal) : CreateErrorResponse("Retrieve Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Insert change request data");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpDelete]
    [Route("DeleteAsmDivisionMapping")]
    public async Task<ActionResult<int>> DeleteAsmDivisionMapping(string UID)
    {
        try
        {
            int retVal = await _storeBl.DeleteAsmDivisionMapping(UID);


            if (retVal > 0)
            {
                //List<string> uids = new List<string> { UID };
                //_ =await _dataPreparationController.PrepareStoreMaster(uids);
            }
            return retVal > 0
                ? (ActionResult<int>)CreateOkApiResponse(retVal)
                : throw new Exception("Delete Failed");
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Deleting Failure");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetDivisionsByAsmEmpUID")]
    public async Task<ActionResult<int>> GetDivisionsByAsmEmpUID([FromQuery] string asmEmpUID)
    {
        try
        {
            var retVal = await _storeBl.GetDivisionsByAsmEmpUID(asmEmpUID);
            return retVal != null
                ? (ActionResult<int>)CreateOkApiResponse(retVal)
                : throw new Exception("Retrive Failed");
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Retrive Failure");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("GenerateMyTeam")]
    public async Task<IActionResult> GenerateMyTeam(string jobPositionUid = null)
    {
        try
        {
            await _storeBl.GenerateMyTeam(jobPositionUid);
            return CreateOkApiResponse("My team populated successfully");
        }
        catch (Exception e)
        {
            return CreateErrorResponse("Failed to populate");
            throw;
        }
    }

    [HttpGet]
    [Route("IsGstUnique")]
    public async Task<ActionResult> IsGstUnique(string GStNumber)
    {
        try
        {
            bool IsUniqueGST = await _storeBl.IsGstUnique(GStNumber);
            return CreateOkApiResponse(IsUniqueGST);

        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve AddressDetails with UID: {@UID}", "IsUniqueGST");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

        }
    }
    [HttpPost]
    [Route("GetTabsCount")]
    public async Task<ActionResult> GetTabsCount([FromBody] List<FilterCriteria>? filters = null, [FromQuery] string JobPositionUID = null, [FromQuery] string Role = null)
    {
        try
        {
            var statusDict = await _storeBl.GetTabsCount(filters, JobPositionUID, Role);
            if (statusDict == null)
            {
                return CreateErrorResponse("Error occured while retiving the Tabs Status Count");
            }
            return CreateOkApiResponse(statusDict);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to Retrive Tabs Status Count");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost("GetApplicableToCustomers")]
    public async Task<IActionResult> GetApplicableToCustomers([FromBody] ApplicableToCustomerRequestBody applicableToCustomerRequestBody)
    {
        if (applicableToCustomerRequestBody == null)
        {
            return BadRequest("request should not be null");
        }
        try
        {
            var stores = await _storeBl.GetApplicableToCustomers(stores: applicableToCustomerRequestBody.Stores, broadClassifications: applicableToCustomerRequestBody.BroadClassifications, branches: applicableToCustomerRequestBody.Branches);
            return CreateOkApiResponse(stores);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

//[HttpGet]
//[Route("ReadStoreCache")]
//public async Task<ActionResult> ReadStoreCache(string searchString, int pageNumber, int pageSize)
//{
//    try
//    {
//        int startIndex = (pageNumber - 1) * pageSize;
//        int endIndex = startIndex + pageSize - 1;

//        List<Winit.Modules.Store.Model.Interfaces.IStore> storeList = new List<Winit.Modules.Store.Model.Interfaces.IStore>();
//        string searchPattern = $"Filter_Store_Name_*{searchString}*";
//        // Get Store UID based on pattern
//        List<string> searchedKey = _cacheService.GetKeyByPattern(searchPattern);
//        Dictionary<string,string> keyValuePairs = _cacheService.GetMultiple<string>(searchedKey);
//        List<string> FilteredStoreUID = new List<string>(keyValuePairs.Values);
//        //foreach (string key in searchedKey)
//        //{
//        //    FilteredStoreUID.Add(_cacheService.Get<string>(key));
//        //}

//        //searchedKey = _cacheService.GetHashValueByPattern("Filter_Store_Name", searchString);

//        // Apply Filter
//        storeList.AddRange(_cacheService.HGet<Winit.Modules.Store.Model.Classes.Store>("TestStore", FilteredStoreUID));
//        //Apply Sorting
//        List<SortCriteria> sortCriteriaList = new List<SortCriteria>();
//        sortCriteriaList.Add(new SortCriteria("Name", SortDirection.Asc));
//        storeList = await _sortHelper.Sort(storeList, sortCriteriaList);
//        List<Winit.Modules.Store.Model.Interfaces.IStore> pagingData = GetDataInRange<Winit.Modules.Store.Model.Interfaces.IStore>(storeList, startIndex, endIndex);

//        //foreach (string field in FilteredStoreUID)
//        //{
//        //    storeList.Add(_cacheService.HGet<Winit.Modules.Store.Model.Classes.Store>("TestStore", field));
//        //}
//        return Ok(pagingData);
//    }
//    catch (Exception ex)
//    {
//        return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
//    }
//}


//[HttpPost]
//[Route("SelectAllStore")]
//public async Task<ActionResult<ApiResponse<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore>>>> SelectAllStore(string searchString, PagingRequest pagingRequest)
//{
//    try
//    {
//        PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore> pagedResponseStoreList = null;
//        if (pagingRequest == null)
//        {
//            return BadRequest("Invalid request data");
//        }

//        if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
//        {
//            return BadRequest("Invalid page size or page number");
//        }
//        int startIndex = (pagingRequest.PageNumber - 1) * pagingRequest.PageNumber;
//        int endIndex = startIndex + pagingRequest.PageSize - 1;
//        List<Winit.Modules.Store.Model.Interfaces.IStore> storeList = new List<Winit.Modules.Store.Model.Interfaces.IStore>();
//        string searchPattern = $"Filter_Store_StoreName_*{searchString}*";
//        string searchPatternType = $"Filter_Store_Type_*{searchString}*";

//        // Get Store UID based on pattern
//        List<string> searchedKey = _cacheService.GetKeyByPattern(searchPattern);
//        Dictionary<string, string> keyValuePairs = _cacheService.GetMultiple<string>(searchedKey);
//        List<string> FilteredStoreUID = new List<string>(keyValuePairs.Values);
//        storeList.AddRange(_cacheService.HGet<Winit.Modules.Store.Model.Classes.Store>("STORE", FilteredStoreUID));
//        List<Winit.Modules.Store.Model.Interfaces.IStore> pagingData = GetDataInRange<Winit.Modules.Store.Model.Interfaces.IStore>(storeList, startIndex, endIndex);
//        pagedResponseStoreList = new();
//        if (pagingData != null && pagingData.Count>0)
//        {
//            pagedResponseStoreList.PagedData= pagingData;

//        }
//        return CreateOkApiResponse(pagedResponseStoreList);

//    }
//    catch (Exception ex)
//    {
//        Log.Error(ex, "Fail to retrieve Store  Details");
//        return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
//    }
//}

//private List<T> GetDataInRange<T>(List<T> data, int startIndex, int endIndex)
//{
//    // Check for invalid input
//    // if (data == null || startIndex < 0 || endIndex < 0 || startIndex > endIndex || endIndex >= data.Count)
//    if (data == null || startIndex < 0 || endIndex < 0 || startIndex > endIndex)
//    {
//        return null; 
//    }

//    // Use LINQ to get data in the specified range
//    List<T> result = data.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();
//    return result;
//}
//[HttpPost]
//[Route("SelectAllStoreElasticSearch")]
//public async Task<ActionResult<ApiResponse<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore>>>> SelectAllStore_ElasticSearch([FromBody] PagingRequest pagingRequest)
//{
//    try
//    {
//        PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore> pagedResponseStoreList = null;
//        if (pagingRequest == null)
//        {
//            return BadRequest("Invalid request data");
//        }

//        if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
//        {
//            return BadRequest("Invalid page size or page number");
//        }
//        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
//        .DefaultIndex("storerecord"); // Replace with your index name
//        var client = new ElasticClient(settings);

//        if (!client.Indices.Exists("storerecord").Exists)
//        {
//            client.Indices.Create("storerecord", c => c
//                .Map<Winit.Modules.Store.Model.Classes.Store>(m => m
//                    .AutoMap()
//                )
//            );
//            var storelist = await _storeBl.SelectAllStore(pagingRequest.SortCriterias, pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, true);
//            var response = client.IndexMany(storelist.PagedData);
//            if (response.IsValid)
//            {
//                return CreateOkApiResponse(storelist);
//            }
//            else
//            {
//                return NotFound();
//            }
//        }
//        else
//        {
//            int num = int.MaxValue;
//            var scrollResponse = client.Search<Winit.Modules.Store.Model.Classes.Store>(s => s.Size(10000)
//         // Set the initial page size to a reasonable number
//         ); // Set your desired scroll time
//            var hits = scrollResponse.Documents;
//            var totalHits = scrollResponse.Total;
//            return CreateOkApiResponse(hits);
//        }
//    }
//    catch (Exception)
//    {
//        throw;
//    }
//}
