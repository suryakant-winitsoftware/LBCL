using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Calender.Models.Interfaces;

namespace Winit.Modules.Calender.Models.Classes
{
    public class Calender : BaseModel, ICalender
    {
        public string PeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PeriodYear { get; set; }
        public int PeriodNum { get; set; }
        public int QuarterNum { get; set; }
    }
}
