using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Interfaces
{
    public interface IDataPreparationDL
    {
        Task<(List<Model.Interfaces.ISKU>, List<Model.Interfaces.ISKUConfig>, List<Model.Interfaces.ISKUUOM>, List<Model.Interfaces.ISKUAttributes>, List<Model.Interfaces.ITaxSkuMap>)>
            PrepareSKUMaster(List<String> orgUIDs, List<string> DistributionChannelUIDs, List<string> skuUIDs, List<string> attributeTypes);


        Task<(List<Winit.Modules.Store.Model.Interfaces.IStore>, List<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>,
            List<Winit.Modules.Store.Model.Interfaces.IStoreCredit>, List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>,
            List<Winit.Modules.Address.Model.Interfaces.IAddress>, List<Winit.Modules.Contact.Model.Interfaces.IContact>)> PrepareStoreMaster(List<string> storeUIDs);


        Task<List<IOrgCurrency>> PrepareOrgCurrencyMaster();

    }
}
