using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Interfaces
{
    public interface ISKUTemplateLineDL
    {
        Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine>> SelectSKUTemplateLineDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine> SelectSKUTemplateLineByUID(string UID);
        Task<int> CreateSKUTemplateLine(Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine sKUTemplateLine);
        Task<int> UpdateSKUTemplateLine(Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine sKUTemplateLine);
        Task<int> DeleteSKUTemplateLine(string UID);
        Task<int> CUDSKUTemplateAndLine(SKUTemplateMaster sKUTemplateMaster);
        Task<int> DeleteSKUTemplateLines(List<string> uIDs);

    }
}
