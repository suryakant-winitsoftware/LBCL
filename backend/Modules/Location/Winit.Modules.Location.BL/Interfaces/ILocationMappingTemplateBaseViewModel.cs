using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.BL.Interfaces
{
    public interface ILocationMappingTemplateBaseViewModel
    {
        List<ILocationTemplate> Templates { get; set; }
        Task PopulateViewModel();
    }
}
