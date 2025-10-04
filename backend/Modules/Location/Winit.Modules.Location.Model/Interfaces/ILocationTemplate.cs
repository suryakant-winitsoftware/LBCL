using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ILocationTemplate:IBaseModel
    {
        string TemplateCode { get; set; }
        string TemplateName { get; set; }
        bool IsActive { get; set; }
        string LocationTemplateData { get; set; }
    }
}
