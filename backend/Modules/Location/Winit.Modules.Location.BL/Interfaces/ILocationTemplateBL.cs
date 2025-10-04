using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.BL.Interfaces
{
    public interface ILocationTemplateBL
    {
        Task<List<ILocationTemplate>> SelectAllLocationTemplates();
        Task<int> CreateLocationTemplate(ILocationTemplate locationTemplate);
        Task<int> UpdateLocationTemplate(ILocationTemplate locationTemplate);
        Task<List<ILocationTemplateLine>> SelectAllLocationTemplatesLineBytemplateUID(string templateUID);
        Task<int> CreateTemplateLine(List<LocationTemplateLine> templateLines);
        Task<int> CUDLocationMappingAndLine(LocationTemplateMaster locationTemplateMaster);
        Task<int> DeleteLocationTemplateLines(List<string> uIDs);
    }
}
