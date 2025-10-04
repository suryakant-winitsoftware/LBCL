using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using WINITSharedObjects.Models;

namespace WINITAPI.Controllers.Org
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrgController : WINITBaseController
    {
        private readonly Winit.Modules.Org.BL.Interfaces.IOrgBL _orgBLservice;
        public OrgController(IServiceProvider serviceProvider, Winit.Modules.Org.BL.Interfaces.IOrgBL orgBLservice) : base(serviceProvider)
        {
            _orgBLservice = orgBLservice;
        }
        [HttpPost]
        [Route("GetOrgDetails")]
        public async Task<ActionResult> GetOrgDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrg> PagedResponseOrgList = null;
                PagedResponseOrgList = await _orgBLservice.GetOrgDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseOrgList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseOrgList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve OrgDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetOrgByUID")]
        public async Task<ActionResult> GetOrgByUID(string UID)
        {
            try
            {
                // Try to get from cache with application-level timeout
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    var cacheTask = Task.Run(async () => 
                    {
                        try 
                        {
                            return _cacheService.HGet<Winit.Modules.Org.Model.Classes.Org>("ORG", UID);
                        }
                        catch (OperationCanceledException)
                        {
                            return null;
                        }
                    }, cts.Token);
                    
                    var CachedData = await cacheTask.ConfigureAwait(false);
                    if (CachedData != null)
                    {
                        return CreateOkApiResponse(CachedData);
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Warning("Cache operation timed out after 3 seconds for UID: {@UID}", UID);
                }
                catch (Exception cacheEx)
                {
                    // Log cache error but continue with database fetch
                    Log.Warning(cacheEx, "Cache service unavailable, falling back to database for UID: {@UID}", UID);
                }

                // Fetch from database
                Winit.Modules.Org.Model.Interfaces.IOrg OrgDetails = await _orgBLservice.GetOrgByUID(UID);
                if (OrgDetails != null)
                {
                    // Try to cache the result with timeout, but don't block response
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                            await Task.Run(() => _cacheService.HSet("ORG", UID, OrgDetails, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120), cts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            Log.Warning("Cache set operation timed out after 2 seconds for UID: {@UID}", UID);
                        }
                        catch (Exception cacheEx)
                        {
                            Log.Warning(cacheEx, "Failed to cache organization data for UID: {@UID}", UID);
                        }
                    });

                    return CreateOkApiResponse(OrgDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve OrgDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("CreateOrg")]
        public async Task<ActionResult> CreateOrg([FromBody] Winit.Modules.Org.Model.Classes.Org createOrg)
        {
            try
            {
                createOrg.ServerAddTime = DateTime.Now;
                createOrg.ServerModifiedTime = DateTime.Now;
                var retVal = await _orgBLservice.CreateOrg(createOrg);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Org details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateOrgBulk")]
        public async Task<ActionResult> CreateOrgBulk([FromBody] List<Winit.Modules.Org.Model.Interfaces.IOrg> createOrg)
        {
            try
            {
                var retVal = await _orgBLservice.CreateOrgBulk(createOrg);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Org details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateOrg")]
        public async Task<ActionResult> UpdateOrg([FromBody] Winit.Modules.Org.Model.Classes.Org updateOrg)
        {
            try
            {
                var existingDetails = await _orgBLservice.GetOrgByUID(updateOrg.UID);
                if (existingDetails != null)
                {
                    updateOrg.ServerModifiedTime = DateTime.Now;
                    var retVal = await _orgBLservice.UpdateOrg(updateOrg);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Org Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteOrg")]
        public async Task<ActionResult> DeleteOrg([FromQuery] string UID)
        {
            try
            {
                var retVal = await _orgBLservice.DeleteOrg(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("InsertOrgHierarchy")]
        public async Task<ActionResult> InsertOrgHierarchy()
        {
            try
            {
                var retVal = await _orgBLservice.InsertOrgHierarchy();
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create OrgHierarchy details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("PrepareOrgMaster")]
        public async Task<ActionResult<Winit.Modules.Org.Model.Interfaces.IOrg>> PrepareOrgMaster()
        {
            try
            {

                var orgList = await _orgBLservice.PrepareOrgMaster();
                if (orgList == null)
                {
                    return NotFound();
                }
                
                // Try to cache but don't fail if Redis is unavailable
                try
                {
                    foreach (var org in orgList)
                    {
                        Winit.Modules.Org.Model.Classes.Org obj = new Winit.Modules.Org.Model.Classes.Org();
                        obj = (Winit.Modules.Org.Model.Classes.Org)org;
                        if (obj != null)
                        {
                            _cacheService.HSet("ORG", org.UID, obj, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                        }
                    }
                    return CreateOkApiResponse("Success - Data cached");
                }
                catch (Exception cacheEx)
                {
                    Log.Warning(cacheEx, "Cache service unavailable, but master data was prepared successfully");
                    return CreateOkApiResponse("Success - Cache unavailable but data prepared");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to prepare Org Master data");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpPost]
        [Route("ViewFranchiseeWarehouse")]
        public async Task<ActionResult> ViewFranchiseeWarehouse(PagingRequest pagingRequest, [FromQuery] string FranchiseeOrgUID)
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

                PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView> PagedResponseOrgList = null;
                PagedResponseOrgList = await _orgBLservice.ViewFranchiseeWarehouse(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, FranchiseeOrgUID);
                if (PagedResponseOrgList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseOrgList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Warehouse");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("ViewFranchiseeWarehouseByUID")]
        public async Task<ActionResult> ViewFranchiseeWarehouseByUID(string UID)
        {
            try
            {
                Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView data = await _orgBLservice.ViewFranchiseeWarehouseByUID(UID);
                if (data != null)
                {
                    return CreateOkApiResponse(data);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Warehouse and Addresses details with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("GetAllWareHouseStock")]
        public async Task<ActionResult> GetAllWareHouseStock(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Org.Model.Interfaces.IWareHouseStock> PagedResponseOrgList = null;
                PagedResponseOrgList = await _orgBLservice.GetAllWareHouseStock(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseOrgList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseOrgList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve OrgDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("GetWarehouseStockDetails")]
        public async Task<ActionResult> GetWarehouseStockDetails(PagingRequest pagingRequest, string FranchiseeOrgUID, string WarehouseUID, string StockType)
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
                PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> PagedResponseWareHouseStockList = null;
                PagedResponseWareHouseStockList = await _orgBLservice.GetWarehouseStockDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, FranchiseeOrgUID, WarehouseUID, StockType);

                foreach (var item in PagedResponseWareHouseStockList.PagedData)
                {
                    BuyPrice buyPrice = (_cacheService.HGet<Winit.Modules.SKU.Model.Classes.BuyPrice>($"BuyPrice_{FranchiseeOrgUID}", item.SKUCode));


                    if (buyPrice != null)
                    {
                        item.CostPrice = buyPrice.Price;
                        item.TotalCost = buyPrice.Price * item.EAQty;
                        item.Net = item.TotalEAQty * item.CostPrice;
                    }
                    if (StockType == "FOC")
                    {
                        IEnumerable<IFOCStockItem> fOCStockReservedItems = await _orgBLservice.GetFOCStockItemDetails(StockType);
                        if (fOCStockReservedItems != null)
                        {
                            IFOCStockItem fOCStockItem = fOCStockReservedItems.Where(e => e.ItemUID == item.SKUCode)
                                .FirstOrDefault();
                            if (fOCStockItem != null)
                            {
                                item.ReservedOUQty = fOCStockItem.ReservedOUQty;
                                item.ReservedBUQty = fOCStockItem.ReservedBUQty;
                            }
                        }
                    }


                }
                if (PagedResponseWareHouseStockList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseWareHouseStockList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve WarehouseStock");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateViewFranchiseeWarehouse")]
        public async Task<ActionResult> CreateViewFranchiseeWarehouse([FromBody] Winit.Modules.Org.Model.Classes.EditWareHouseItemView createWareHouseItemView)
        {
            try
            {

                var retVal = await _orgBLservice.CreateViewFranchiseeWarehouse(createWareHouseItemView);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create FranchiseeWarehouse details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateViewFranchiseeWarehouse")]
        public async Task<ActionResult> UpdateViewFranchiseeWarehouse([FromBody] Winit.Modules.Org.Model.Classes.EditWareHouseItemView updateWareHouseItemView)
        {
            try
            {

                var retVal = await _orgBLservice.UpdateViewFranchiseeWarehouse(updateWareHouseItemView);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ViewFranchiseeWarehouse Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteViewFranchiseeWarehouse")]
        public async Task<ActionResult> DeleteViewFranchiseeWarehouse([FromQuery] string UID)
        {
            try
            {
                var retVal = await _orgBLservice.DeleteViewFranchiseeWarehouse(UID);
                if (retVal == -1)
                {
                    return CreateErrorResponse($"Cannot delete the  because this UID: {UID} referenced by other entities.");
                }
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetOrgTypeDetails")]
        public async Task<ActionResult> GetOrgTypeDetails(PagingRequest pagingRequest)
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

                PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrgType> PagedResponseOrgList = null;
                PagedResponseOrgList = await _orgBLservice.GetOrgTypeDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseOrgList == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(PagedResponseOrgList);

            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve OrgType Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetOrgByOrgTypeUID")]
        public async Task<ActionResult> GetOrgByOrgTypeUID([FromQuery] string OrgTypeUID, [FromQuery] string parentUID = null, [FromQuery]  string branchUID = null)
        {
            try
            {
                IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg> OrgDetails = await _orgBLservice.GetOrgByOrgTypeUID(OrgTypeUID, parentUID, branchUID);
                if (OrgDetails != null)
                {
                    return CreateOkApiResponse(OrgDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve OrgDetails with UID: {@OrgTypeUID}", OrgTypeUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }


        [HttpGet]
        [Route("GridDataForMaintainWareHouseStock")]
        public async Task<ActionResult> GridDataForMaintainWareHouseStock([FromQuery] string OrgTypeUID, string parentUID = null, string branchUID = null)
        {
            try
            {
                IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg> OrgDetails = await _orgBLservice.GetOrgByOrgTypeUID(OrgTypeUID, parentUID, branchUID);
                if (OrgDetails != null)
                {
                    return CreateOkApiResponse(OrgDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve OrgDetails with UID: {@OrgTypeUID}", OrgTypeUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }




        [HttpPost("GetOrgHierarchyParentUIDsByOrgUID")]
        public async Task<ActionResult> GetOrgHierarchyParentUIDsByOrgUID(List<string> orgUIDs)
        {
            try
            {
                IEnumerable<string> oRGs = await _orgBLservice.GetOrgHierarchyParentUIDsByOrgUID(orgUIDs);
                if (oRGs != null)
                {
                    return CreateOkApiResponse(oRGs);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Org ");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet("GetProductOrgSelectionItems")]
        public async Task<ActionResult> GetProductOrgSelectionItems()
        {
            try
            {
                IEnumerable<ISelectionItem> oRGs = await _orgBLservice.GetProductOrgSelectionItems();
                if (oRGs != null)
                {
                    return CreateOkApiResponse(oRGs);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Org ");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet("GetProductDivisionSelectionItems")]
        public async Task<ActionResult> GetProductDivisionSelectionItems()
        {
            try
            {
                IEnumerable<ISelectionItem> oRGs = await _orgBLservice.GetProductDivisionSelectionItems();
                if (oRGs != null)
                {
                    return CreateOkApiResponse(oRGs);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Org ");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
         
         
        [HttpPost]
        [Route("GetDivisions")]
        public async Task<ActionResult> GetDivisions([FromBody]string storeUID)
        {

            try
            {
                List<Winit.Modules.Org.Model.Interfaces.IOrg> PagedResponseOrgList = null;
                PagedResponseOrgList = await _orgBLservice.GetDivisions(storeUID);
                if (PagedResponseOrgList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseOrgList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve OrgDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSkuGroupBySkuGroupTypeUID")]
        public async Task<ActionResult> GetSkuGroupBySkuGroupTypeUID([FromQuery] string SkuGroupTypeUid)
        {
            try
            {
                IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroup> OrgDetails = await _orgBLservice.GetSkuGroupBySkuGroupTypeUID(SkuGroupTypeUid);
                if (OrgDetails != null)
                {
                    return CreateOkApiResponse(OrgDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve OrgDetails with UID: {@SkuGroupTypeUid}", SkuGroupTypeUid);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpGet("GetDeliveryDistributorsByOrgUID")]
        public async Task<ActionResult> GetDeliveryDistributorsByOrgUID(string orgUID, string storeUID)
        {
            
            try
            {
                IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg> distributors = await _orgBLservice.GetDeliveryDistributorsByOrgUID(orgUID, storeUID);
                if (distributors != null)
                {
                    return CreateOkApiResponse(distributors);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve delivery distributors for OrgUID: {@orgUID} and StoreUID: {@storeUID}", orgUID, storeUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


    }
}