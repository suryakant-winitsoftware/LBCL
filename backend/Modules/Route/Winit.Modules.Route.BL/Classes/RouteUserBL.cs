using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.BL.Classes
{
    public class RouteUserBL : IRouteUserBL
    {
        protected readonly DL.Interfaces.IRouteUserDL _RouteUserDL = null;
        public RouteUserBL(DL.Interfaces.IRouteUserDL routeUserDL)
        {
            _RouteUserDL = routeUserDL;
        }
        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteUser >> SelectAllRouteUserDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _RouteUserDL.SelectAllRouteUserDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<IEnumerable<Winit.Modules.Route.Model.Interfaces.IRouteUser>> SelectRouteUserByUID(List<string> UIDs)
        {
            return await _RouteUserDL.SelectRouteUserByUID(UIDs);
        }
        public async Task<int> CreateRouteUser(List<Winit.Modules.Route.Model.Classes.RouteUser> routeUserList)
        {
            return await _RouteUserDL.CreateRouteUser(routeUserList);
        }

        public async Task<int> UpdateRouteUser(List<Winit.Modules.Route.Model.Classes.RouteUser> routeUserList)
        {
            return await _RouteUserDL.UpdateRouteUser(routeUserList);
        }

        public async Task<int> DeleteRouteUser(List<string> UIDs)
        {
            return await _RouteUserDL.DeleteRouteUser(UIDs);
        }
    }
}
