using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Holiday.Model.Interfaces;

namespace Winit.Modules.Holiday.Model.Classes
{
    public class Holiday : BaseModel, IHoliday
    {
        public string HolidayListUID { get; set; }
        public DateTime HolidayDate { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public bool IsOptional { get; set; }
        public int Year { get; set; }
    }
}
