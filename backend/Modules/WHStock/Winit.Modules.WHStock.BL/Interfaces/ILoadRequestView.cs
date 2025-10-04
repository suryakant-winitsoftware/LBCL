using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.WHStock.BL.Interfaces
{
    public interface ILoadRequestView
    {
        public List<WHStockRequestLineItemViewUI> SelectedWHStockRequestLineItemViewUI { get; set; }
        Task PopulateViewModel(string apiParam = null);
        public WHStockRequestItemView WHStockRequest { get; set; }
        public List<IWHStockRequestItemViewUI> DisplayWHStockRequestItemView { get; set; }
        Task ApplyFilter(List<FilterCriteria> filterCriterias, string ActiveTab);
        public WHRequestTempleteModel WHRequestTempletemodel { get; set; }
        public List<ISelectionItem> RouteListForSelection { get; set; }
        Task<bool> CUDWHStock(WHRequestTempleteModel wHRequestTempleteModel);
        Task GetRoutesByOrgUID(string OrgUID);
        public List<Winit.Modules.Route.Model.Interfaces.IRoute> RouteList { get; set; }
        public List<ISelectionItem> DisplayRequestToDDL { get; set; }
        Task GetVehicleDropDown();
        public List<ISelectionItem> DisplayRequestFromDDL { get; set; }
        Task GetRequestFromDropDown();
    }
}
