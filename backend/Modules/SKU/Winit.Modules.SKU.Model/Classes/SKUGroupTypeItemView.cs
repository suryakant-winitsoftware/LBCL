using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes
{
    public class SKUGroupTypeItemView : SKUGroupType,ISKUGroupTypeItemView
    {
        public bool IsCreatePopUpOpen { get; set; }
        public bool IsUpdatePopUpOpen { get; set; }
        public bool IsDeletePopUpOpen { get; set; }
        public bool IsOpen { get; set; }
        public string ParentName { get; set; }
        public List<ISKUGroupTypeItemView> ChildGrids { get; set; }
    }
}
