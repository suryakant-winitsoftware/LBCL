using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreDocumentController:WINITBaseController
    {
        private readonly Winit.Modules.StoreDocument.BL.Interfaces.IStoreDocumentBL _StoreDocumentBL;

        public StoreDocumentController(IServiceProvider serviceProvider, 
            Winit.Modules.StoreDocument.BL.Interfaces.IStoreDocumentBL StoreDocumentBL) : base(serviceProvider)
        {
            _StoreDocumentBL = StoreDocumentBL;
        }
        [HttpPost]
        [Route("SelectAllStoreDocumentDetails")]
        public async Task<ActionResult> SelectAllStoreDocumentDetails(
            PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument> pagedResponseStoreDocumentList = null;
                pagedResponseStoreDocumentList = await _StoreDocumentBL.SelectAllStoreDocumentDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseStoreDocumentList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseStoreDocumentList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve StoreDocument  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpGet]
        [Route("GetStoreDocumentDetailsByUID")]
        public async Task<ActionResult<ApiResponse<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument>>> GetStoreDocumentDetailsByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument storeDocument = await _StoreDocumentBL.GetStoreDocumentDetailsByUID(UID);
                if (storeDocument != null)
                {
                    return CreateOkApiResponse(storeDocument);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StoreDocumentList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpPost]
        [Route("CreateStoreDocumentDetails")]
        public async Task<ActionResult> CreateStoreDocumentDetails([FromBody] Winit.Modules.StoreDocument.Model.Classes.StoreDocument createStoreDocument)
        {
            try
            {
                createStoreDocument.ServerAddTime = DateTime.Now;
                createStoreDocument.ServerModifiedTime = DateTime.Now;
                var retVal = await _StoreDocumentBL.CreateStoreDocumentDetails(createStoreDocument);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create StoreDocument details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }


        [HttpPut]
        [Route("UpdateStoreDocumentDetails")]
        public async Task<ActionResult> UpdateStoreDocumentDetails([FromBody] Winit.Modules.StoreDocument.Model.Classes.StoreDocument updateStoreDocument)
        {
            try
            {
                var existingStoreDocumentDetails = await _StoreDocumentBL.GetStoreDocumentDetailsByUID(updateStoreDocument.UID);
                if (existingStoreDocumentDetails != null)
                {
                    updateStoreDocument.ServerModifiedTime = DateTime.Now;
                    var retVal = await _StoreDocumentBL.UpdateStoreDocumentDetails(updateStoreDocument);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating StoreDocumentDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }


        [HttpDelete]
        [Route("DeleteStoreDocumentDetails")]
        public async Task<ActionResult> DeleteStoreDocumentDetails([FromQuery] string UID)
        {
            try
            {
                var retVal = await _StoreDocumentBL.DeleteStoreDocumentDetails(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
    }
}
