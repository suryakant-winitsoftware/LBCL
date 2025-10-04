using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Territory
{
    [Route("api/[controller]")]
    [ApiController]
    public class TerritoryController : WINITBaseController
    {
        private readonly Winit.Modules.Territory.BL.Interfaces.ITerritoryBL _TerritoryBL;

        public TerritoryController(IServiceProvider serviceProvider,
            Winit.Modules.Territory.BL.Interfaces.ITerritoryBL TerritoryBL) : base(serviceProvider)
        {
            _TerritoryBL = TerritoryBL;
        }

        [HttpPost]
        [Route("SelectAllTerritories")]
        public async Task<ActionResult> SelectAllTerritories(PagingRequest pagingRequest)
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

                PagedResponse<Winit.Modules.Territory.Model.Interfaces.ITerritory> PagedResponseTerritoryList = null;
                PagedResponseTerritoryList = await _TerritoryBL.SelectAllTerritories(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);

                if (PagedResponseTerritoryList == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(PagedResponseTerritoryList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Territory details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTerritoryByUID")]
        public async Task<ActionResult> GetTerritoryByUID(string UID)
        {
            try
            {
                Winit.Modules.Territory.Model.Interfaces.ITerritory TerritoryDetails = await _TerritoryBL.GetTerritoryByUID(UID);
                if (TerritoryDetails != null)
                {
                    return CreateOkApiResponse(TerritoryDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Territory with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTerritoryByCode")]
        public async Task<ActionResult> GetTerritoryByCode(string territoryCode, string orgUID)
        {
            try
            {
                Winit.Modules.Territory.Model.Interfaces.ITerritory TerritoryDetails = await _TerritoryBL.GetTerritoryByCode(territoryCode, orgUID);
                if (TerritoryDetails != null)
                {
                    return CreateOkApiResponse(TerritoryDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Territory with Code: {@TerritoryCode}, OrgUID: {@OrgUID}", territoryCode, orgUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTerritoriesByOrg")]
        public async Task<ActionResult> GetTerritoriesByOrg(string orgUID)
        {
            try
            {
                var territories = await _TerritoryBL.GetTerritoriesByOrg(orgUID);
                if (territories != null && territories.Any())
                {
                    return CreateOkApiResponse(territories);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Territories for Org: {@OrgUID}", orgUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTerritoriesByManager")]
        public async Task<ActionResult> GetTerritoriesByManager(string managerEmpUID)
        {
            try
            {
                var territories = await _TerritoryBL.GetTerritoriesByManager(managerEmpUID);
                if (territories != null && territories.Any())
                {
                    return CreateOkApiResponse(territories);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Territories for Manager: {@ManagerEmpUID}", managerEmpUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTerritoriesByCluster")]
        public async Task<ActionResult> GetTerritoriesByCluster(string clusterCode)
        {
            try
            {
                var territories = await _TerritoryBL.GetTerritoriesByCluster(clusterCode);
                if (territories != null && territories.Any())
                {
                    return CreateOkApiResponse(territories);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Territories for Cluster: {@ClusterCode}", clusterCode);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateTerritory")]
        public async Task<ActionResult> CreateTerritory([FromBody] Winit.Modules.Territory.Model.Classes.Territory territory)
        {
            try
            {
                territory.ServerAddTime = DateTime.Now;
                territory.ServerModifiedTime = DateTime.Now;
                var retVal = await _TerritoryBL.CreateTerritory(territory);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Territory");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateTerritory")]
        public async Task<ActionResult> UpdateTerritory([FromBody] Winit.Modules.Territory.Model.Classes.Territory updateTerritory)
        {
            try
            {
                var existingTerritory = await _TerritoryBL.GetTerritoryByUID(updateTerritory.UID);
                if (existingTerritory != null)
                {
                    updateTerritory.ServerModifiedTime = DateTime.Now;
                    var retVal = await _TerritoryBL.UpdateTerritory(updateTerritory);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Territory");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteTerritory")]
        public async Task<ActionResult> DeleteTerritory([FromQuery] string UID)
        {
            try
            {
                var retVal = await _TerritoryBL.DeleteTerritory(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete Territory");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
