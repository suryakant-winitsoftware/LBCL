using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IWeekDays : IBaseModel
    {
        public string StoreUID { get; set; }
        public bool WeekOffSun { get; set; }
        public bool WeekOffMon { get; set; }
        public bool WeekOffTue { get; set; }
        public bool WeekOffWed { get; set; }
        public bool WeekOffThu { get; set; }
        public bool WeekOffFri { get; set; }
        public bool WeekOffSat { get; set; }
    }
}
