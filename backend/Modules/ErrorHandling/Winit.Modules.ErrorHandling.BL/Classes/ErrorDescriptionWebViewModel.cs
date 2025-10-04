using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.ErrorHandling.Model.Classes;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ErrorHandling.BL.Classes
{
    public class ErrorDescriptionWebViewModel : ErrorDescriptionBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        //private readonly IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public ErrorDescriptionWebViewModel(IServiceProvider serviceProvider,
               IFilterHelper filter,
               ISortHelper sorter,
               //   IAppUser appUser,
               IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
             ) : base(serviceProvider, filter, sorter, /*appUser,*/ listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            //  _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            //WareHouseItemViewList = new List<IOrgType>();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public override async Task<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDescriptionDetails>? GetErrorDescriptionDatailsData(string error_code)
        {
            return await GetErrorDescriptionDataFromAPIAsync(error_code);

        }

        private async Task<IErrorDescriptionDetails>? GetErrorDescriptionDataFromAPIAsync(string error_code)
        {
            try
            {
                ApiResponse<Winit.Modules.ErrorHandling.Model.Classes.ErrorDescriptionDetailsDTO> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.ErrorHandling.Model.Classes.ErrorDescriptionDetailsDTO>(
                    $"{_appConfigs.ApiBaseUrl}KnowledgeBase/GetErrorDescriptionDetailsByErroCode?errorCode={error_code}",
                    HttpMethod.Post);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null )
                {
                    IErrorDescriptionDetails data = new ErrorDescriptionDetails
                    {
                        errorDetail = apiResponse.Data.errorDetail ?? new(),
                        errorDetailsLocalizationList = apiResponse.Data.errorDetailsLocalizationList?.ToList<IErrorDetailsLocalization>() ?? new List<IErrorDetailsLocalization>()
                    };
                    return data;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
    }
}
