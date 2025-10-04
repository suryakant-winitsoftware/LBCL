using DBServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Tally.Model.Classes;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Tally
{
    [Route("api/[controller]")]
    [ApiController]
    public class TallyIntegrationController : WINITBaseController
    {
        private readonly Winit.Modules.Tally.BL.Interfaces.ITallyMappingBL _tallyIntegrationService;
        private readonly IDBService _dbService;
        public TallyIntegrationController(IServiceProvider serviceProvider, 
            Winit.Modules.Tally.BL.Interfaces.ITallyMappingBL tallyIntegrationService,
            IDBService dbService) : base(serviceProvider)
        {
            _tallyIntegrationService = tallyIntegrationService;
            _dbService = dbService;
            
        }

        [HttpPost]
        [Route("GetTallyConfigurationData")]
        public async Task<IActionResult> GetTallyConfigurationData([FromQuery] string DistCode)
        {
            try
            {
                if (string.IsNullOrEmpty(DistCode))
                {
                    return BadRequest();
                }
                var tallyConfigurationData = await _tallyIntegrationService.GetTallyConfigurationData(DistCode);

                if (tallyConfigurationData == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(tallyConfigurationData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        [Route("InsertRetailersFromTally")]
        public async Task<ActionResult> InsertRetailersFromTally([FromBody] List<IRetailersFromTally> retailersFromTally)
        {
            try
            {
                bool retValue = await _tallyIntegrationService.InsertRetailersFromTally(retailersFromTally);
                if (retValue)
                {
                    return Created("Created", retValue);
                }
                else
                {
                    return StatusCode(500, retValue);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create details");
            }
        }
        [HttpPost]
        [Route("InsertInventoryFromTally")]
        public async Task<ActionResult> InsertInventoryFromTally([FromBody] List<IInventoryFromTally> inventoryFromTally)
        {
            try
            {
                bool retValue = await _tallyIntegrationService.InsertInventoryFromTally(inventoryFromTally);
                if (retValue)
                {
                    return Created("Created", retValue);
                }
                else
                {
                    return StatusCode(500, retValue);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create details");
            }
        }
        [HttpPost]
        [Route("InsertOrdersFromTally")]
        public async Task<ActionResult> InsertOrdersFromTally([FromBody] List<ISalesOrderHeaderFromTally> ordersFromTally)
        {
            try
            {
                bool retValue = await _tallyIntegrationService.InsertOrdersFromTally(ordersFromTally);
                if (retValue)
                {
                    return Created("Created", retValue);
                }
                else
                {
                    return StatusCode(500, retValue);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create details");
            }
        }
        [HttpPost]
        [Route("GetDistMappedSKUList")]
        public async Task<IActionResult> GetDistMappedSKUList([FromQuery] string DistCode)
        {
            try
            {
                if (string.IsNullOrEmpty(DistCode))
                {
                    return BadRequest();
                }
                var distmappedSKUList = await _tallyIntegrationService.GetDistMappedSKUList(DistCode);

                if (distmappedSKUList == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(distmappedSKUList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        [Route("GetRetailersFromDB")]
        public async Task<IActionResult> GetRetailersFromDB([FromQuery] string orgUID)
        {
            try
            {
                if (string.IsNullOrEmpty(orgUID))
                {
                    return BadRequest();
                }
                var getRetailersList = await _tallyIntegrationService.GetRetailersFromDB(orgUID);

                if (getRetailersList == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(getRetailersList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        [Route("RetailerStatusFromTally")]
        public async Task<ActionResult> RetailerStatusFromTally([FromBody] List<IRetailerTallyStatus> retailerStatusFromTally)
        {
            try
            {
                bool retValue = await _tallyIntegrationService.RetailerStatusFromTally(retailerStatusFromTally);
                if (retValue)
                {
                    return Created("Created", retValue);
                }
                else
                {
                    return StatusCode(500, retValue);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create details");
            }
        }
        [HttpPost]
        [Route("GetSalesOrderFromDB")]
        public async Task<IActionResult> GetSalesOrderFromDB([FromQuery] string orgUID)
        {
            try
            {
                if (string.IsNullOrEmpty(orgUID))
                {
                    return BadRequest();
                }
                var getRetailersList = await _tallyIntegrationService.GetSalesOrderFromDB(orgUID);

                if (getRetailersList == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(getRetailersList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        [Route("SalesStatusFromTally")]
        public async Task<ActionResult> SalesStatusFromTally([FromBody] List<ISalesTallyStatus> salesStatusFromTally)
        {
            try
            {
                bool retValue = await _tallyIntegrationService.SalesStatusFromTally(salesStatusFromTally);
                if (retValue)
                {
                    return Created("Created", retValue);
                }
                else
                {
                    return StatusCode(500, retValue);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create details");
            }
        }
    }
}
