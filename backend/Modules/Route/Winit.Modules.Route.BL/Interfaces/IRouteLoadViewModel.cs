using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Route.BL.Interfaces
{
    public interface IRouteLoadViewModel
    {
      
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SkuUOMList { get; set; }
        public List<ISelectionItem> RouteListForSelection { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SkuList { get; set; }
        public string SelectedRouteUID { get; set; }
        Task OnselectRoue(DropDownEvent dropDown);
       
        public string TemplateName { get; set; }
        public string TemplateDescription { get; set; }
        Task PopulateViewModel(string apiParam = null);
        public List<Modules.Route.Model.Classes.Route> RouteList { get; set; }
        public List<IRouteLoadTruckTemplateUI> DisplayRouteLoadTruckTemplateView { get; set; }
        public List<IRouteLoadTruckTemplateUI> FilterRouteLoadTruckTemplateView { get; set; }
        Task<bool> DeleteRouteLoadTruckTemplate(string selectedUID);
        public RouteLoadTruckTemplateViewDTO DisplayRouteLoadTruckTemplateViewDTO { get; set; }
        public RouteLoadTruckTemplateViewDTO FilterRouteLoadTruckTemplateViewDTO { get; set; }

        Task ApplyFilter(List<FilterCriteria> filterCriterias);
        Task GetRouteLoadTruckLineByUID(string uid);

       

        Task<List<Winit.Modules.SKU.Model.Interfaces.ISKU>> FindCompleteSKU(IEnumerable<SKUMasterData> sKUMasters);

        Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>> FindCompleteSKUAttributes(IEnumerable<SKUMasterData> sKUMasters);

        Task<bool> CreateUpdateIRouteLoadTruckTemplateLineDataFromAPIAsync(RouteLoadTruckTemplateViewDTO iRuteLodTrukTmplateLine, bool IsCreate);


    }
}
