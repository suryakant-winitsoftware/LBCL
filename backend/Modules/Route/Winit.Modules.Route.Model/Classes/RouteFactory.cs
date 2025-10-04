using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Route.Model.Interfaces;

namespace Winit.Modules.Route.Model.Classes
{
    public class RouteFactory : IFactory
    {
        private readonly string _className; 

        public RouteFactory(string className)
        {
            _className = className;
        }

        public object? CreateInstance()
        {
            switch (_className)
            {
                case RouteModule.Route:
                    return new Route();
                case RouteModule.RouteCustomer:
                    return new RouteCustomer();
                case RouteModule.RouteUser:
                    return new RouteUser();
                default:
                    return null; 
            }
        }
    }

}
