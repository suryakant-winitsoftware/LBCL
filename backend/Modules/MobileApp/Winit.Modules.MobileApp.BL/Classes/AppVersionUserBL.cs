using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mobile.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mobile.BL.Classes
{
    public class AppVersionUserBL : IAppVersionUserBL
    {
        protected readonly Modules.Mobile.DL.Interfaces.IAppVersionUserDL _appVersionUserDL = null;
        public AppVersionUserBL(Modules.Mobile.DL.Interfaces.IAppVersionUserDL appVersionUserDL)
        {
            _appVersionUserDL = appVersionUserDL;
        }
        public async Task<PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser>> GetAppVersionDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired,string OrgUID)
        {
            return await _appVersionUserDL.GetAppVersionDetails(sortCriterias, pageNumber,pageSize, filterCriterias, isCountRequired, OrgUID);
        }
        public async Task<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> GetAppVersionDetailsByUID(string UID)
        {
            return await _appVersionUserDL.GetAppVersionDetailsByUID(UID);
        }
        public async Task<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> GetAppVersionDetailsByEmpUID(string empUID)
        {
            return await _appVersionUserDL.GetAppVersionDetailsByEmpUID(empUID);
        }
        public async Task<int> UpdateAppVersionDetails(Winit.Modules.Mobile.Model.Classes.AppVersionUser appVersionUser)
        {
            return await _appVersionUserDL.UpdateAppVersionDetails(appVersionUser);
        }

        /// <summary>
        /// Inserts a new app version user record
        /// </summary>
        /// <param name="appVersionUser">App version user data to insert</param>
        /// <returns>Number of rows affected (1 if successful, 0 if failed)</returns>
        public async Task<int> InsertAppVersionUser(Winit.Modules.Mobile.Model.Classes.AppVersionUser appVersionUser)
        {
            return await _appVersionUserDL.InsertAppVersionUser(appVersionUser);
        }
    }
}
