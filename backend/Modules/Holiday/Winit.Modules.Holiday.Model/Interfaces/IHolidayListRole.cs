using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Holiday.Model.Interfaces
{
    public interface IHolidayListRole:IBaseModel
    {
        public string HolidayListUID { get; set; }
        public string UserRoleUID { get; set; }
    }
}
