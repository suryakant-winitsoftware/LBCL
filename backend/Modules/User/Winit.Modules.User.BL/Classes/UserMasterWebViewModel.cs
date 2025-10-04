
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.util;
using Winit.Modules.Base.BL;
using Winit.Modules.Calender.Models.Classes;
using Winit.Modules.Calender.Models.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;

namespace Winit.Modules.User.BL.Classes
{
    public class UserMasterWebViewModel : UserMasterBaseViewModel
    {
        IAppConfig _appConfig;
        ApiService _apiService;
        public UserMasterWebViewModel(IAppConfig appConfig, ApiService apiService, IAppUser appUser, IAppSetting appSetting,
            Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView ModulesMasterHierarchy)
        {
            _appConfig = appConfig;
            _apiService = apiService;
            _appUser = appUser;
            _appSetting = appSetting;
            _ModulesMasterHierarchy = ModulesMasterHierarchy;
        }
        public override async Task GetUserMasterData(string logInID)
        {
            Winit.Shared.Models.Common.ApiResponse<IUserMaster> apiResponse = await _apiService.FetchDataAsync<IUserMaster>($"{_appConfig.ApiBaseUrl}" +
                $"MaintainUser/GetAllUserMasterDataByLoginID?LoginID={logInID}", HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess == true && apiResponse.Data != null)
            {
                SetAppUser(apiResponse.Data);
                await GetCalenderPeriods();
            }

        }
        private async Task GetCalenderPeriods()
        {
            
            _appUser.CalenderPeriods = [];
            ApiResponse<List<ICalender>> apiResponse = await _apiService.FetchDataAsync<List<ICalender>>($"{_appConfig.ApiBaseUrl}" +
                $"Calender/GetCalenderPeriods" , HttpMethod.Post, new CalenderPeriodRequest(_appSetting.SchemeCalendarPeriod, DateTime.Now.Date));
            if (apiResponse != null && apiResponse.IsSuccess == true && apiResponse.Data != null)
            {
                _appUser.CalenderPeriods.AddRange(apiResponse.Data);
            }
        }

    }
}
