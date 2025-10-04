
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Classes
{
    public class OrgBaseViewModel : Winit.Modules.Org.BL.Interfaces.IOrgViewModel
    {
       

        public List<IOrg> OrgItemViews { get; set; }
         public IOrg org { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        IEnumerable<Winit.Modules.Org.Model.Classes.Org> ORGList;
        public List<FilterCriteria> OrgFilterCriterials { get; set; }
       

        //Constructor
        protected OrgBaseViewModel(IServiceProvider serviceProvider,
                IFilterHelper filter,
                ISortHelper sorter,
                IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
              )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;

            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            
            // Initialize common properties or perform other common setup
            OrgItemViews = new List<Model.Interfaces.IOrg>();
            org = new Winit.Modules.Org.Model.Classes.Org();
            OrgFilterCriterials = new List<FilterCriteria>();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }

        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
        {
            OrgFilterCriterials.Clear();
            OrgFilterCriterials.AddRange(filterCriterias);

        }
        public async Task ResetFilter()
        {
            OrgFilterCriterials.Clear();

        }

        public virtual async void Dispose()
        {
        }
        //protected async Task OnInitializedAsync()
        //{
        //    await PopulateOrgMaster();
        //}
        public async Task PopulateViewModel()
        {

            OrgItemViews = await GetOrgDataFromAPIAsync();
        }

        public async Task GetORGforEditDetailsData(string ORGUid)
        {
            org = await GetORGforEditDetailsDataFromAPIAsync(ORGUid);
        }
        public async Task SaveUpdateORGItem(IOrg org, bool Iscreate)
        {
            if (Iscreate)
            {
                org.IsActive = true;
                await CreateUpdateOrgDataFromAPIAsync(org, true);
            }
            else
            {
                await CreateUpdateOrgDataFromAPIAsync(org, false);
            }
        }
        private void AddCreateFields(IBaseModel baseModel, bool IsUIDRequired)
        {

            baseModel.CreatedBy = "7ee9f49f-26ea-4e89-8264-674094d805e1";//_appUser.Emp.UID;
            baseModel.ModifiedBy = "7ee9f49f-26ea-4e89-8264-674094d805e1";//_appUser.Emp.UID;
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
        }
        private void AddUpdateFields(IBaseModel baseModel)
        {
            baseModel.ModifiedBy = "7ee9f49f-26ea-4e89-8264-674094d805e1"; //_appUser.Emp.UID;
            baseModel.ModifiedTime = DateTime.Now;
        }
        private async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = OrgFilterCriterials;
                
                    pagingRequest.IsCountRequired = true;
                
                    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}Org/GetOrgDetails",
                        HttpMethod.Post, pagingRequest);

                    if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                    {
                        string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                        PagedResponse<Winit.Modules.Org.Model.Classes.Org> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Org.Model.Classes.Org>>(data);
                        if (selectionORGs.PagedData != null)
                        {
                        return selectionORGs.PagedData.OfType<IOrg>().ToList();
                    }
                    }
                
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        private async Task<Winit.Modules.Org.Model.Interfaces.IOrg> GetORGforEditDetailsDataFromAPIAsync(string orguid)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 1;
                pagingRequest.PageSize = int.MaxValue;

                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.IsCountRequired = true;
                ApiResponse<Winit.Modules.Org.Model.Classes.Org> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Org.Model.Classes.Org>(
                    $"{_appConfigs.ApiBaseUrl}Org/GetOrgByUID?UID={orguid}",
                    HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<bool> CreateUpdateOrgDataFromAPIAsync(Model.Interfaces.IOrg org, bool IsCreate)
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    AddCreateFields(org, true);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Org/CreateOrg", HttpMethod.Post, org);
                }
                else
                {
                    AddUpdateFields(org);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Org/UpdateOrg", HttpMethod.Put, org);
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
       
        public async Task<string> DeleteItem(string orgUID)
        {
           
            return await DeleteOrgFromAPIAsync(orgUID);
        }
        //private async Task<bool> DeleteOrgFromAPIAsync(string uid)
        //{
        //    try
        //    {
        //        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
        //            $"{_appConfigs.ApiBaseUrl}Org/DeleteOrg?UID={uid}",
        //            HttpMethod.Delete, uid);

        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        //        {
        //            return true;
        //        }
        //        else
        //        { return false; }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //}
        private async Task<string> DeleteOrgFromAPIAsync(string uid)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Org/DeleteOrg?UID={uid}",
                    HttpMethod.Delete, uid);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return "SKU successfully deleted.";
                }
                else if (apiResponse != null && apiResponse.Data != null)
                {
                    ApiResponse<string> data = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                    return $"Error Failed to delete Item. Error: {data.ErrorMessage}";
                }
                else
                {
                    return "An unexpected error occurred.";
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
