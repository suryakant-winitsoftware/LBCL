using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Int_CommonMethods.BL.Interfaces;
using Winit.Shared.Models.Common;
using WINITServices.Interfaces.CacheHandler;

namespace WINITAPI.Controllers.IntPushedDataStatus
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IntPendingDataInsertionController : WINITBaseController
    {
        private readonly Winit.Modules.Int_CommonMethods.BL.Interfaces.Iint_CommonMethodsBL _int_CommonMethods;
        public IntPendingDataInsertionController(IServiceProvider serviceProvider, 
            Iint_CommonMethodsBL int_CommonMethods) : base(serviceProvider)
        {
            _int_CommonMethods = int_CommonMethods;
        }

        [HttpPost]
        [Route("InsertPendingData")]
        public async Task<ActionResult> PostPendingData([FromBody] Winit.Modules.Int_CommonMethods.Model.Interfaces.IPendingDataRequest pagingRequest)
        {
            try
            {
                await _int_CommonMethods.InsertPendingData(pagingRequest);
                return CreateOkApiResponse("success");
            }
            catch { throw; }
        }

        [HttpPost]
        [Route("InsertPendingDataList")]
        public async Task<ActionResult> InsertPendingDataList([FromBody] List<Winit.Modules.Int_CommonMethods.Model.Interfaces.IPendingDataRequest> pagingRequest)
        {
            try
            {
                await _int_CommonMethods.InsertPendingDataList(pagingRequest);
                return CreateOkApiResponse("success");
            }
            catch { throw; }
        }
    }

}
