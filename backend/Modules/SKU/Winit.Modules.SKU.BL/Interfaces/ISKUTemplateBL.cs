using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.BL.Classes;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface ISKUTemplateBL
    {
        Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate>> SelectAllSKUTemplateDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate> SelectSKUTemplateByUID(string UID);
        Task<int> CreateSKUTemplate(Winit.Modules.SKU.Model.Interfaces.ISKUTemplate sKUTemplate);
        Task<int> UpdateSKUTemplate(Winit.Modules.SKU.Model.Interfaces.ISKUTemplate sKUTemplate);
        Task<int> DeleteSKUTemplate(string UID);

    }
}
