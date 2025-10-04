using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.BL.Interfaces
{
    public interface IUserJourney_AttendanceReportViewModel
    {
        public List<IUserJourneyGrid> userJourneyGrids { get; set; }
        public IUserJourneyView userJourneyView { get; set; }
        public int PageNumber { get; set; } 
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public string OrgUID { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<FilterCriteria> UserJourney_AttendanceReportFilterCriterias { get; set; }
        Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias);
        Task GetSalesman(string OrgUID);
        Task PageIndexChanged(int pageNumber);
        Task PopulateViewModel();
        Task PopulateUserJourneyReportforView(string UID);
        Task ApplySort(SortCriteria sortCriteria);
    }
}
