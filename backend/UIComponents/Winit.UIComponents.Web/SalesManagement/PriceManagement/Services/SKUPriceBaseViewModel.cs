using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.UIComponents.Web.SalesManagement.PriceManagement.Services
{
    public class SKUPriceBaseViewModel : ISKUPriceBaseViewModel
    {
        ApiService _apiService;
        IAppConfig _appConfigs;
        CommonFunctions _commonFunctions;
        public SKUPriceBaseViewModel(ApiService apiService, IAppConfig appConfig, CommonFunctions commonFunctions)
        {
            _apiService = apiService;
            _appConfigs = appConfig;
            _commonFunctions = commonFunctions;
        }

        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SKULIst { get; set; }

        public async Task GetAllProducts()
        {
            Winit.Shared.Models.Common.PagingRequest pagingRequest = new Winit.Shared.Models.Common.PagingRequest();
            pagingRequest.PageNumber = 1;
            pagingRequest.PageSize = 1000;
            pagingRequest.IsCountRequired = true;
            Winit.Shared.Models.Common.ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKU>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKU>>(
                    $"{_appConfigs.ApiBaseUrl}SKU/SelectAllSKUDetails",
                    HttpMethod.Post, pagingRequest);


            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                SKULIst = apiResponse?.Data?.PagedData?.ToList();
            }
            else
            {
            }
        }

        public async Task<int> SaveUpdateOrDelete(string responseMethod, HttpMethod httpMethod, object obj)
        {
            int count = 0;
            string response = string.Empty;
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                  $"{_appConfigs.ApiBaseUrl}SKUPrice/{responseMethod}",
                  httpMethod, obj);
            if (apiResponse != null)
            {
                if (apiResponse.StatusCode == 200)
                {
                    response = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                    count = Winit.Shared.CommonUtilities.Common.CommonFunctions.GetIntValue(response);
                    if (count > 0)
                    {
                        ///Message = "Saved Successfully !";
                    }
                    else
                    {
                        //Message = response;
                    }
                }
                else
                {
                    //Message = apiResponse.ErrorMessage;
                }
            }
            return count;
        }

    }
}
