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
using WINITSharedObjects.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using System.Security.Cryptography;
using Winit.Shared.Models.Common;
using Winit.Modules.FirebaseReport.BL.Interfaces;
using Winit.UIModels.Common.Sales;
using Newtonsoft.Json;
using WINITServices.Classes.RabbitMQ;
using DBServices.Interfaces;
//using RabbitMQService.Interfaces;
using WINITSharedObjects.Models;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using DBServices.Classes;
using Winit.Modules.FirebaseReport.Models.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;

namespace WINITAPI.Controllers.FirebaseReport
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FirebaseReportController : WINITBaseController
    {
        private readonly IFirebaseReportBL _firebaseReportBL;
        private readonly IDBService _dbService;
        private readonly ILogger<FirebaseReportController> _logger;

        public FirebaseReportController(IServiceProvider serviceProvider, 
            IFirebaseReportBL firebaseReportBL,
            ILogger<FirebaseReportController> logger) : base(serviceProvider)
        {
            _firebaseReportBL = firebaseReportBL;
            _logger = logger;
        }
        [HttpPost]
        [Route("SelectAllFirebaseReportDetails")]
        public async Task<ActionResult<PagedResponse<IFirebaseReport>>> SelectAllFirebaseReportDetails(PagingRequest pagingRequest)
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
                var firebaseReportDetailsModel = await _firebaseReportBL.SelectAllFirebaseReportDetails(pagingRequest.SortCriterias, 
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired);
                if (firebaseReportDetailsModel == null)
                {
                    return NotFound();
                }
                else
                {
                    return CreateOkApiResponse(firebaseReportDetailsModel);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("SelectFirebaseDetailsData")]
        public async Task<ActionResult<IFirebaseReport>> SelectFirebaseDetailsData(object UID)
        {
            try
            {
                string requiredUID = UID.ToString();
                var firebaseReportDetailsModel = await _firebaseReportBL.SelectFirebaseDetailsData(requiredUID);
                if (firebaseReportDetailsModel == null)
                {
                    return NotFound();
                }
                else
                {
                    return CreateOkApiResponse(firebaseReportDetailsModel);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}