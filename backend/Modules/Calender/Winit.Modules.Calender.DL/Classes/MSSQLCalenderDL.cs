using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Calender.DL.Interface;
using Winit.Modules.Calender.Models.Interfaces;
using Winit.Shared.Models.Constants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Winit.Modules.Calender.DL.Classes
{
    public class MSSQLCalenderDL : SqlServerDBManager, ICalenderDL
    {
        public MSSQLCalenderDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<IList<ICalender>> GetCalenderPeriods(DateTime date, int calendarPeriod)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"CalendarPeriod",  calendarPeriod},
                    {"Date",date }
            };
                string sql = @"
                         WITH NextPeriods AS (
    SELECT *,
           ROW_NUMBER() OVER (ORDER BY start_date) AS row_num
    FROM dbo.calendar_period
    WHERE start_date >= (
        SELECT MIN(start_date)
        FROM dbo.calendar_period
        WHERE @Date BETWEEN start_date AND end_date
    )
)
SELECT row_num, period_name, start_date, end_date, period_year, period_num, quarter_num
FROM NextPeriods
WHERE row_num <= @CalendarPeriod;";
                IList<ICalender> calenders = await ExecuteQueryAsync<ICalender>(sql, parameters);
                return calenders;
            }
            catch
            {
                throw;
            }

        }
    }
}
