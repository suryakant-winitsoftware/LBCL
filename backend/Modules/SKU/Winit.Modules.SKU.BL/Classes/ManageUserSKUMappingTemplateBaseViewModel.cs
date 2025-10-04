using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.BL.Classes
{
    public abstract class ManageUserSKUMappingTemplateBaseViewModel: IManageUserSKUMappingTemplateBaseViewModel
    {
        public List<SKUTemplate> Templates { get; set; }=new List<SKUTemplate>();
        public async Task PopulateViewModel()
        {
           await GetLocationTemplateDetails();
        }

        protected abstract  Task GetLocationTemplateDetails();
    }
}
