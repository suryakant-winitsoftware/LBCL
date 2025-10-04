using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.FileSys.BL.Interfaces
{
    public interface IFileSysTemplateBL
    {
        Task<PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate>> SelectAllFileSysTemplateDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate> GetFileSysTemplateByUID(string UID);
        Task<int> CreateFileSysTemplate(Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate createFileSysTemplate);
        Task<int> UpdateFileSysTemplate(Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate updateFileSysTemplate);
        Task<int> DeleteFileSysTemplate(string UID);
    }
}
