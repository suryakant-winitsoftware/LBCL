using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mobile.DL.Interfaces
{
    public interface IAppVersionUserDL
    {
        Task<PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser>> GetAppVersionDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string OrgUID);
        Task<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> GetAppVersionDetailsByUID(string UID);
        Task<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> GetAppVersionDetailsByEmpUID(string empUID);
        Task<int> UpdateAppVersionDetails(Winit.Modules.Mobile.Model.Classes.AppVersionUser appVersionUser);
        
        /// <summary>
        /// Inserts a new app version user record
        /// </summary>
        /// <param name="appVersionUser">App version user data to insert</param>
        /// <returns>Number of rows affected (1 if successful, 0 if failed)</returns>
        Task<int> InsertAppVersionUser(Winit.Modules.Mobile.Model.Classes.AppVersionUser appVersionUser);
    }
}
