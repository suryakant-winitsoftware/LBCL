using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.BL.Classes
{
    public abstract class LocationMappingTemplateBaseViewModel : ILocationMappingTemplateBaseViewModel
    {
        public List<ILocationTemplate> Templates { get; set; }=new List<ILocationTemplate>();
        public async Task PopulateViewModel()
        {
            await GetLocationTemplateDetails();
        }

        #region  Abstract classes
        protected abstract  Task GetLocationTemplateDetails();

        #endregion
    }
}
