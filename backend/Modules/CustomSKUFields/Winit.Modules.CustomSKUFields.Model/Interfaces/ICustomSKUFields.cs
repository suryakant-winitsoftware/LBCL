using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CustomSKUField.Model.Classes;

namespace Winit.Modules.CustomSKUField.Model.Interfaces
{
    public interface ICustomSKUFields : IBaseModel
    {
        public string SKUUID { get; set; }
        public string CustomField { get; set; }
    }
    public interface ICustomSKUField : IBaseModel
    {
        public string? SKUUID { get; set; }
        public List<CustomField> CustomField { get; set; }
    }

}
