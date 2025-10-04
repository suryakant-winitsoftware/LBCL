using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JobPosition.Model.Interfaces;

namespace Winit.Modules.DashBoard.BL.Interfaces
{
    public interface IDashBoardViewModel
    {
        public DateTime JourneyStartDate { get; set; }
        public Winit.Modules.Common.Model.Interfaces.IMyRole SelectedRole { get; set; }
        public List<Winit.Modules.Common.Model.Interfaces.IMyRole> Roles { get; set; }
        public Winit.Modules.Route.Model.Interfaces.IRoute SelectedRoute { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRoute> RouteList { get; set; }
        public List<Winit.Modules.Org.Model.Interfaces.IOrg> MyOrgs { get; set; }
        public Winit.Modules.Org.Model.Interfaces.IOrg SelectedOrg { get; set; }
        public Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory SelectedBeatHistory { get; set; }
        public List<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> BeatHistoryList { get; set; }
        public Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance JobPositionAttendance { get; set; }
        public string StartDayBtnText { get; set; }
        void StartDayInitiate();
        public Task GetUserAttendence(string jobPositionUID, string empUID);
        public Task<bool> UpdateJobPositionAttendance(JobPosition.Model.Interfaces.IJobPositionAttendance jobPositionAttendance);
        Task PopulateCacheData();
        Task<IJobPositionAttendance> GetTotalAssignedAndVisitedStores();
    }
}
