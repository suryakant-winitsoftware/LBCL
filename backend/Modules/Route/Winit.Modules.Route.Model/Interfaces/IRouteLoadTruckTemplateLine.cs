using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.Model.Interfaces
{
    public interface IRouteLoadTruckTemplateLine : Winit.Modules.Base.Model.IBaseModel
    {
            public string OrgUID { get; set; }
            public string CompanyUID { get; set; }
            public string RouteLoadTruckTemplateUID { get; set; }
            public string SKUCode { get; set; }
            public string UOM { get; set; }
            public int MondayQty { get; set; }
            public int MondaySuggestedQty { get; set; }
            public int TuesdayQty { get; set; }
            public int TuesdaySuggestedQty { get; set; }
            public int WednesdayQty { get; set; }
            public int WednesdaySuggestedQty { get; set; }
            public int ThursdayQty { get; set; }
            public int ThursdaySuggestedQty { get; set; }
            public int FridayQty { get; set; }
            public int FridaySuggestedQty { get; set; }
            public int SaturdayQty { get; set; }
            public int SaturdaySuggestedQty { get; set; }
            public int SundayQty { get; set; }
            public int SundaySuggestedQty { get; set; }
              public int LineNumber { get; set; }
        public ActionType ActionTypes { get; set; }
            public bool IsSelected { get; set; } 
    }
}
