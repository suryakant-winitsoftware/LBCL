using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Holiday.Model.Interfaces;

namespace Winit.Modules.Holiday.Model.Classes
{
    public class HolidayList: BaseModel, IHolidayList
    {
        public string CompanyUID { get; set; }
        public string OrgUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LocationUID { get; set; }
        public bool IsActive { get; set; }
        public int Year { get; set; }
    }
}
