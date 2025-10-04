using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ServiceAndCallRegistration.Model.Classes;
using Winit.Modules.ServiceAndCallRegistration.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ServiceAndCallRegistration.DL.Interfaces
{
    public interface IServiceAndCallRegistrationDL
    {
        Task<ICallRegistration> GetCallRegistrationItemDetailsByCallID(string serviceCallNumber);
        Task<PagedResponse<ICallRegistration>> GetCallRegistrations(List<SortCriteria> sortCriterias, 
            int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string jobPositionUID);
        Task<int> SaveCallRegistrationDetails(CallRegistration callRegistrationDetails);
    }
}
