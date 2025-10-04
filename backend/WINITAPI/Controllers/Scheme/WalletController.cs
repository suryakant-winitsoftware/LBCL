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
    //[Authorize]
    public class WalletController : WINITBaseController
    {
        private readonly IWalletBL _walletBL;

        public WalletController(IServiceProvider serviceProvider, 
            IWalletBL walletBL):base(serviceProvider)
        {
            _walletBL = walletBL;
        }

        [HttpPost]
        [Route("SelectAllWallets")]
        public async Task<ActionResult> SelectAllWallets(PagingRequest pagingRequest)
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

                var pagedResponse = await _walletBL.SelectAllWallet(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponse == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Wallets");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetWalletByUID")]
        public async Task<ActionResult> GetWalletByUID(string UID)
        {
            try
            {
                var details = await _walletBL.GetWalletByUID(UID);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Wallet with UID: {@UID}", UID);
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetWalletByOrgUID")]
        public async Task<ActionResult> GetWalletByOrgUID(string OrgUID)
        {
            try
            {
                var details = await _walletBL.GetWalletByOrgUID(OrgUID);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Wallet with OrgUID: {@UID}", OrgUID);
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateWallet")]
        public async Task<ActionResult> CreateWallet([FromBody] Wallet wallet)
        {
            try
            {
                wallet.ServerAddTime = DateTime.Now;
                wallet.ServerModifiedTime = DateTime.Now;
                var retVal = await _walletBL.CreateWallet(wallet);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Wallet ");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateWallet")]
        public async Task<ActionResult> UpdateWallet([FromBody] Wallet wallet)
        {
            try
            {
                var existing = await _walletBL.GetWalletByUID(wallet.UID);
                if (existing != null)
                {
                    wallet.ServerModifiedTime = DateTime.Now;
                    var retVal = await _walletBL.UpdateWallet(wallet);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Wallet ");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteWallet")]
        public async Task<ActionResult> DeleteWallet([FromQuery] string UID)
        {
            try
            {
                var retVal = await _walletBL.DeleteWallet(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpPost]
        [Route("UpdateWalletAsync")]
        public async Task<ActionResult> UpdateWalletAsync([FromBody] List<IWalletLedger> walletLedgers)
        {
            try
            {
                var retVal = await _walletBL.UpdateWalletAsync(walletLedgers);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("UpdateWalletAsync Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to UpdateWalletAsync ");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }
    }
}