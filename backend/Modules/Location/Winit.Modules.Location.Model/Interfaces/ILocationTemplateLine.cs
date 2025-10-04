using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ILocationTemplateLine : IBaseModel
    {
        string LocationTemplateUID { get; set; }
        string LocationTypeUID { get; set; }
        string LocationUID { get; set; }
        bool IsExcluded { get; set; }
        string Type { get; set; }
        string Value { get; set; }
        bool IsSelected { get; set; }
    }
}
