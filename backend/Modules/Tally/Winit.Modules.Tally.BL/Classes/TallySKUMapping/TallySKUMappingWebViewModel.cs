using Azure;
using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Tally.BL.Classes.TallySKUMapping
{
    public class TallySKUMappingWebViewModel : TallySKUMappingBaseViewModel
    {
        protected readonly ApiService _apiService;
        
        public TallySKUMappingWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
        {
            _apiService = apiService;
        }
        public override async Task<List<ITallySKU>> GetAllTallySKUDetails()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.SortCriterias = SortCriterias;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;

                Winit.Shared.Models.Common.ApiResponse<string> BroadClassificationDetails = await
                _apiService.FetchDataAsync
                    ($"{_appConfig.ApiBaseUrl}Tally/GetAllTallySKU", HttpMethod.Post, pagingRequest);
                if (BroadClassificationDetails != null && BroadClassificationDetails.IsSuccess)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallySKU>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallySKU>>>(BroadClassificationDetails.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Tally.Model.Classes.TallySKU>().ToList<ITallySKU>();
                    //TotalCount = BroadClassificationDetails.Data.TotalCount;
                    //return BroadClassificationDetails.Data.PagedData.ToList<Model.Classes.Store>();
                }
                return default;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override async Task<List<ITallySKUMapping>> GetAllSKUMappingDetailsByDistCode(string Code, string Tab)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.SortCriterias = SortCriterias;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;
                Winit.Shared.Models.Common.ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallySKUMapping>> responseTab = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Tally.Model.Classes.TallySKUMapping>>
                ($"{_appConfig.ApiBaseUrl}Tally/GetAllTallySKUMappingByDistCode?Code=" + Code + "&Tab=" + Tab, HttpMethod.Post, pagingRequest);
                if (responseTab != null && responseTab.IsSuccess)
                {
                    TotalCount = responseTab.Data.TotalCount;
                    return responseTab.Data.PagedData.ToList<ITallySKUMapping>();
                }
                return default;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override async Task<List<ISKUV1>> GetAllSKUDetailsByOrgUID(string Code)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = 1000000;
                pagingRequest.SortCriterias = SortCriterias;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;
                Winit.Shared.Models.Common.ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUV1>> responseTab = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUV1>>
                ($"{_appConfig.ApiBaseUrl}SKU/SelectAllSKUDetails", HttpMethod.Post, pagingRequest);
                if (responseTab != null && responseTab.IsSuccess)
                {
                    TotalCount = responseTab.Data.TotalCount;   
                    return responseTab.Data.PagedData.ToList<ISKUV1>();
                }
                return default;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override async Task<bool> InsertTallySKUMapping(List<ITallySKUMapping> tallySKUMapping)
        {
            try
            {
                ApiResponse<bool> apiResponse = await _apiService.FetchDataAsync<bool>
                ($"{_appConfig.ApiBaseUrl}Tally/UpdateTallySKUMapping", HttpMethod.Put, tallySKUMapping);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public override async Task<List<IEmp>> GetAllDistributors()
        {
            try
            {
                ApiResponse<List<Winit.Modules.Emp.Model.Classes.Emp>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Emp.Model.Classes.Emp>>
                ($"{_appConfig.ApiBaseUrl}Tally/GetAllDistributors", HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess)
                {
                    List<Winit.Modules.Emp.Model.Classes.Emp> distributors = apiResponse.Data.ToList();
                    return distributors.ToList<IEmp>();
                }
                return default;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

    }
}
