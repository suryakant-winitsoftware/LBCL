using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Route.Model.Interfaces
{
    public interface IRouteCustomerItemView:IRouteCustomer
    {
        public string Address { get; set; }
        public string StoreCode { get; set; }
        public string StoreLabel { get; set; }
        public string RouteCustomerUID { get; set; }
        public bool IsSelected { get; set; }
    }
}
