using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Holiday.Model.Classes
{
    public class HolidayFactory:IFactory
    {
        public readonly string _ClassName;
        public HolidayFactory(string className ) 
        { 
            _ClassName = className;
        }

        public object? CreateInstance()
        {
           switch (_ClassName)
           {
                case HolidayModule.Holiday:return new Holiday();
                case HolidayModule.HolidayList: return new HolidayList();
                case HolidayModule.HolidayListRole: return new HolidayListRole();
                default: return null;
           }
        }
    }
}
