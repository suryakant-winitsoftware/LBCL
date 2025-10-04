using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Modules.Currency.Model.Interfaces;

namespace Winit.Modules.SKU.BL.Classes
{
    public class DataPreparationBL : Interfaces.IDataPreparationBL
    {
        protected readonly DL.Interfaces.IDataPreparationDL _dataPreparationDL = null;
        IServiceProvider _serviceProvider = null;

        public DataPreparationBL(DL.Interfaces.IDataPreparationDL dataPreparationDL, IServiceProvider serviceProvider)
        {
            _dataPreparationDL = dataPreparationDL;
            _serviceProvider = serviceProvider;
        }

        public async Task<Winit.Modules.Store.Model.Classes.StoreMaster> PrepareStoreMaster(List<string> storeUIDs)
        {
            var (storeList, storeAdditionalInfoList, storeCreditList, storeAttributesList, addressList, contactList) = await _dataPreparationDL.PrepareStoreMaster(storeUIDs);
            Winit.Modules.Store.Model.Classes.StoreMaster storeMaster = new Winit.Modules.Store.Model.Classes.StoreMaster();
            if (storeList != null && storeList.Count > 0)
            {
               
                foreach (var store in storeList)
                {
                   
                    if (store != null)
                    {
                        storeMaster.Store = store;
                    }
                    if (storeAdditionalInfoList != null)
                    {
                        storeMaster.StoreAdditionalInfo = storeAdditionalInfoList.Where(e => e.StoreUID == store.UID).FirstOrDefault();
                    }
                    if (storeCreditList != null)
                    {
                        storeMaster.storeCredits = storeCreditList.Where(e => e.StoreUID == store.UID).ToList();
                        //storeMaster.OrgList = PrepareStoreOrg(storeMaster.storeCredits);
                        //storeMaster.DistributionChannelList = PrepareStoreDistributionChannel(storeMaster.storeCredits);
                    }
                    if (storeAttributesList != null)
                    {
                        storeMaster.storeAttributes = storeAttributesList.Where(e => e.StoreUID == store.UID).ToList();
                    }
                    if (addressList != null)
                    {
                        storeMaster.Addresses = addressList.Where(e => e.LinkedItemUID == store.UID).ToList();
                    }

                    if (contactList != null)
                    {
                        storeMaster.Contacts = contactList.Where(e => e.LinkedItemUID == store.UID).ToList();
                    }
                }
               

            }
            return storeMaster;
        }

        


        public async   Task<List<IOrgCurrency>> PrepareOrgCurrencyMaster()
        {
            return await _dataPreparationDL.PrepareOrgCurrencyMaster();
        }

        public Task<SKUMaster> PrepareSKUMaster(List<string> orgUIDs, List<string> DistributionChannelUIDs, List<string> skuUIDs, List<string> attributeTypes)
        {
            throw new NotImplementedException();
        }
    }
}
