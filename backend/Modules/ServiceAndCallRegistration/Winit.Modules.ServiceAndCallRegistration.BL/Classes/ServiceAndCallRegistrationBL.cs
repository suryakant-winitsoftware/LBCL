using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ServiceAndCallRegistration.BL.Interfaces;
using Winit.Modules.ServiceAndCallRegistration.Model.Classes;
using Winit.Modules.ServiceAndCallRegistration.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ServiceAndCallRegistration.BL.Classes
{
    public class ServiceAndCallRegistrationBL : IServiceAndCallRegistrationBL
    {
        protected readonly Winit.Modules.ServiceAndCallRegistration.DL.Interfaces.IServiceAndCallRegistrationDL _serviceAndCallRegistration;
        private readonly IServiceProvider? _serviceProvider = null;
        public ServiceAndCallRegistrationBL(Winit.Modules.ServiceAndCallRegistration.DL.Interfaces.IServiceAndCallRegistrationDL serviceAndCallRegistration, IServiceProvider serviceProvider)
        {
            _serviceAndCallRegistration = serviceAndCallRegistration;
            _serviceProvider = serviceProvider;
        }

        public Task<ICallRegistration> GetCallRegistrationItemDetailsByCallID(string serviceCallNumber)
        {
            return _serviceAndCallRegistration.GetCallRegistrationItemDetailsByCallID(serviceCallNumber);
        }

        public Task<PagedResponse<ICallRegistration>> GetCallRegistrations(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, 
            List<FilterCriteria> filterCriterias, bool isCountRequired, string jobPositionUID)
        {
            return  _serviceAndCallRegistration.GetCallRegistrations(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired,jobPositionUID);
        }

        public Task<int> SaveCallRegistrationDetails(CallRegistration callRegistrationDetails)
        {
            return _serviceAndCallRegistration.SaveCallRegistrationDetails(callRegistrationDetails);
        }
    }
}
