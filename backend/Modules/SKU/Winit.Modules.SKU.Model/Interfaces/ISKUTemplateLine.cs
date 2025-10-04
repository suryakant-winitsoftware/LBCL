using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface ISKUTemplateLine : IBaseModel
    {
         string SKUTemplateUID { get; set; }
         string SKUGroupTypeUID { get; set; }
         string SKUGroupUID { get; set; }
         bool IsExcluded { get; set; }
         bool IsSelected { get; set; }
        string SKUGroupName { get; set; }
        string SKUGroupParentName { get; set; }
        string SKUGroupTypeName { get; set; }
    }
}
