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

namespace Winit.Modules.SKU.BL.Classes
{
    public class SkuSequenceBL : ISkuSequenceBL
    {
        protected readonly DL.Interfaces.ISkuSequenceDL _skuSequenceDL = null;
        IServiceProvider _serviceProvider = null;
        public SkuSequenceBL(DL.Interfaces.ISkuSequenceDL skuSequenceDL, IServiceProvider serviceProvider)
        {
            _skuSequenceDL = skuSequenceDL;
            _serviceProvider = serviceProvider;
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISkuSequence>> SelectAllSkuSequenceDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string SeqType)
        {
            return await _skuSequenceDL.SelectAllSkuSequenceDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, SeqType);
        }

        public async Task<int> CreateGeneralSKUSequenceForSKU(string BUOrgUID, string SKUUID)
        {
            return await _skuSequenceDL.CreateGeneralSKUSequenceForSKU(BUOrgUID, SKUUID);

        }

        public async  Task<int> CUDSkuSequence(List<Winit.Modules.SKU.Model.Classes.SkuSequence> skuSequencesList)
        {
            return await _skuSequenceDL.CUDSkuSequence(skuSequencesList);
        }

    }
}
