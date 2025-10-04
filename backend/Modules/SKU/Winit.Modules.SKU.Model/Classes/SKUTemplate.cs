using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes
{
    public class SKUTemplate : BaseModel, ISKUTemplate
    {
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public bool IsActive { get; set; }
        public string SKUTemplateData { get; set; }
    }
}
