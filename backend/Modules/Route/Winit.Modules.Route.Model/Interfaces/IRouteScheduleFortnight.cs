using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Route.Model.Interfaces
{
    public interface IRouteScheduleFortnight : Winit.Modules.Base.Model.IBaseModel
    {
        public string CompanyUID { get; set; }
        public string RouteScheduleUID { get; set; }
        public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }
        public int Saturday { get; set; }
        public int Sunday { get; set; }
        public int MondayFN { get; set; }
        public int TuesdayFN { get; set; }
        public int WednesdayFN { get; set; }
        public int ThursdayFN { get; set; }
        public int FridayFN { get; set; }
        public int SaturdayFN { get; set; }
        public int SundayFN { get; set; }

    }
}
