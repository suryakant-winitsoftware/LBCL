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
using Newtonsoft.Json;
using Serilog;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITAPI.Controllers.SKU;
using WINITServices.Interfaces.CacheHandler;

namespace WINITAPI.Controllers.Address
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController:WINITBaseController
    {
        // Constants for error messages
        private const string InvalidRequestData = "Invalid request data";
        private const string InvalidPageSizeOrNumber = "Invalid page size or page number";
        private const string ProcessingError = "An error occurred while processing the request.";
        private const string InsertFailed = "Insert Failed";
        private const string UpdateFailed = "Update Failed";
        private const string DeleteFailed = "Delete Failed";

        private readonly Winit.Modules.Address.BL.Interfaces.IAddressBL _addressBL;
        private readonly DataPreparationController _dataPreparationController;
        
        public AddressController(IServiceProvider serviceProvider, 
            Winit.Modules.Address.BL.Interfaces.IAddressBL addressBL,
            DataPreparationController dataPreparationController) : base(serviceProvider)
        {
            _addressBL = addressBL;
            _dataPreparationController= dataPreparationController;
        }

        /// <summary>
        /// Retrieves all address details with pagination support.
        /// </summary>
        /// <param name="pagingRequest">The paging request containing sort, filter, and pagination parameters</param>
        /// <returns>A paged response containing address details</returns>
        [HttpPost]
        [Route("SelectAllAddressDetails")]
        public async Task<ActionResult> SelectAllAddressDetails(PagingRequest pagingRequest)
        {
            try
            {
                if (pagingRequest == null)
                {
                    return BadRequest(InvalidRequestData);
                }
                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest(InvalidPageSizeOrNumber);
                }
                var pagedResponseAddressList = await _addressBL.SelectAllAddressDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseAddressList == null)
                {
                    return NotFound();
                }
               
                return CreateOkApiResponse(pagedResponseAddressList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve AddressDetails");
                return CreateErrorResponse(ProcessingError + ex.Message);
            }
        }

        /// <summary>
        /// Gets address details by the specified UID.
        /// </summary>
        /// <param name="UID">The unique identifier of the address</param>
        /// <returns>The address details if found, otherwise NotFound</returns>
        [HttpGet]
        [Route("GetAddressDetailsByUID")]
        public async Task<ActionResult> GetAddressDetailsByUID(string UID)
        {
            try
            {
                //var CachedData = _cacheService.HGet<Winit.Modules.Address.Model.Classes.Address>("ADDRESSES", UID);
                //if (CachedData != null)
                //{
                //    return CreateOkApiResponse(CachedData);
                //}
                var addressDetails = await _addressBL.GetAddressDetailsByUID(UID);
                if (addressDetails != null)
                {
                    return CreateOkApiResponse(addressDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve AddressDetails with UID: {@UID}", UID);
                return CreateErrorResponse(ProcessingError + ex.Message);
            }
        }

        /// <summary>
        /// Creates new address details.
        /// </summary>
        /// <param name="createAddress">The address details to create</param>
        /// <returns>The number of records created</returns>
        [HttpPost]
        [Route("CreateAddressDetails")]
        public async Task<ActionResult> CreateAddressDetails([FromBody] Winit.Modules.Address.Model.Classes.Address createAddress)
        {
            try
            {
                createAddress.ServerAddTime = DateTime.Now;
                createAddress.ServerModifiedTime = DateTime.Now;
                var retVal = await _addressBL.CreateAddressDetails(createAddress);
                 if(retVal > 0) 
                    {
                    var uids = new List<string> { createAddress.LinkedItemUID };
                    _ = _dataPreparationController.PrepareStoreMaster(uids);
                    return CreateOkApiResponse(retVal);
                     }
                else
                {
                    throw new Exception(InsertFailed);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Address details");
                return CreateErrorResponse(ProcessingError + ex.Message);
            }
        }

        /// <summary>
        /// Creates multiple address details from a list.
        /// </summary>
        /// <param name="createAddress">The list of address details to create</param>
        /// <returns>The number of records created</returns>
        [HttpPost]
        [Route("CreateAddressDetailsList")]
        public async Task<ActionResult> CreateAddressDetailsList([FromBody] List<Winit.Modules.Address.Model.Interfaces.IAddress> createAddress)
        {
            try
            {
                var retVal = await _addressBL.CreateAddressDetailsList(createAddress);
                 if(retVal > 0) 
                    {
                    foreach (var item in createAddress)
                    {
                        var uids = new List<string> { item.LinkedItemUID };
                        _ = _dataPreparationController.PrepareStoreMaster(uids);
                    }
                    return CreateOkApiResponse(retVal);
                     }
                else
                {
                    throw new Exception(InsertFailed);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Address details");
                return CreateErrorResponse(ProcessingError + ex.Message);
            }
        }

        /// <summary>
        /// Updates existing address details.
        /// </summary>
        /// <param name="updateAddress">The address details to update</param>
        /// <returns>The number of records updated</returns>
        [HttpPut]
        [Route("UpdateAddressDetails")]
        public async Task<ActionResult> UpdateAddressDetails([FromBody] Winit.Modules.Address.Model.Classes.Address updateAddress)
        {
            try
            {
                var existingAddressDetails = await _addressBL.GetAddressDetailsByUID(updateAddress.UID);
                if (existingAddressDetails != null)
                {
                    var retVal = await _addressBL.UpdateAddressDetails(updateAddress);
                    if (retVal > 0) {
                        var uids = new List<string> { updateAddress.LinkedItemUID };
                        _ = _dataPreparationController.PrepareStoreMaster(uids);
                        return CreateOkApiResponse(retVal);
                            } else { throw new Exception(UpdateFailed); }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating AddressDetails");
                return CreateErrorResponse(ProcessingError + ex.Message);
            }
        }

        /// <summary>
        /// Deletes address details by UID.
        /// </summary>
        /// <param name="UID">The unique identifier of the address to delete</param>
        /// <returns>The number of records deleted</returns>
        [HttpDelete]
        [Route("DeleteAddressDetails")]
        public async Task<ActionResult> DeleteAddressDetails([FromQuery] string UID)
        {
            try
            {
                var retVal = await _addressBL.DeleteAddressDetails(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception(DeleteFailed);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse(ProcessingError + ex.Message);
            }
        }
    }
}
