using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Location.BL.Classes
{
    public class LocationMappingTemplateWebViewModel : LocationMappingTemplateBaseViewModel
    {
        ApiService _apiService;
        IAppConfig _appConfigs;
        public LocationMappingTemplateWebViewModel(ApiService apiService, IAppConfig appConfigs)
        {
            _apiService = apiService;
            _appConfigs = appConfigs;
        }
        protected override async Task GetLocationTemplateDetails()
        {

            Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}LocationTemplate/SelectAllLocationTemplates", HttpMethod.Post);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<List<LocationTemplate>>? pagedResponse = JsonConvert.DeserializeObject<ApiResponse<List<LocationTemplate>>>(apiResponse.Data);
                    if (pagedResponse != null)
                    {
                        Templates = pagedResponse.Data.ToList<ILocationTemplate>();
                        //  SortedPromotionsList = PromotionsList.OrderByDescending(p => p.ModifiedTime).ToList();
                    }
                }
            }
        }
    }
}
