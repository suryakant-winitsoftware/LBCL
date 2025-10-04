using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.JourneyPlan.BL.Interfaces
{
    public interface IJourneyPlanViewModelFactory  : IDisposable
    {
        public IJourneyPlanViewModel _viewmodelJp { get;  set; }
        public IJourneyPlanViewModel CreateJourneyPlanViewModel(string ScreenType);
    }
}
