using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface IDataPreparationBL
    {
        Task<SKUMaster> PrepareSKUMaster(List<String> orgUIDs,List<string> DistributionChannelUIDs,List<string>skuUIDs,List<string>attributeTypes);

        Task<Winit.Modules.Store.Model.Classes.StoreMaster> PrepareStoreMaster(List<string> storeUIDs);

        Task<List<IOrgCurrency>> PrepareOrgCurrencyMaster();





    }
}
