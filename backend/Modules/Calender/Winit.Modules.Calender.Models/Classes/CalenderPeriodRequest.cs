using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Calender.Models.Classes
{
    public class CalenderPeriodRequest
    {
        public int Period { get; set; }
        public DateTime Date { get; set; }

        public CalenderPeriodRequest(int period, DateTime dateTime)
        {
            Period = period;
            Date = dateTime;
        }
    }
}
