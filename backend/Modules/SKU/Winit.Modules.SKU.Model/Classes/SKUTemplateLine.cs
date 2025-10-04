using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes
{
    public class SKUTemplateLine : BaseModel, ISKUTemplateLine
    {
        public string SKUTemplateUID { get; set; }
        public string SKUGroupTypeUID { get; set; }
        public string SKUGroupTypeName { get; set; }
        public string SKUGroupUID { get; set; }
        public string SKUGroupName { get; set; }
        public string SKUGroupParentName { get; set; }
        public bool IsExcluded { get; set; }
        public bool IsSelected { get; set; }
    }
}
