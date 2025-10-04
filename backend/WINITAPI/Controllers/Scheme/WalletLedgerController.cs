using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Scheme.DL.Classes;

namespace WINITAPI.Controllers.AwayPeriod
{
    [Route("api/[controller]")]
    [ApiController]
    ////[Authorize]
    public class WalletLedgerController : WINITBaseController
    {



        private readonly IWalletLedgerBL _walletLedgerBL;

        public WalletLedgerController(IServiceProvider serviceProvider, 
            IWalletLedgerBL walletLedgerBL):base(serviceProvider)
        {
            _walletLedgerBL = walletLedgerBL;
        }

        [HttpPost]
        [Route("SelectAllWalletLedger")]
        public async Task<ActionResult> SelectAllWalletLedger(PagingRequest pagingRequest)
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

                var pagedResponse = await _walletLedgerBL.SelectAllWalletLedger(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponse == null)
                {
                    return NotFound();
                }

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve WalletLedgers");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetWalletLedgerByUID")]
        public async Task<ActionResult> GetWalletLedgerByUID(string UID)
        {
            try
            {
                var details = await _walletLedgerBL.GetWalletLedgerByUID(UID);
                if (details != null)
                {
                    return Ok(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve WalletLedger with UID: {@UID}", UID);
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateWalletLedger")]
        public async Task<ActionResult> CreateWalletLedger([FromBody] WalletLedger walletLedger)
        {
            try
            {
                walletLedger.ServerAddTime = DateTime.Now;
                walletLedger.ServerModifiedTime = DateTime.Now;
                var retVal = await _walletLedgerBL.CreateWalletLedger(walletLedger);
                return (retVal > 0) ? Ok(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create WalletLedger ");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateWalletLedger")]
        public async Task<ActionResult> UpdateWalletLedger([FromBody] WalletLedger walletLedger)
        {
            try
            {
                var existing = await _walletLedgerBL.GetWalletLedgerByUID(walletLedger.UID);
                if (existing != null)
                {
                    walletLedger.ServerModifiedTime = DateTime.Now;
                    var retVal = await _walletLedgerBL.UpdateWalletLedger(walletLedger);
                    return (retVal > 0) ? Ok(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating WalletLedger ");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteWalletLedger")]
        public async Task<ActionResult> DeleteWalletLedger([FromQuery] string UID)
        {
            try
            {
                var retVal = await _walletLedgerBL.DeleteWalletLedger(UID);
                return (retVal > 0) ? Ok(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }
    }
}