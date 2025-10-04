using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.SKU.BL.Classes
{
    public class ManageUserSKUMappingTemplateWebViewModel: ManageUserSKUMappingTemplateBaseViewModel
    {
        ApiService _apiService;
        IAppConfig _appConfigs;
        
        public ManageUserSKUMappingTemplateWebViewModel(ApiService apiService, IAppConfig appConfigs)
        {
            _apiService = apiService;
            _appConfigs = appConfigs;
        }
        protected override async Task GetLocationTemplateDetails()
        {
            PagingRequest pagingRequest = new PagingRequest();
            Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}SKUTemplate/SelectAllSKUTemplateDetails",
                                                                                    HttpMethod.Post, pagingRequest);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    PagedResponse<SKUTemplate>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<SKUTemplate>>(new CommonFunctions().GetDataFromResponse(apiResponse.Data));
                    if (pagedResponse != null && pagedResponse.PagedData!=null)
                    {
                        Templates = pagedResponse.PagedData.ToList();
                        //  SortedPromotionsList = PromotionsList.OrderByDescending(p => p.ModifiedTime).ToList();
                    }
                }
            }
        }

    }
}
