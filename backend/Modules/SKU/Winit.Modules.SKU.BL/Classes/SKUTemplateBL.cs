using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Winit.UIModels.Web.SKU;

namespace Winit.Modules.SKU.BL.Classes
{
    public class SKUTemplateBL : ISKUTemplateBL
    {
        protected readonly DL.Interfaces.ISKUTemplateDL _skuTemplateDL;
        public SKUTemplateBL(DL.Interfaces.ISKUTemplateDL skuTemplateDL)
        {
            _skuTemplateDL=skuTemplateDL;
        }

      public async  Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate>> SelectAllSKUTemplateDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _skuTemplateDL.SelectAllSKUTemplateDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
       public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate> SelectSKUTemplateByUID(string UID)
        {
            return await _skuTemplateDL.SelectSKUTemplateByUID(UID);
        }
       public async Task<int> CreateSKUTemplate(Winit.Modules.SKU.Model.Interfaces.ISKUTemplate sKUTemplate)
        {
            return await _skuTemplateDL.CreateSKUTemplate(sKUTemplate);
        }
       public async Task<int> UpdateSKUTemplate(Winit.Modules.SKU.Model.Interfaces.ISKUTemplate sKUTemplate)
        {
            return await _skuTemplateDL.UpdateSKUTemplate(sKUTemplate); 
        }
       public async Task<int> DeleteSKUTemplate(string UID)
        {
            return await _skuTemplateDL.DeleteSKUTemplate(UID);
        }


    }
}
