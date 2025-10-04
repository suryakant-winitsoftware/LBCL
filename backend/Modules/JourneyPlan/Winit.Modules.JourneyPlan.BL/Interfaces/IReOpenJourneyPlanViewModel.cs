using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Interfaces;

namespace Winit.Modules.JourneyPlan.BL.Interfaces
{
    public interface IReOpenJourneyPlanViewModel 
    {
        public List<IUserJourney> userJourneyList { get; set; }
        Task PopulateViewModel();
    }
}
