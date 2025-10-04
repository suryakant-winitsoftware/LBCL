using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.StoreActivity.Model.Classes;

namespace Winit.Modules.JourneyPlan.Model.Classes
{
    public class MasterDTO
    {
        public List<StoreHistory>? StoreHistoryList { get; set; }
        public List<StoreHistoryStats>? StoreHistoryStatsList { get; set; }
        public List<JobPositionAttendance>? JobPositionAttendenceList { get; set; }
        public List<UserJourney>? UserJourneyList { get; set; }
        public List<ExceptionLog>? ExceptionLogList { get; set; }
        public List<BeatHistory>? BeatHistoryList { get; set; }
        public List<StoreActivityHistory>? StoreActivityHistoryList { get; set; }
        public List<Winit.Modules.Address.Model.Classes.Address>? AddressList { get; set; }
        public List<Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor>? CaptureCompetitorList { get; set; }

        public Dictionary<string, List<string>>? RequestUIDDictionary { get; set; }
        }
    }
