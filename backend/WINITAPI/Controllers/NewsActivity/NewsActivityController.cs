using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.NewsActivity.BL.Interfaces;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.NewsActivity
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsActivityController : WINITBaseController
    {
        private readonly INewsActivityBL _newsActivityBL;
        private readonly IFileSysBL _fileSysBL;
        public NewsActivityController(IServiceProvider serviceProvider, 
            INewsActivityBL newsActivityBL, IFileSysBL fileSysBL) : base(serviceProvider)
        {
            _newsActivityBL = newsActivityBL;
            _fileSysBL = fileSysBL;
        }
        [HttpPost]
        [Route("SelectAllNewsActivities/{isFilesNeeded}")]
        public async Task<ActionResult> SelectAllNewsActivities([FromBody] PagingRequest pagingRequest, bool isFilesNeeded)
        {
            //if (pagingRequest == null)
            //{
            //    return BadRequest("Invalid request data");
            //}
            //if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            //{
            //    return BadRequest("Invalid page size or page number");
            //}
            //try
            //{
            //    var response = await _newsActivityBL.SelectAllNewsActivities(pagingRequest);
            //    if (isFilesNeeded && response != null && response.PagedData != null)
            //    {
            //        List<string> arr = [];
            //        arr.AddRange(response.PagedData.Select(p => p.UID));
            //        var files = await _fileSysBL.GetFileSysByLinkedItemUIDs(arr);
            //        if (files != null && files.Count > 0)
            //        {
            //            foreach (var item in response.PagedData)
            //            {
            //                item.FilesysList = [];
            //                item.FilesysList.AddRange(files.FindAll(p => p.LinkedItemUID.Equals(item.UID)));
            //            }
            //        }
            //    }
            //    return CreateOkApiResponse(response);
            //}
            //catch (Exception ex)
            //{
            //    Log.Error(ex, "Fail to retrieve Role");
            //    return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            //}

            return BadRequest("Invalid page size or page number");
        }
        [HttpPost]
        [Route("CreateNewsActivity")]
        public async Task<IActionResult> CreateNewsActivity([FromBody] INewsActivity newsActivity)
        {
            if (newsActivity == null)
            {
                return BadRequest("Details Shouldn't be null");
            }
            try
            {
                newsActivity.ServerAddTime = DateTime.Now;
                newsActivity.ServerModifiedTime = DateTime.Now;
                return CreateOkApiResponse(await _newsActivityBL.CreateNewsActivity(newsActivity));
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex.Message);
            }
        }
        [HttpGet]
        [Route("GetNewsActivitysByUID")]
        public async Task<IActionResult> GetNewsActivitysByUID(string UID)
        {
            if (string.IsNullOrEmpty(UID))
            {
                return BadRequest("UID Shouldn't be null");
            }
            try
            {
                return CreateOkApiResponse(await _newsActivityBL.GetNewsActivitysByUID(UID));
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateNewsActivity")]
        public async Task<IActionResult> UpdateNewsActivity([FromBody] INewsActivity newsActivity)
        {
            if (newsActivity == null)
            {
                return BadRequest("Details Shouldn't be null");
            }
            try
            {
                newsActivity.ServerModifiedTime = DateTime.Now;
                return CreateOkApiResponse(await _newsActivityBL.UpdateNewsActivity(newsActivity));
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex.Message);
            }
        }
        [HttpPut]
        [Route("DeleteNewsActivityByUID")]
        public async Task<IActionResult> DeleteNewsActivityByUID([FromQuery] string UID)
        {
            if (UID == null)
            {
                return BadRequest("UID Shouldn't be null");
            }
            try
            {
                return CreateOkApiResponse(await _newsActivityBL.DeleteNewsActivityByUID(UID));
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex.Message);
            }
        }
    }
}
