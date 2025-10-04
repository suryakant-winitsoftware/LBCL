using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.BL.Classes
{
    public  class UserJourneyBL : Winit.Modules.JourneyPlan.BL.Interfaces.IUserJourneyBL
    {
        protected readonly DL.Interfaces.IUserJourneyDL _userJourneyDL;
        public UserJourneyBL(DL.Interfaces.IUserJourneyDL userJourneyDL)
        {
            _userJourneyDL = userJourneyDL;
        }
      public async Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney>> SelectAlUserJourneyDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _userJourneyDL.SelectAlUserJourneyDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan>> SelectTodayJourneyPlanDetails(List<SortCriteria> sortCriterias, int pageNumber,
      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string Type, DateTime VisitDate, string JobPositionUID, string OrgUID)
        {
            return await _userJourneyDL.SelectTodayJourneyPlanDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, Type, VisitDate, JobPositionUID, OrgUID);
        }

        public async Task<IEnumerable<Winit.Modules.JourneyPlan.Model.Interfaces.ITodayJourneyPlanInnerGrid>> SelecteatHistoryInnerGridDetails(string BeatHistoryUID)
        {
            return await _userJourneyDL.SelecteatHistoryInnerGridDetails(BeatHistoryUID);
        }
        public async Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid>> GetUserJourneyGridDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _userJourneyDL.GetUserJourneyGridDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyView> GetUserJourneyDetailsByUID(string UID)
        {
           return await _userJourneyDL.GetUserJourneyDetailsByUID(UID);
        }


    }
}
