using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
namespace Winit.Modules.JourneyPlan.Model.Interfaces;

public interface IUserJourneyGrid 
{
    public string UID { get; set; }

    public string User { get; set; }
    public DateTime JourneyDate { get; set; }
    public string? EOTStatus { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

