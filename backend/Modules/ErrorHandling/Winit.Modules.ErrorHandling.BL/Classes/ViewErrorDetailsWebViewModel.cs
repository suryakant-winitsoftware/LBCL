using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ErrorHandling.BL.Classes
{
    public class ViewErrorDetailsWebViewModel : ViewErrorDetailsBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        public List<FilterCriteria> FilterCriterias { get; set; }

        //private readonly IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public ViewErrorDetailsWebViewModel(IServiceProvider serviceProvider,
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

        public override async Task PopulateViewModel()
        {
            await base.PopulateViewModel();
        }
        public override async Task<List<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail>> GetErrorDetailsData()
        {
            return await GetBankDetailsDataDataFromAPIAsync();
        }
        

        private async Task<List<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail>> GetBankDetailsDataDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                
                pagingRequest.FilterCriterias = ErrorDetailsFilterCriteria;
                pagingRequest.SortCriterias = ErrorDetailsSortCriterias;
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}KnowledgeBase/GetErrorDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.ErrorHandling.Model.Classes.ErrorDetail>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.ErrorHandling.Model.Classes.ErrorDetail>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail>().ToList();
                }
            }
            catch (Exception e)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
    }
}
