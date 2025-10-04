using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ILocationHierarchy : IBaseModel
    {
        public string CompanyUID { get; set; }
        public string LocationTypeUID { get; set; }
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
        public string LocationParentUID { get; set; }
        public int ItemLevel { get; set; }
        public string LocationTypeName { get; set; }
        public string LocationTypeCode { get; set; }
        public string LocationTypeParentUID { get; set; }
        public int LevelNo { get; set; }
    }
}
