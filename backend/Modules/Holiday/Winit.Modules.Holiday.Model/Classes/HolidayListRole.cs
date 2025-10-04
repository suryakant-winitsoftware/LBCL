using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Holiday.Model.Interfaces;

namespace Winit.Modules.Holiday.Model.Classes
{
    public class HolidayListRole:BaseModel, IHolidayListRole
    {
        public string HolidayListUID { get; set; }
        public string UserRoleUID { get; set; }
    }
}
