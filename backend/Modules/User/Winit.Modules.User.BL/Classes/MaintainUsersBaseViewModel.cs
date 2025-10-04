using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.User.BL.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.User.BL.Classes
{
    public abstract class MaintainUsersBaseViewModel : IMaintainUsersViewModel
    {
        public List<IMaintainUser> maintainUsersList { get; set; }
        public IMaintainUser maintainUser { get; set; }
        public EmpDTOModel EmpDTOModelmaintainUser { get; set; }
        public IEmpDTO EmpDTOmaintainUser { get; set; }
        public IEmp empUser { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalSKUItemsCount { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<FilterCriteria> MaintainUserFilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        public IEmpInfo empInfoUser { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        protected FilterCriteria filterCriteria;
        IAppUser _appUser;
        public MaintainUsersBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,
            IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser=appUser;

            EmpSelectionList = new List<ISelectionItem>();
            maintainUsersList = new List<IMaintainUser>();
            MaintainUserFilterCriterias = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
            filterCriteria = new("OrgUID",_appUser.SelectedJobPosition.OrgUID,FilterType.Equal);
        }
        public async Task<bool> CheckUserExistsAsync(string code, string loginId, string empNo)
        {
            if(maintainUsersList == null || maintainUsersList.Count<=0)
            {
                maintainUsersList.AddRange(await GetMaintainUsersDetailsGridiview() ?? new());

            }
            return await Task.FromResult(
                maintainUsersList.Any(u => u.Code == code || u.LoginId == loginId || u.EmpNo == empNo)
            );
        }
        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
        {
            MaintainUserFilterCriterias.Clear();
            MaintainUserFilterCriterias.AddRange(filterCriterias);
            await PopulateViewModel();
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateViewModel();
        }
        public async Task GetSalesman(string OrgUID)
        {
            EmpSelectionList.Clear();
            EmpSelectionList.AddRange(await GetSalesmanData(OrgUID));
        }
        public virtual async Task PopulateViewModel()
        {
            maintainUsersList.Clear();
            maintainUsersList.AddRange(await GetMaintainUsersDetailsGridiview()??new());
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
        public async Task PopulateUsersDetailsforEdit(string Uid)
        {
            EmpDTOmaintainUser = await GetUsersDetailsforEdit(Uid);
        }
        public async Task DisableDataFromGridview(IEmpDTO user, bool IsCreate)
        {
            if (user.Emp.Status == "Active")
            {
                user.Emp.Status = "Inactive";
                await GetDisableDataFromGridview(user, false);
            }
            else
            {
                user.Emp.Status = "Active";
                await GetDisableDataFromGridview(user, false);
            }

        }

        public abstract Task<List<IMaintainUser>> GetMaintainUsersDetailsGridiview();
        public abstract Task<IEmpDTO> GetUsersDetailsforEdit(string uid);
        public abstract Task<bool> GetDisableDataFromGridview(IEmpDTO user, bool IsCreate);
        public abstract Task<List<ISelectionItem>> GetSalesmanData(string OrgUID);
        public virtual async Task<string> UpdateNewPassword(IChangePassword changePassword)
        {
            try
            {
                //changePassword.NewPassword = await GetEncryptedText(changePassword.NewPassword);
                ApiResponse<string> apiResponse =
                await _apiService.FetchDataAsync<string>(
                    $"{_appConfigs.ApiBaseUrl}Auth/UpdateNewPassword",
                    HttpMethod.Post, changePassword);
                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess ? apiResponse.Data : throw new Exception(apiResponse.ErrorMessage);
                }
                return "failed to send the request";
            }
            catch (Exception ex)
            {
                throw new Exception(" Error occured while changing the password");
            }
        }

        public async Task<string> GetEncryptedText(string text)
        {
            try
            {
                ApiResponse<string> apiResponse =
                await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Auth/GetEncryptedText?text={text}",
                    HttpMethod.Get);
                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess ? apiResponse.Data : apiResponse.ErrorMessage;
                }
                return "failed to send the request";
            }
            catch (Exception ex)
            {
                return "failed to send mail";
            }
        }
    }
}
