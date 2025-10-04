using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Shared.Models.Events;

namespace Winit.Modules.JourneyPlan.BL.Interfaces;

public interface IBeatHistoryViewModel
{
    Task Load();
    Task OnRouteDD_Select(DropDownEvent dropDownEvent);
    Task StopBeatHistory(IBeatHistory beatHistory, string stockAuditUID);
}
