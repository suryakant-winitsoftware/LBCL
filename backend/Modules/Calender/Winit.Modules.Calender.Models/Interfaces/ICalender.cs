using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Calender.Models.Interfaces
{
    public interface ICalender : IBaseModel
    {
        string PeriodName { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        int PeriodYear { get; set; }
        int PeriodNum { get; set; }
        int QuarterNum { get; set; }
    }
}
