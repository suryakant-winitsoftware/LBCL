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
    public class MobileAppActionBL : IMobileAppActionBL
    {
        protected readonly Modules.Mobile.DL.Interfaces.IMobileAppActionDL _mobileAppActionDL = null;
        public MobileAppActionBL(Modules.Mobile.DL.Interfaces.IMobileAppActionDL mobileAppActionDL)
        {
            _mobileAppActionDL = mobileAppActionDL;
        }
        public async Task<PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>> GetClearDataDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _mobileAppActionDL.GetClearDataDetails(sortCriterias, pageNumber,pageSize, filterCriterias, isCountRequired);
        }

        public async Task<IEnumerable<Winit.Modules.Mobile.Model.Interfaces.IUser>> GetUserDDL(string OrgUID)
        {
            return await _mobileAppActionDL.GetUserDDL(OrgUID);
        }
        public async Task<int> PerformCUD(List<Winit.Modules.Mobile.Model.Classes.MobileAppAction> mobileAppActions)
        {
            return await _mobileAppActionDL.PerformCUD(mobileAppActions);
        }

        public async Task<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction> GetMobileAppAction(string empUID)
        {
            return await _mobileAppActionDL.GetMobileAppAction(empUID);
        }
        public async Task<int> InitiateDBCreation(string employeeUID, string jobPositionUID, string roleUID, string orgUID, string vehicleUID, string empCode)
        {
            return await _mobileAppActionDL.InitiateDBCreation(employeeUID, jobPositionUID, roleUID, orgUID, vehicleUID, empCode);
        }
        public async Task<Winit.Modules.Mobile.Model.Interfaces.ISqlitePreparation> GetDBCreationStatus(string employeeUID, string jobPositionUID, string roleUID)
        {
            return await _mobileAppActionDL.GetDBCreationStatus(employeeUID, jobPositionUID, roleUID);
        }
    }
}
