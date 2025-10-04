using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ILocation:IBaseModel
    {
       
        public string CompanyUID { get; set; }
        public string LocationTypeUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ParentUID { get; set; }
        public int ItemLevel { get; set; }
        public bool HasChild { get; set; }
        public string LocationTypeName { get; set; }
        public string LocationTypeCode { get; set; }
    }
}
