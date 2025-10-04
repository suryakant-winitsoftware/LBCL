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

namespace WINITAPI.Controllers.Contact
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContactController : WINITBaseController
    {
        private readonly Winit.Modules.Contact.BL.Interfaces.IContactBL _contactBL;
        private readonly ICacheService _cacheService;
        private readonly DataPreparationController _dataPreparationController;
        public ContactController(IServiceProvider serviceProvider, 
            Winit.Modules.Contact.BL.Interfaces.IContactBL contactBL,
            DataPreparationController dataPreparationController) 
            : base(serviceProvider)
        {
            _contactBL = contactBL;
            _dataPreparationController= dataPreparationController;
        }

        [HttpPost]
        [Route("SelectAllContactDetails")]
        public async Task<ActionResult> SelectAllContactDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact> pagedResponseContactList = null;
                pagedResponseContactList = await _contactBL.SelectAllContactDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseContactList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseContactList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ContactDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetContactDetailsByUID")]
        public async Task<ActionResult> GetContactDetailsByUID(string UID)
        {
            try
            {
                Winit.Modules.Contact.Model.Interfaces.IContact ContactDetails = await _contactBL.GetContactDetailsByUID(UID);
                if (ContactDetails != null)
                {
                    return CreateOkApiResponse(ContactDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ContactDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("CreateContactDetails")]
        public async Task<ActionResult> CreateContactDetails([FromBody] Winit.Modules.Contact.Model.Classes.Contact createContact)
        {
            try
            {
                var retVal = await _contactBL.CreateContactDetails(createContact);
                if (retVal > 0) {
                    List<string> uids = new List<string> { createContact.LinkedItemUID };
                    _ = await _dataPreparationController.PrepareStoreMaster(uids);
                   return CreateOkApiResponse(retVal); } else { throw new Exception("Insert Failed"); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Contact details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }

        [HttpPut]
        [Route("UpdateContactDetails")]
        public async Task<ActionResult> UpdateContactDetails([FromBody] Winit.Modules.Contact.Model.Classes.Contact updateContact)
        {
            try
            {
                var existingContactDetails = await _contactBL.GetContactDetailsByUID(updateContact.UID);
                if (existingContactDetails != null)
                {
                    var retVal = await _contactBL.UpdateContactDetails(updateContact);
                    if (retVal > 0)
                    { List<string> uids = new List<string> { updateContact.LinkedItemUID };
                      _= await _dataPreparationController.PrepareStoreMaster(uids);
                      return  CreateOkApiResponse(retVal);
                    }
                    else
                    {
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
                Log.Error(ex, "Error updating ContactDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteContactDetails")]
        public async Task<ActionResult> DeleteContactDetails([FromQuery] string UID)
        {
            try
            {
                var retVal = await _contactBL.DeleteContactDetails(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("ShowAllContactDetails")]
        public async Task<ActionResult> ShowAllContactDetails(PagingRequest pagingRequest)
        {
            try
            {
                PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact> PagedResponseContactList = null;
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }
                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                PagedResponseContactList = await _contactBL.ShowAllContactDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseContactList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseContactList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ContactDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
