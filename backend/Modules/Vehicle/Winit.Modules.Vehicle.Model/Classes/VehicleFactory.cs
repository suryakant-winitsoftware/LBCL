using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Vehicle.Model.Classes
{
    public class VehicleFactory:IFactory
    {
        public readonly string _classname;
        public VehicleFactory(string classname)
        {
            _classname = classname;
        }
        public object? CreateInstance()
        {
            switch (_classname)
            {
                case VehicleModule.Vehicle: return new Vehicle();
                default: return null;
            }
        }
    }
}
