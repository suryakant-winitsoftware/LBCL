using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.FileSys.BL.Classes
{
    public class FileSysTemplateBL:IFileSysTemplateBL
    {
        protected readonly DL.Interfaces.IFileSysTemplateDL _FileSysTemplateRepository = null;
        public FileSysTemplateBL(DL.Interfaces.IFileSysTemplateDL FileSysTemplateRepository)
        {
            _FileSysTemplateRepository = FileSysTemplateRepository;
        }
        public async Task<PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate>> SelectAllFileSysTemplateDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _FileSysTemplateRepository.SelectAllFileSysTemplateDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate> GetFileSysTemplateByUID(string UID)
        {
            return await _FileSysTemplateRepository.GetFileSysTemplateByUID(UID);
        }
        public async Task<int> CreateFileSysTemplate(Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate createFileSysTemplate)
        {
            return await _FileSysTemplateRepository.CreateFileSysTemplate(createFileSysTemplate);
        }
        public async Task<int> UpdateFileSysTemplate(Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate updateFileSysTemplate)
        {
            return await _FileSysTemplateRepository.UpdateFileSysTemplate(updateFileSysTemplate);
        }
        public async Task<int> DeleteFileSysTemplate(string Code)
        {
            return await _FileSysTemplateRepository.DeleteFileSysTemplate(Code);
        }
    }
}
