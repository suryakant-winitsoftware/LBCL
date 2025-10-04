using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using WINITRepository.Interfaces.Customers;
using Winit.Shared.Models.Constants;
using System.Security.Cryptography;
using Winit.Shared.Models.Common;
using Winit.Modules.ListHeader.Model.Interfaces;

namespace WINITAPI.Controllers.ListItemHeader
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class ListItemHeaderController : WINITBaseController
    {
        private readonly Winit.Modules.ListHeader.BL.Interfaces.IListHeaderBL _listHeaderBL;
        public ListItemHeaderController(IServiceProvider serviceProvider, 
            Winit.Modules.ListHeader.BL.Interfaces.IListHeaderBL listHeaderBL) 
            : base(serviceProvider)
        {
            _listHeaderBL = listHeaderBL;
        }
        [HttpPost]
        [Route("GetListHeaders")]
        public async Task<ActionResult> GetListHeaders(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListHeader> PagedResponseListHeaderList = null;
                PagedResponseListHeaderList = await _listHeaderBL.GetListHeaders(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseListHeaderList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseListHeaderList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ListHeaders");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        

        [HttpPost]
        [Route("GetListItemsByCodes")]
        public async Task<ActionResult> GetListItemsByCodes([FromBody] Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest)
        {
            try
            {
                PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem> PagedResponseListItemsList = null;
                PagedResponseListItemsList = await _listHeaderBL.GetListItemsByCodes(listItemRequest.Codes, listItemRequest.isCountRequired);
                if (PagedResponseListItemsList != null)
                {
                    return CreateOkApiResponse(PagedResponseListItemsList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ListItem");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateListItem")]
        public async Task<ActionResult> CreateListItem([FromBody] Winit.Modules.ListHeader.Model.Classes.ListItem listItem)
        {
            try
            {
                listItem.ServerAddTime = DateTime.Now;
                listItem.ServerModifiedTime = DateTime.Now;
                var listItemDetails = await _listHeaderBL.CreateListItem(listItem);
                return (listItemDetails > 0) ? CreateOkApiResponse(listItemDetails) : throw new Exception("Create Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Create ListItem details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateListItem")]
        public async Task<ActionResult> UpdateListItem([FromBody] Winit.Modules.ListHeader.Model.Classes.ListItem listItem)
        {
            try
            {
                listItem.ServerModifiedTime = DateTime.Now;
                    var updateListItemDetails = await _listHeaderBL.UpdateListItem(listItem);
                    return (updateListItemDetails > 0) ? CreateOkApiResponse(updateListItemDetails) : throw new Exception("Update Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ListItem Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteListItemByUID")]
        public async Task<ActionResult> DeleteListItemByUID([FromQuery] string UID)
        {
            try
            {
                var result = await _listHeaderBL.DeleteListItemByUID(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetListItemsByHeaderUID")]
        public async Task<ActionResult> GetListItemsByHeaderUID([FromBody] string headerUID)
        {
            try
            {
                IEnumerable<Winit.Modules.ListHeader.Model.Interfaces.IListItem> OrgDetails = await _listHeaderBL.GetListItemsByHeaderUID(headerUID);
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
                Log.Error(ex, "Failed to retrieve ListItem with headerUID: {@headerUID}", headerUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }



        [HttpGet]
        [Route("GetListItemsByUID")]
        public async Task<ActionResult> GetJobPositionByUID(string UID)
        {
            try
            {
                Winit.Modules.ListHeader.Model.Interfaces.IListItem listItem = await _listHeaderBL.GetListItemsByUID(UID);
                if (listItem != null)
                {
                    return CreateOkApiResponse(listItem);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve List Item with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("GetListItemsByListHeaderCodes")]
        public async Task<ActionResult> GetListItemsByListHeaderCodes([FromBody] Winit.Modules.ListHeader.Model.Classes.ListItems listItemRequest)
        {
            try
            {
                PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem> PagedResponseListItemsList = null;
                PagedResponseListItemsList = await _listHeaderBL.GetListItemsByListHeaderCodes(listItemRequest.ListItemRequest.Codes, listItemRequest.PagingRequest.SortCriterias, listItemRequest.PagingRequest.PageNumber, listItemRequest.PagingRequest.PageSize, listItemRequest.PagingRequest.FilterCriterias, listItemRequest.ListItemRequest.isCountRequired);
                if (PagedResponseListItemsList != null)
                {
                    return CreateOkApiResponse(PagedResponseListItemsList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ListItem");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}