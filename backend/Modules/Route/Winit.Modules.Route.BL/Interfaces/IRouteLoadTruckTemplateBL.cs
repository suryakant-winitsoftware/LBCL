using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.BL.Interfaces
{
    public interface IRouteLoadTruckTemplateBL
    {
        Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplate>> SelectRouteLoadTruckTemplateAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);

        Task<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplateView> SelectRouteLoadTruckTemplateAndLineByUID(string RouteLoadTruckTemplateUID);
        Task<int> CreateRouteLoadTruckTemplateAndLine(Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateViewDTO routeLoadTruckTemplateViewDTO);

        Task<int> UpdateRouteLoadTruckTemplateAndLine(Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateViewDTO routeLoadTruckTemplateViewDTO);

        Task<int> DeleteRouteLoadTruckTemplate(String UID);

        Task<int> DeleteRouteLoadTruckTemplateLine(List<string> RouteLoadTruckTemplateUIDs);

    }
}
