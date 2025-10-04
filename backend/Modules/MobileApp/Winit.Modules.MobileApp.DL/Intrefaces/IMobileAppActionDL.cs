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
    public interface IMobileAppActionDL
    {
        Task<PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>> GetClearDataDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<IEnumerable<Winit.Modules.Mobile.Model.Interfaces.IUser>> GetUserDDL(string OrgUID);
        Task<int> PerformCUD(List<Winit.Modules.Mobile.Model.Classes.MobileAppAction> mobileAppActions);
        Task<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction> GetMobileAppAction(string userName);
        Task<int> InitiateDBCreation(string employeeUID, string jobPositionUID, string roleUID, string orgUID, string vehicleUID, string empCode);
        Task<Winit.Modules.Mobile.Model.Interfaces.ISqlitePreparation> GetDBCreationStatus(string employeeUID, string jobPositionUID, string roleUID);
    }
}
