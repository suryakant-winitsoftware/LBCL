using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Classes
{
    public class LocationFactory:IFactory
    {
        private readonly string _className;

        public LocationFactory(string className)
        {
            _className = className;
        }

        public object? CreateInstance()
        {
            switch (_className)
            {
                case LocationModule.Location:
                    return new Location();
                case LocationModule.LocationMapping:
                    return new LocationMapping();
                case LocationModule.LocationType:
                    return new LocationType();
                default:
                    return null;
            }
        }
    }
}
