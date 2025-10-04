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
    public class SKUTemplateLineBL : ISKUTemplateLineBL
    {
        protected readonly DL.Interfaces.ISKUTemplateLineDL _skuTemplateLineDL;
        public SKUTemplateLineBL(DL.Interfaces.ISKUTemplateLineDL skuTemplateLineDL)
        {
            _skuTemplateLineDL = skuTemplateLineDL;
        }

        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine>> SelectSKUTemplateLineDetails(List<SortCriteria> sortCriterias, int pageNumber,
                int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _skuTemplateLineDL.SelectSKUTemplateLineDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);

        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine> SelectSKUTemplateLineByUID(string UID)
        {
            return await _skuTemplateLineDL.SelectSKUTemplateLineByUID(UID);
        }
        public async Task<int> CreateSKUTemplateLine(Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine sKUTemplateLine)
        {
            return await _skuTemplateLineDL.CreateSKUTemplateLine(sKUTemplateLine);
        }
        public async Task<int> UpdateSKUTemplateLine(Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine sKUTemplateLine)
        {
            return await _skuTemplateLineDL.UpdateSKUTemplateLine(sKUTemplateLine);
        }
        public async Task<int> DeleteSKUTemplateLine(string UID)
        {
            return await _skuTemplateLineDL.DeleteSKUTemplateLine(UID);
        }
        public async Task<int> CUDSKUTemplateAndLine(SKUTemplateMaster sKUTemplateMaster)
        {
            return await _skuTemplateLineDL.CUDSKUTemplateAndLine(sKUTemplateMaster);
        }
        public async Task<int> DeleteSKUTemplateLines(List<string> uIDs)
        {
            return await _skuTemplateLineDL.DeleteSKUTemplateLines(uIDs);
        }


    }
}
