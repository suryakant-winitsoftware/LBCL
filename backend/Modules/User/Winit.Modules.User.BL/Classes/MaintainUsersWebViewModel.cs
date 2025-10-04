using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.User.BL.Classes
{
    public class MaintainUsersWebViewModel : MaintainUsersBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;
        public MaintainUsersWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,

            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,IAppUser appUser)
        : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService, appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;

        }
        public override async Task PopulateViewModel()
        {
            await base.PopulateViewModel();
        }
        public override async Task<List<IMaintainUser>> GetMaintainUsersDetailsGridiview()
        {
            return await GetMaintainUsersDetailsGridiviewFromAPIAsync();
        }
      
        public override async Task<bool> GetDisableDataFromGridview(IEmpDTO user, bool IsCreate)
        {
            return await CreateUpdateUserDataFromAPIAsync(user, IsCreate);
        }

        public override async Task<IEmpDTO> GetUsersDetailsforEdit(string uid)

        {
            return await GetUsersDetailsforEditDataFromAPIAsync(uid);
        }
        public override async Task<List<ISelectionItem>> GetSalesmanData(string OrgUID)
        {
            return await GetSalesmanDataFromAPIAsync(OrgUID);
        }
        private async Task<List<ISelectionItem>> GetSalesmanDataFromAPIAsync(string OrgUID)
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetEmpDropDown?orgUID={OrgUID}",
                    HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<List<SelectionItem>> Response = JsonConvert.DeserializeObject<ApiResponse<List<SelectionItem>>>(apiResponse.Data);
                    return Response.Data.ToList<ISelectionItem>();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        public async Task<List<Winit.Modules.User.Model.Interfaces.IMaintainUser>> GetMaintainUsersDetailsGridiviewFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = MaintainUserFilterCriterias;
                pagingRequest.FilterCriterias.Add(filterCriteria);
                pagingRequest.IsCountRequired = true;
                pagingRequest.SortCriterias = new List<SortCriteria>
                {
                    new SortCriteria("ModifiedTime",SortDirection.Desc)
                };
                pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}MaintainUser/SelectAllMaintainUserDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.User.Model.Classes.MaintainUser>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.User.Model.Classes.MaintainUser>>>(apiResponse.Data);
                    TotalSKUItemsCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.ToList<Winit.Modules.User.Model.Interfaces.IMaintainUser>();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<bool> CreateUpdateUserDataFromAPIAsync(Winit.Modules.User.Model.Interfaces.IEmpDTO user, bool IsCreate)
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    string jsonBody = JsonConvert.SerializeObject(user);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}MaintainUser/CUDEmployee", HttpMethod.Post, user);
                }
                else
                {
                    string jsonBody = JsonConvert.SerializeObject(user);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}MaintainUser/CUDEmployee", HttpMethod.Post, user);
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<Winit.Modules.User.Model.Interfaces.IEmpDTO> GetUsersDetailsforEditDataFromAPIAsync(string uid)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}MaintainUser/SelectMaintainUserDetailsByUID?empUID={uid}",
                    HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<Winit.Modules.User.Model.Classes.EmpDTOModel> empDTOResponse = JsonConvert.DeserializeObject<ApiResponse<Winit.Modules.User.Model.Classes.EmpDTOModel>>(apiResponse.Data);
                    if (empDTOResponse != null && empDTOResponse.IsSuccess)
                    {
                        var empDTOData = empDTOResponse.Data;
                        IEmpDTO empDTO = new EmpDTO();
                        empDTO.Emp = empDTOData.Emp ?? new Winit.Modules.Emp.Model.Classes.Emp();
                        empDTO.EmpInfo = empDTOData.EmpInfo ?? new EmpInfo();
                        return empDTO;
                    }
                    return null;
                }
                
            }
            catch(Exception ex)
            {

            }
            return null;
        }
    }
}
