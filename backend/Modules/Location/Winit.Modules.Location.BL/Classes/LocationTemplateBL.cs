using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.BL.Classes
{
    public class LocationTemplateBL : ILocationTemplateBL
    {
        ILocationTemplateDL _locationTemplateBL;
        public LocationTemplateBL(ILocationTemplateDL locationTemplateBL)
        {
            _locationTemplateBL = locationTemplateBL;
        }
        public async Task<List<ILocationTemplate>> SelectAllLocationTemplates()
        {
            return await _locationTemplateBL.SelectAllLocationTemplates();
        }
        public async Task<int> CreateLocationTemplate(ILocationTemplate locationTemplate)
        {
            return await _locationTemplateBL.CreateLocationTemplate(locationTemplate);
        }
        public async Task<int> UpdateLocationTemplate(ILocationTemplate locationTemplate)
        {
            return await _locationTemplateBL.UpdateLocationTemplate(locationTemplate);
        }
        public async Task<List<ILocationTemplateLine>> SelectAllLocationTemplatesLineBytemplateUID(string templateUID)
        {
            return await _locationTemplateBL.SelectAllLocationTemplatesLineBytemplateUID(templateUID);
        }
        public async Task<int> CreateTemplateLine(List<LocationTemplateLine> templateLines)
        {
            return await _locationTemplateBL.CreateTemplateLine(templateLines);
        }
        public async Task<int> CUDLocationMappingAndLine(LocationTemplateMaster locationTemplateMaster)
        {
            return await _locationTemplateBL.CUDLocationMappingAndLine(locationTemplateMaster);
        }
        public async Task<int> DeleteLocationTemplateLines(List<string> uIDs)
        {
            return await _locationTemplateBL.DeleteLocationTemplateLines(uIDs);
        }
    }
}
