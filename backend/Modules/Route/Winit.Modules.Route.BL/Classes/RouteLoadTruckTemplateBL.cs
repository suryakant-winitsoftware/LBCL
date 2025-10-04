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
    public class RouteLoadTruckTemplateBL : RouteBaseBL, Winit.Modules.Route.BL.Interfaces.IRouteLoadTruckTemplateBL
    {
        protected readonly DL.Interfaces.IRouteLoadTruckTemplateDL _routeLoadTruckTemplateDL = null;
        IServiceProvider _serviceProvider = null;
        public RouteLoadTruckTemplateBL(DL.Interfaces.IRouteLoadTruckTemplateDL routeLoadTruckTemplateDL, IServiceProvider serviceProvider)
        {
            _routeLoadTruckTemplateDL = routeLoadTruckTemplateDL;
            _serviceProvider = serviceProvider;
        }
        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplate>> SelectRouteLoadTruckTemplateAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _routeLoadTruckTemplateDL.SelectRouteLoadTruckTemplateAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplateView> SelectRouteLoadTruckTemplateAndLineByUID(string RouteLoadTruckTemplateUID)
        {
            var(routeLoadTruckTemplateList, routeLoadTruckTemplateLineList) = await _routeLoadTruckTemplateDL.SelectRouteLoadTruckTemplateAndLineByUID(RouteLoadTruckTemplateUID);

                Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplateView routeLoadTruckTemplateView  = _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplateView>();
                if(routeLoadTruckTemplateList !=null && routeLoadTruckTemplateList.Count > 0)
                {
                    routeLoadTruckTemplateView.RouteLoadTruckTemplate = routeLoadTruckTemplateList.FirstOrDefault();
                }
                if(routeLoadTruckTemplateLineList !=null && routeLoadTruckTemplateList.Count > 0)
             
                routeLoadTruckTemplateView.RouteLoadTruckTemplateLineList = routeLoadTruckTemplateLineList;
               return routeLoadTruckTemplateView;

        }
        public async Task<int> CreateRouteLoadTruckTemplateAndLine(Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateViewDTO routeLoadTruckTemplateViewDTO)
        {
            return await _routeLoadTruckTemplateDL.CreateRouteLoadTruckTemplateAndLine(routeLoadTruckTemplateViewDTO);

        }
        public async Task<int> UpdateRouteLoadTruckTemplateAndLine(Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateViewDTO routeLoadTruckTemplateViewDTO)
        {
            return await _routeLoadTruckTemplateDL.UpdateRouteLoadTruckTemplateAndLine(routeLoadTruckTemplateViewDTO);
        }


        public async Task<int> DeleteRouteLoadTruckTemplate(String UID)
        {
            return await _routeLoadTruckTemplateDL.DeleteRouteLoadTruckTemplate(UID);
        }
        public async Task<int> DeleteRouteLoadTruckTemplateLine(List<string> RouteLoadTruckTemplateUIDs)
        {
            return await _routeLoadTruckTemplateDL.DeleteRouteLoadTruckTemplateLine(RouteLoadTruckTemplateUIDs);
        }



    }
}
