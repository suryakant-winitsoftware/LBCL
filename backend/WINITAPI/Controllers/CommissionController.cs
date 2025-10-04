using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using WINITRepository.Interfaces.Customers;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Commission.PostgreSQLCommissionRepository;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommissionController : WINITBaseController
    {
        private readonly WINITServices.Interfaces.ICommissionService _CommissionService;

        public CommissionController(IServiceProvider serviceProvider, 
            WINITServices.Interfaces.ICommissionService commissionService) : base(serviceProvider)
        {
            _CommissionService = commissionService;
        }
       


        [HttpGet]
        [Route("ProcessCommission")]
        public async Task<ActionResult> ProcessCommission()
        {
            try
            {
               
                int Result = await _CommissionService.ProcessCommission();
                Log.Information("Successfully processed Commission details {@Result}", Result);
                return Ok(Result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to process Commission data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to process Commission data");
            }
        }

    }
}