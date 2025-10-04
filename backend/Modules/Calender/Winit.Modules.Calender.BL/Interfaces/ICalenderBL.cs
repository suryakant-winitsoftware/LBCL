using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Calender.Models.Interfaces;

namespace Winit.Modules.Calender.BL.Interfaces
{
    public interface ICalenderBL
    {
        Task<IList<ICalender>> GetCalenderPeriods(DateTime date, int calendarPeriod);
    }
}
