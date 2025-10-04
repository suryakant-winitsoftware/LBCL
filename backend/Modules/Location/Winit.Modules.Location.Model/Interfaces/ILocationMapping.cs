using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ILocationMapping:IBaseModel
    {
        
        public string LinkedItemUID { get; set; }
        public string LinkedItemType { get; set; }
        public string LocationTypeUID { get; set; }
        public string LocationUID { get; set; }
    }
}
