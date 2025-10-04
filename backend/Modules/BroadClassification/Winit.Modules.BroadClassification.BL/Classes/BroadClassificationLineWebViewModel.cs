using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
namespace Winit.Modules.BroadClassification.BL.Classes
{
    public class BroadClassificationLineWebViewModel : BroadClassificationLineBaseViewModel
    {
        protected IAppUser _iAppUser { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly ILanguageService _languageService;
        private IStringLocalizer<LanguageKeys> _localizer;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appuser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;

        public BroadClassificationLineWebViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IAppUser appuser,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService, IStringLocalizer<LanguageKeys> Localizer,
        ILanguageService languageService
         ) : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appuser = appuser;
            _localizer = Localizer;
            _languageService = languageService;
            //WareHouseItemViewList = new List<IOrgType>();
        }

        public override async Task<bool> CreateUpdateBroadClassificationLineData(IBroadClassificationLine broadClassificationLine, bool Operation)
        {
            return await CreateUpdateBroadClassificationLineDataasync(broadClassificationLine, Operation);
        }

        public override async Task<string> DeleteBroadClassificationLineData(IBroadClassificationLine broadClassificationline)
        {
            return await DeleteBroadClassificationLineDataAsync(broadClassificationline);
        }
        public override async Task<List<IBroadClassificationLine>> GetBroadClassificationLineData()
        {
            return await GetBroadClassificationLineDataFromAPIAsync();
        }

        public override async Task<IBroadClassificationLine> GetBroadClassificationLineDetailsByUID(string UID)
        {
            return await GetBroadClassificationLineDetailsByUIDFromApiAsync(UID);
        }



        private async Task<List<IBroadClassificationLine>> GetBroadClassificationLineDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = broadClassificationLineDataFilterCriterias;
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}BroadClassificationLine/GetBroadClassificationLineDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.BroadClassification.Model.Classes.BroadClassificationLine>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.BroadClassification.Model.Classes.BroadClassificationLine>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine>().ToList();
                }
            }
            catch (Exception ex)
            {

                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }

        private async Task<IBroadClassificationLine> GetBroadClassificationLineDetailsByUIDFromApiAsync(string uID)
        {
            throw new NotImplementedException();
        }
        private async Task<bool> CreateUpdateBroadClassificationLineDataasync(IBroadClassificationLine broadClassificationLine, bool operation)
        {
            /// CreateBroadClassificationLine
            try
            {
                ApiResponse<string> apiResponse;
                broadClassificationLine.UID = Guid.NewGuid().ToString();
                broadClassificationLine.CreatedTime = DateTime.Now;
                broadClassificationLine.ModifiedTime = DateTime.Now;
                broadClassificationLine.CreatedBy = _appuser.Emp.UID ?? "";
                broadClassificationLine.ModifiedBy = _appuser.Emp?.UID ?? "";
                apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}BroadClassificationLine/CreateBroadClassificationLine",
                    HttpMethod.Post, broadClassificationLine);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;

            }
        }

        private async Task<string> DeleteBroadClassificationLineDataAsync(IBroadClassificationLine broadClassificationline)
        {
            try
            {
                broadClassificationline.CreatedBy=_appuser.Emp.CreatedBy;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}BroadClassificationLine/DeleteBroadClassificationLine",
                    HttpMethod.Delete, broadClassificationline);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return "Success";
                }
                else if (apiResponse != null && apiResponse.Data != null)
                {
                    ApiResponse<string> data = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                    return $"Failed To Delete: {data.ErrorMessage}";
                }
                else
                {
                    return "Un Expected Error ";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
