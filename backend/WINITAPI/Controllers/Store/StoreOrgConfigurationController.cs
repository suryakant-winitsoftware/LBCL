using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Store.Model.Classes;

namespace WINITAPI.Controllers.Store
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreOrgConfigurationController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreOrgConfigurationBL _iStoreOrgConfigurationBL;
        Winit.Shared.CommonUtilities.Common.CommonFunctions commonFunctions = new();
        public StoreOrgConfigurationController(IServiceProvider serviceProvider, 
            Winit.Modules.Store.BL.Interfaces.IStoreOrgConfigurationBL iStoreOrgConfigurationBL) 
            : base(serviceProvider)
        {
            _iStoreOrgConfigurationBL = iStoreOrgConfigurationBL;
        }

        [HttpPost]
        [Route("CreateStoreOrgConfiguration")]
        public async Task<ActionResult> CreateStoreOrgConfiguration([FromBody] OrgConfigurationUIModel orgConfiguration)
        {
            try
            {
                var retValue = await _iStoreOrgConfigurationBL.CreateStoreOrgConfiguratoion(orgConfiguration);
                if (retValue > 0)
                {
                   
                    return CreateOkApiResponse(retValue);
                }
                else { throw new Exception("Insert Failed"); }
            }
            catch (Exception ex)
            {
               
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
         [HttpGet]
        [Route("SelectStoreOrgConfigurationByStoreUID")]
        public async Task<ActionResult> SelectStoreOrgConfigurationByStoreUID(string storeUID)
        {
            try
            {
                var retValue = await _iStoreOrgConfigurationBL.SelectStoreOrgConfigurationByStoreUID(storeUID);
                if (retValue != null)
                {
                   
                    return CreateOkApiResponse(retValue);
                }
                else 
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
               
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

    }
}
