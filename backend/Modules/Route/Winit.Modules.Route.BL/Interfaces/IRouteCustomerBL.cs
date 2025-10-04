using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.BL.Interfaces
{
    public interface IRouteCustomerBL
    {
        Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteCustomer>> SelectRouteCustomerAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);

        Task<Winit.Modules.Route.Model.Interfaces.IRouteCustomer> SelectRouteCustomerDetailByUID(string UID);
        Task<int> CreateRouteCustomerDetails(Winit.Modules.Route.Model.Interfaces.IRouteCustomer routecustomerDetails);

        Task<int> UpdateRouteCustomerDetails(Winit.Modules.Route.Model.Interfaces.IRouteCustomer routecustomerDetails);
        Task<int> DeleteRouteCustomerDetails(List<String> UIDs);
        Task<IEnumerable<SelectionItem>> GetRouteByStoreUID(string storeUID);
    } 
}
