using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CustomSKUField.Model.Interfaces;

namespace Winit.Modules.CustomSKUField.Model.Classes
{
    public class CustomSKUFields : BaseModel, ICustomSKUFields
    {
        public string? SKUUID { get; set; }
        
        public string CustomField { get; set; }
    }
    public class CustomField
    {
        public int SNo { get; set; }
        public string UID { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
    public class CustomSKUField : BaseModel,ICustomSKUField
    {
        public string? SKUUID { get; set; }
        public List<CustomField> CustomField { get; set; }
    }


}
