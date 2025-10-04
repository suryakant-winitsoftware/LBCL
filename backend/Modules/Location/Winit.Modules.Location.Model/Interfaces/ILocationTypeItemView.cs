using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ILocationTypeItemView:ILocationType
    {
        public bool IsCreatePopUpOpen { get; set; }
        public bool IsUpdatePopUpOpen { get; set; }
        public bool IsDeletePopUpOpen { get; set; }
        public bool IsOpen { get; set; }
        public string ParentName { get; set; }
        public List<ILocationTypeItemView> ChildGrids { get; set; }
    }
}
