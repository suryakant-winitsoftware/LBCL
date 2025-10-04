using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface IManageUserSKUMappingTemplateBaseViewModel
    {
        List<SKUTemplate> Templates { get; set; }
        Task PopulateViewModel();
    }
}
