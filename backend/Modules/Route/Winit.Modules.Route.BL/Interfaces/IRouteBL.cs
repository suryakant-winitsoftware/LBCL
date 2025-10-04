using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.BL.Interfaces
{
    public interface IRouteBL
    {
        Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRoute>> SelectRouteAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired,string OrgUID);
        Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog>> SelectRouteChangeLogAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);


        Task<Winit.Modules.Route.Model.Interfaces.IRoute> SelectRouteDetailByUID(string UID);
        Task<int> CreateRouteDetails(Winit.Modules.Route.Model.Interfaces.IRoute routeDetails);

        Task<int> UpdateRouteDetails(Winit.Modules.Route.Model.Interfaces.IRoute routeDetails);
        Task<int> DeleteRouteDetail(String UID);

        Task<int> CreateRouteMaster(Winit.Modules.Route.Model.Classes.RouteMaster routeMasterDetails);
        Task<int> UpdateRouteMaster(Winit.Modules.Route.Model.Classes.RouteMaster routeMasterDetails);
        Task<Winit.Modules.Route.Model.Interfaces.IRouteMasterView> SelectRouteMasterViewByUID(string UID);
        //dropdowns
        Task<List<ISelectionItem>> GetVehicleDDL(string orgUID);
        Task<List<ISelectionItem>> GetWareHouseDDL(string OrgTypeUID, string ParentUID);
        Task<List<ISelectionItem>> GetUserDDL(string OrgUID);
        Task<List<Model.Interfaces.IRoute>> GetRoutesByStoreUID(string orgUID, string storeUID);
        Task<List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>> GetAllRouteScheduleConfigs();

    }
}
