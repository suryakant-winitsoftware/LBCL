using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Calender.BL.Interfaces;
using Winit.Modules.Calender.DL.Interface;
using Winit.Modules.Calender.Models.Interfaces;

namespace Winit.Modules.Calender.BL.Classes
{
    public class CalenderBL : ICalenderBL
    {
        ICalenderDL _calenderDL;
        public CalenderBL(ICalenderDL calenderDL)
        {
            _calenderDL = calenderDL;
        }
        public async Task<IList<ICalender>> GetCalenderPeriods(DateTime date, int calendarPeriod)
        {
            return await _calenderDL.GetCalenderPeriods(date, calendarPeriod);
        }
    }
}
