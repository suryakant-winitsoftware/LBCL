using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Route.BL.Interfaces
{
    public interface IAddEditRouteLoadViewModel
    {
        public List<string> AllUIDs { get; set; }
        public List<string> ExistingUIDs { get; set; }
        Task PopulateViewModel(string apiParam = null);
        public RouteLoadTruckTemplateViewDTO DisplayRouteLoadTruckTemplateViewDTO { get; set; }
        public RouteLoadTruckTemplateViewDTO FilterRouteLoadTruckTemplateViewDTO { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
        public string SelectedRouteUID { get; set; }
        public string SelectedRouteName { get; set; }

        public string RouteLoadTruckTemplateUID { get; set; }
        public string TemplateName { get; set; }
        public string TemplateDescription { get; set; }
        public List<ISelectionItem> RouteListForSelection { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SkuList { get; set; }
        Task<bool> CreateUpdateIRouteLoadTruckTemplateDTO();
        Task OnselectRoue(DropDownEvent dropDown);
        Task GetRoutes();
        Task GetSKUMasterData();
        Task ApplySearch(string searchString);
        void CreateRouteLoadTruckTemplate();
        void CreateInstancesOfTemplateDTO();
        void MatchingNewExistingUIDs();
        void CreateRouteLoadTruckTemplateLine(ISelectionItem selectionItem);
        Task<bool> DeleteSelectedTemplates();

        public int LineNumber { get; set; }

        public string SelectedRoute { get; set; }
    }
}
