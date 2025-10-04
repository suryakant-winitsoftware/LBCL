using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.BL.Interfaces
{
    public interface IRouteUserBL
    {
        Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteUser >> SelectAllRouteUserDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<IEnumerable<Winit.Modules.Route.Model.Interfaces.IRouteUser>> SelectRouteUserByUID(List<string> UIDs);

        Task<int> CreateRouteUser(List<Winit.Modules.Route.Model.Classes.RouteUser> routeUserList);
        Task<int> UpdateRouteUser(List<Winit.Modules.Route.Model.Classes.RouteUser> routeUserList);
        Task<int> DeleteRouteUser(List<string> UIDs);
    }
}
