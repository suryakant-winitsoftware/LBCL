using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Winit.Shared.Models.Common;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.PriceLadder.Model.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WINITAPI.Controllers.PriceLadder
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PriceLadderingController : WINITBaseController
    {
        private readonly Winit.Modules.PriceLadder.BL.Interfaces.ISKUPriceLadderingBL _sKUPriceLadderingBL;
        public PriceLadderingController(IServiceProvider serviceProvider, 
            Winit.Modules.PriceLadder.BL.Interfaces.ISKUPriceLadderingBL sKUPriceLadderingBL) 
            : base(serviceProvider)
        {
            _sKUPriceLadderingBL = sKUPriceLadderingBL;
        }

        [HttpPost]
        [Route("SelectAllThePriceLaddering")]
        public async Task<IActionResult> GetAllThePriceLaddering(PagingRequest pagingRequest)
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

                var pagedResponse = await _sKUPriceLadderingBL.SelectAllThePriceLaddering(pagingRequest.SortCriterias,
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
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }


        }

        [HttpPost]
        [Route("GetPriceLadders")]
        public async Task<IActionResult> GetPriceLadders(PagingRequest pagingRequest)
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
                var pagedResponse = await _sKUPriceLadderingBL.GetPriceLadders(pagingRequest.SortCriterias,
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
                return CreateErrorResponse("An error occurred while retrieving price laddering data: " + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetRelatedData")]
        public async Task<IActionResult> GetRelatedData([FromBody] IPriceLaddering priceLaddering)
        {
            try
            {
                var relatedData = await _sKUPriceLadderingBL.GetRelatedData(priceLaddering.OperatingUnit, priceLaddering.Division , priceLaddering.Branch , priceLaddering.SalesOffice , priceLaddering.BroadCustomerClassification);

                if (relatedData == null || relatedData.Count == 0)
                {
                    return NotFound("No related price laddering data found.");
                }

                return CreateOkApiResponse(relatedData);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("An error occurred while retrieving related data: " + ex.Message);
            }
        }
        [HttpPost]
        [Route("GetSkuDetailsFromProductCategoryId")]
        public async Task<IActionResult> GetSkuDetailsFromProductCategoryId([FromBody] int ProductcategoryId)
        {
            try
            {
                var relatedData = await _sKUPriceLadderingBL.GetSkuDetailsFromProductCategoryId(ProductcategoryId);

                if (relatedData == null)
                {
                    return NotFound("No related price laddering Sku Details found.");
                }

                return CreateOkApiResponse(relatedData);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("An error occurred while retrieving related data: " + ex.Message);
            }
        }

    }
}
