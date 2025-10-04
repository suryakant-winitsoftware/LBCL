using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Route.Model.Interfaces;

namespace Winit.Modules.JourneyPlan.BL.Interfaces
{
    public interface ICFDViewModel
    {
        public bool IsOnline { get; set; }
        public string UserJourneyUID { get; set; }
        public int StartReading { get; set; }
        public int EndReading { get; set; }
        public string UploadStatus { get; set; }
        public string EOTStatus { get; set; }
        public string AuditStatus { get; set; }
        public DateTime? JourneyStartTime { get; set; }
        public DateTime? JourneyEndTime { get; set; }
        public bool HasAuditCompleted { get; set; }
        public int NonVisited { get; set; }
        public bool IsStockAuditCompleted { get; set; }
        public bool IsSyncPushPending { get; set; }
        // public WinIT.mSFA.Shared.sFAModel.VehicleStatus OldSelectedVehicleStatus { get; set; }
        public string WHStockRequestUID { get; set; }
        public bool IsOldRouteOpen { get; set; }
        public IBeatHistory beatHistoryToClose { get; set; }
        public IUserJourney UserJourney { get; set; }
        public IRoute SelectedRoute { get; set; }
        public string StartDayStatus { get; set; }
        public IBeatHistory SelectedBeatHistory { get; set; }
        public List<Winit.Modules.Store.Model.Interfaces.IStandardListSource> StandardListSourceList { get; set; }

        // methodes
        Task PopulateViewModel();
        Task UpdateUploadStatus();
        Task DeleteUserJourneyColumns();

        Task<bool> UpdateBeatHistoryAndUserJourney();
    }
}
