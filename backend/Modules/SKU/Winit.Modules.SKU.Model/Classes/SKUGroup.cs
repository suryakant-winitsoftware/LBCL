using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes
{
    public class SKUGroup : BaseModel, ISKUGroup
    {
        public string SKUGroupTypeUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ParentUID { get; set; }
        public string ParentName { get; set; }
        public int ItemLevel { get; set; }
        public string SupplierOrgUID { get; set; }
    }
}
