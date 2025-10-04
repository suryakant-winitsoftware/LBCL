using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.DL.Interfaces
{
    public interface IUserJourneyDL
    {
        Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney>> SelectAlUserJourneyDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);

        Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan>> SelectTodayJourneyPlanDetails(List<SortCriteria> sortCriterias, int pageNumber,
      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string Type, DateTime VisitDate, string JobPositionUID, string OrgUID);
        Task<IEnumerable<Winit.Modules.JourneyPlan.Model.Interfaces.ITodayJourneyPlanInnerGrid>> SelecteatHistoryInnerGridDetails(string BeatHistoryUID);
        Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid>> GetUserJourneyGridDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyView> GetUserJourneyDetailsByUID(string UID);


    }
}
