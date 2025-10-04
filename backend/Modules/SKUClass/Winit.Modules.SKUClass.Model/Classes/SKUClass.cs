using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKUClass.Model.Interfaces;

namespace Winit.Modules.SKUClass.Model.Classes
{
    public class SKUClass:BaseModel,ISKUClass
    {
        public string CompanyUID { get; set; }
        public string ClassName { get; set; }
        public string Description { get; set; }
        public string ClassLabel { get; set; }
    }
}
