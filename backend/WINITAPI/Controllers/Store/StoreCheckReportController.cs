using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using Serilog;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using WINITAPI.Controllers.SKU;

namespace WINITAPI.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreCheckReportController: WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreCheckReportBL _storeCheckReportBl;
        private readonly ISortHelper _sortHelper;
        private readonly ApiSettings _apiSettings;
        private readonly Winit.Shared.CommonUtilities.Common.CommonFunctions commonFunctions = new();
        private readonly DataPreparationController _dataPreparationController;

        public StoreCheckReportController(IServiceProvider serviceProvider,
            Winit.Modules.Store.BL.Interfaces.IStoreCheckReportBL storeCheckReportBl,
            ISortHelper sortHelper,
            IOptions<ApiSettings> apiSettings,
            JsonSerializerSettings jsonSerializerSettings,
            DataPreparationController dataPreparationController) : base(serviceProvider)
        {
            _storeCheckReportBl = storeCheckReportBl;
            _sortHelper = sortHelper;
            _apiSettings = apiSettings.Value;
            _dataPreparationController = dataPreparationController;
        }
        [HttpPost]
        [Route("GetStoreCheckReportDetails")]
        public async Task<ActionResult> GetStoreCheckReportDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreCheckReport> PagedResponse = null;
                PagedResponse = await _storeCheckReportBl.GetStoreCheckReportDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
                if (PagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponse);


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve MaintainUser details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetStoreCheckReportItems")]
        public async Task<ActionResult> GetStoreCheckReportItems(string uid)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                    return BadRequest("UID is required");

                var items = await _storeCheckReportBl.GetStoreCheckReportItems(uid);
                if (items == null)
                    return NotFound();

                return CreateOkApiResponse(items);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StoreCheckReport items");
                return CreateErrorResponse("An error occurred: " + ex.Message);
            }
        }
    }
}
