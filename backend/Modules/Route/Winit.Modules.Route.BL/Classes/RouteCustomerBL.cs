using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.BL.Classes
{
    public class RouteCustomerBL : RouteBaseBL, Winit.Modules.Route.BL.Interfaces.IRouteCustomerBL
    {
        protected readonly DL.Interfaces.IRouteCustomerDL _routecustomerDL = null;
        public RouteCustomerBL(DL.Interfaces.IRouteCustomerDL routecustomerDL)
        {
            _routecustomerDL = routecustomerDL;
        }
        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteCustomer>> SelectRouteCustomerAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _routecustomerDL.SelectRouteCustomerAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<Winit.Modules.Route.Model.Interfaces.IRouteCustomer> SelectRouteCustomerDetailByUID(string UID)
        {
            return await _routecustomerDL.SelectRouteCustomerDetailByUID(UID);
        }
        public async Task<int> CreateRouteCustomerDetails(Winit.Modules.Route.Model.Interfaces.IRouteCustomer routecustomerDetails)
        {
            return await _routecustomerDL.CreateRouteCustomerDetails(routecustomerDetails);
        }

        public async Task<int> UpdateRouteCustomerDetails(Winit.Modules.Route.Model.Interfaces.IRouteCustomer routecustomerDetails)
        {
            return await _routecustomerDL.UpdateRouteCustomerDetails(routecustomerDetails);
        }

        public async Task<int> DeleteRouteCustomerDetails(List<String> UIDs)
        {
            return await _routecustomerDL.DeleteRouteCustomerDetails(UIDs);
        }
        public async Task<IEnumerable<SelectionItem>> GetRouteByStoreUID(string storeUID)
        {
            return await _routecustomerDL.GetRouteByStoreUID(storeUID);
        }
    }
}
