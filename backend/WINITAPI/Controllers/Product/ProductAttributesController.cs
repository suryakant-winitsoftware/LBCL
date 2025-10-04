using System;
using System.Collections.Generic;
using System.IO;
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
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITRepository.Interfaces.Customers;
using WINITServices.Classes.CacheHandler;

namespace WINITAPI.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductAttributesController : WINITBaseController
    {
        private readonly Winit.Modules.Product.BL.Interfaces.IProductAttributesBL _productAttributesBL;
        public ProductAttributesController(IServiceProvider serviceProvider, 
            Winit.Modules.Product.BL.Interfaces.IProductAttributesBL productAttributesBL) 
            : base(serviceProvider)
        {
            _productAttributesBL = productAttributesBL;
        }

        [HttpPost]
        [Route("SelectProductAttributesAll")]
        public async Task<ActionResult> SelectProductAttributesAll(PagingRequest pagingRequest)
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
                var cacheKey = CacheConstants.ALL_ProductAttributes;
                PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductAttributes> PagedResponseProductAttributesList = null;
                PagedResponseProductAttributesList = null;

                if (PagedResponseProductAttributesList != null)
                {
                    return CreateOkApiResponse(PagedResponseProductAttributesList);
                }
                PagedResponseProductAttributesList = await _productAttributesBL.SelectProductAttributesAll(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseProductAttributesList == null)
                {
                    return NotFound();
                }
                else
                {
                    //currencyListResponse = currencyList.OfType<object>().ToList();
                    //_cacheService.Set(cacheKey, currencyListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return CreateOkApiResponse(PagedResponseProductAttributesList);

            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductAttributesDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


    }
}

