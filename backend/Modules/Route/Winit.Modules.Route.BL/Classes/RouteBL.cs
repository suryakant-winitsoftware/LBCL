using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Classes;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.BL.Classes
{
    public class RouteBL : RouteBaseBL, Winit.Modules.Route.BL.Interfaces.IRouteBL
    {
        protected readonly DL.Interfaces.IRouteDL _routeDL = null;
        IServiceProvider _serviceProvider = null;
        public RouteBL(DL.Interfaces.IRouteDL routeDL, IServiceProvider serviceProvider)
        {
            _routeDL = routeDL;
            _serviceProvider = serviceProvider;
        }
        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRoute>> SelectRouteAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string OrgUID)
        {
            return await _routeDL.SelectRouteAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, OrgUID);
        }

        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog>> SelectRouteChangeLogAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _routeDL.SelectRouteChangeLogAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<Winit.Modules.Route.Model.Interfaces.IRoute> SelectRouteDetailByUID(string UID)
        {
            return await _routeDL.SelectRouteDetailByUID(UID);
        }
        public async Task<int> CreateRouteDetails(Winit.Modules.Route.Model.Interfaces.IRoute routeDetails)
        {
            return await _routeDL.CreateRouteDetails(routeDetails);
        }

        public async Task<int> UpdateRouteDetails(Winit.Modules.Route.Model.Interfaces.IRoute routeDetails)
        {
            return await _routeDL.UpdateRouteDetails(routeDetails);
        }

        public async Task<int> DeleteRouteDetail(String UID)
        {
            return await _routeDL.DeleteRouteDetail(UID);
        }
        public async Task<int> CreateRouteMaster(Winit.Modules.Route.Model.Classes.RouteMaster routeMasterDetails)
        {
            // Debug logging for business layer
            Console.WriteLine("[DEBUG BL] ========== CreateRouteMaster BL Called ==========");
            Console.WriteLine($"[DEBUG BL] RouteCustomersList count: {routeMasterDetails.RouteCustomersList?.Count ?? 0}");
            Console.WriteLine($"[DEBUG BL] RouteScheduleCustomerMappings count: {routeMasterDetails.RouteScheduleCustomerMappings?.Count ?? 0}");
            
            if (routeMasterDetails.RouteScheduleCustomerMappings != null)
            {
                foreach (var mapping in routeMasterDetails.RouteScheduleCustomerMappings)
                {
                    Console.WriteLine($"[DEBUG BL] Mapping - RouteScheduleUID: {mapping.RouteScheduleUID}, ConfigUID: {mapping.RouteScheduleConfigUID}, CustomerUID: {mapping.CustomerUID}");
                }
            }
            
            return await _routeDL.CreateRouteMaster(routeMasterDetails);
        }
        public async Task<int> UpdateRouteMaster(Winit.Modules.Route.Model.Classes.RouteMaster routeMasterDetails)
        {
            return await _routeDL.UpdateRouteMaster(routeMasterDetails);
        }

        public async Task<Winit.Modules.Route.Model.Interfaces.IRouteMasterView> SelectRouteMasterViewByUID(string UID)
        {

            var (routeList, routeSheduleList, routeScheduleConfigList, routeScheduleCustomerMappingList, routeCustomerList, routeUserList) = await _routeDL.SelectRouteMasterViewByUID(UID);

            Winit.Modules.Route.Model.Interfaces.IRouteMasterView routeMasterView = _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRouteMasterView>();
            if (routeList != null && routeList.Count > 0)
            {
                routeMasterView.Route = routeList.FirstOrDefault();
            }
            if (routeSheduleList != null && routeSheduleList.Count > 0)
            {
                routeMasterView.RouteSchedule = routeSheduleList.FirstOrDefault();
            }
            if (routeScheduleConfigList != null && routeScheduleConfigList.Count > 0)
            {
                routeMasterView.RouteScheduleConfigs = routeScheduleConfigList;
            }
            if (routeScheduleCustomerMappingList != null && routeScheduleCustomerMappingList.Count > 0)
            {
                routeMasterView.RouteScheduleCustomerMappings = routeScheduleCustomerMappingList;
            }

            if (routeCustomerList != null && routeCustomerList.Count > 0)
            {
                routeMasterView.RouteCustomersList = routeCustomerList;
            }
            if (routeUserList != null && routeUserList.Count > 0)
            {
                routeMasterView.RouteUserList = routeUserList;
            }
            return routeMasterView;
        }

        //dropdowns
        public async Task<List<ISelectionItem>> GetVehicleDDL(string orgUID)
        {
            return await _routeDL.GetVehicleDDL(orgUID);
        }
        public async Task<List<ISelectionItem>> GetWareHouseDDL(string OrgTypeUID, string ParentUID)
        {
            return await _routeDL.GetWareHouseDDL(OrgTypeUID, ParentUID);
        }
        public async Task<List<ISelectionItem>> GetUserDDL(string OrgUID)
        {
            return await _routeDL.GetUserDDL(OrgUID);
        }
        public async Task<List<Model.Interfaces.IRoute>> GetRoutesByStoreUID(string orgUID, string storeUID)
        {
            return await _routeDL.GetRoutesByStoreUID(orgUID, storeUID);
        }

        public async Task<List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>> GetAllRouteScheduleConfigs()
        {
            return await _routeDL.GetAllRouteScheduleConfigs();
        }
    }
}
