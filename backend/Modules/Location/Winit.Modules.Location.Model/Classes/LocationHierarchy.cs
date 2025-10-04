using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes
{
    public class LocationHierarchy: BaseModel, ILocationHierarchy
    {
        public string CompanyUID { get; set; }
        public string LocationTypeUID { get; set; }
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
        public string LocationParentUID { get; set; }
        public int ItemLevel { get; set; }
        public string LocationTypeName { get; set; }
        public string LocationTypeCode {  get; set; }
        public string LocationTypeParentUID { get; set; }
        public int LevelNo { get; set; }
    }
}
