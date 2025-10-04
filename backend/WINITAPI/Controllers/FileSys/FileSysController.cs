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
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.SKU.Model.Classes;
using WINITAPI.Controllers.SKU;



namespace WINITAPI.Controllers.FileSys
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileSysController:WINITBaseController
    {
        private readonly Winit.Modules.FileSys.BL.Interfaces.IFileSysBL _FileSysBL;
        private readonly DataPreparationController _dataPreparationController;

        public FileSysController(IServiceProvider serviceProvider,
            Winit.Modules.FileSys.BL.Interfaces.IFileSysBL FileSysBL,
            DataPreparationController dataPreparationController)
            : base(serviceProvider)
        {
            _FileSysBL = FileSysBL;
            _dataPreparationController = dataPreparationController;
        }
        [HttpPost]
        [Route("SelectAllFileSysDetails")]
        public async Task<ActionResult> SelectAllFileSysDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSys> pagedResponseFileSysList = null;
                pagedResponseFileSysList = await _FileSysBL.SelectAllFileSysDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseFileSysList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseFileSysList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve FileSysDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetFileSysByUID")]
        public async Task<ActionResult> GetFileSysByUID(string UID)
        {
            try
            {
                Winit.Modules.FileSys.Model.Interfaces.IFileSys FileSysDetails = await _FileSysBL.GetFileSysByUID(UID);
                if (FileSysDetails != null)
                {
                    return CreateOkApiResponse(FileSysDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve FileSysDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("CreateFileSys")]
        public async Task<ActionResult> CreateFileSys([FromBody] Winit.Modules.FileSys.Model.Classes.FileSys FileSys)
        {
            try
            {
                FileSys.ServerAddTime = DateTime.Now;
                FileSys.ServerModifiedTime = DateTime.Now;
                var retVal = await _FileSysBL.CreateFileSys(FileSys);
                if (retVal > 0) {
                    PrepareSKURequestModel sKURequestModel = new PrepareSKURequestModel()
                    {
                        SKUUIDs = new List<string>() { FileSys.LinkedItemUID }
                    };
                    _ = await _dataPreparationController.PrepareSKUMaster(sKURequestModel);
                   return CreateOkApiResponse(retVal);

                } else { throw new Exception("Insert Failed"); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create FileSys details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateFileSys")]
        public async Task<ActionResult> UpdateFileSys([FromBody] Winit.Modules.FileSys.Model.Classes.FileSys updateFileSys)
        {
            try
            {
                var existingFileSysDetails = await _FileSysBL.GetFileSysByUID(updateFileSys.UID);
                if (existingFileSysDetails != null)
                {
                    
                    updateFileSys.ServerModifiedTime = DateTime.Now;
                    var retVal = await _FileSysBL.UpdateFileSys(updateFileSys);
                    if (retVal > 0) {
                        PrepareSKURequestModel sKURequestModel = new PrepareSKURequestModel()
                        {
                            SKUUIDs = new List<string>() { updateFileSys.LinkedItemUID }
                        };
                        _ = await _dataPreparationController.PrepareSKUMaster(sKURequestModel);
                        return CreateOkApiResponse(retVal); }
                    else {
                        throw new Exception("Update Failed");
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating FileSysDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteFileSys")]
        public async Task<ActionResult> DeleteFileSys([FromQuery] string UID)
        {
            try
            {
                var retVal = await _FileSysBL.DeleteFileSys(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUImageIsDefault")]
        public async Task<ActionResult> UpdateSKUImageIsDefault([FromBody] List<Winit.Modules.FileSys.Model.Classes.SKUImage>  sKUmagesList)
        {
            int retVal = -1;
            try
            {
                if (sKUmagesList != null)
                {
                     retVal = await _FileSysBL.UpdateSKUImageIsDefault(sKUmagesList);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating FileSysDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
            return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
        }
        [HttpPost]
        [Route("CreateFileSysForBulk")]
        public async Task<ActionResult> CreateFileSysForBulk([FromBody] List<Winit.Modules.FileSys.Model.Classes.FileSys> createFileSys)
        {
            try
            {
                List<CommonUIDResponse> commonUIDResponses = await _FileSysBL.CreateFileSysForBulk(createFileSys); ;
              return  CreateOkApiResponse(commonUIDResponses);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create FileSys details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateFileSysForList")]
        public async Task<ActionResult> CreateFileSysForList([FromBody] List<List<Winit.Modules.FileSys.Model.Classes.FileSys>> createFileSys)
        {
            try
            {
                bool commonUIDResponses = await _FileSysBL.CreateFileSysForList(createFileSys); ;
              return  CreateOkApiResponse(commonUIDResponses);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create FileSys details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CUDFileSys")]
        public async Task<ActionResult> CUDFileSys([FromBody] Winit.Modules.FileSys.Model.Classes.FileSys FileSys)
        {
            try
            {
                FileSys.ServerAddTime = DateTime.Now;
                FileSys.ServerModifiedTime = DateTime.Now;
                var retVal = await _FileSysBL.CUDFileSys(FileSys);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CUD Operation Failed");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
    }
}
