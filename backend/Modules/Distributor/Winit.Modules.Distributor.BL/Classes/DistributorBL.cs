using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Address.BL.Interfaces;
using Winit.Modules.Address.Model.Classes;
using Winit.Modules.Contact.BL.Interfaces;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Currency.DL.Interfaces;
using Winit.Modules.Distributor.DL.Interfaces;
using Winit.Modules.Distributor.Model.Classes;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.StoreDocument.BL.Interfaces;
using Winit.Modules.StoreDocument.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Distributor.BL.Classes
{
    public class DistributorBL : Interfaces.IDistributorBL
    {
        IDistributorDL _distributorDL;
        IStoreDL _storeDL;
        IOrgBL _orgBL;
        IStoreAdditionalInfoDL _storeAdditionalInfoDL;
        IStoreCreditDL _storeCreditDL; IContactBL _contactBL; IAddressBL _addressBL; IStoreDocumentBL _storeDocumentBL;
        ICurrencyDL _currencyDL;
        public DistributorBL(IDistributorDL distributorDL, IOrgBL orgBL
            , IStoreDL storeDL, IStoreAdditionalInfoDL storeAdditionalInfoDL,
            IStoreCreditDL storeCreditDL, IContactBL contactBL, 
            IAddressBL addressBL, IStoreDocumentBL storeDocumentBL,ICurrencyDL currencyDL)
        {
            _distributorDL = distributorDL;
            _orgBL = orgBL;
            _storeDL = storeDL;
            _addressBL = addressBL;
            _storeDocumentBL = storeDocumentBL;
            _storeCreditDL = storeCreditDL;
            _contactBL = contactBL;
            _addressBL = addressBL;
            _storeAdditionalInfoDL= storeAdditionalInfoDL;
            _currencyDL= currencyDL;
        }
        public async Task<int> CreateDistributor(Winit.Modules.Distributor.Model.Classes.DistributorMasterView distributorMasterView)
        {
            var retVal = 0; //= await _distributorBL.CreateDistributor(distributorMasterView);
            int plCount = 0;
            int count = 0;
            //ORG
            var org = await _orgBL.GetOrgByUID(distributorMasterView.Org.UID);
            if (org == null)
            {
                count += await _orgBL.CreateOrg(distributorMasterView.Org);
            }
            else
            {
                count += await _orgBL.UpdateOrg(distributorMasterView.Org);
            }
            //Store
            var store = await _storeDL.SelectStoreByUID(distributorMasterView.Store.UID);
            if (store == null)
                count += await _storeDL.CreateStore(distributorMasterView.Store);
            else
                count += await _storeDL.UpdateStore(distributorMasterView.Store);
            //Store Additional Info
            var StoreAddinfo = await _storeAdditionalInfoDL.SelectStoreAdditionalInfoByUID(distributorMasterView.StoreAdditionalInfo.UID);
            if (StoreAddinfo == null)
                count += await _storeAdditionalInfoDL.CreateStoreAdditionalInfo(distributorMasterView.StoreAdditionalInfo);
            else
                count += await _storeAdditionalInfoDL.UpdateStoreAdditionalInfo(distributorMasterView.StoreAdditionalInfo);

            //Store Credit
            var storeCredit = await _storeCreditDL.SelectStoreCreditByUID(distributorMasterView.StoreCredit.UID);
            if (storeCredit == null)
                count += await _storeCreditDL.CreateStoreCredit(distributorMasterView.StoreCredit);
            else
                count += await _storeCreditDL.UpdateStoreCredit(distributorMasterView.StoreCredit);

            //Address
            if (distributorMasterView.Address != null)
            {
                var Address = await _addressBL.GetAddressDetailsByUID(distributorMasterView.Address.UID);
                if (Address == null)
                    count += await _addressBL.CreateAddressDetails(distributorMasterView.Address);
                else
                    count += await _addressBL.UpdateAddressDetails(distributorMasterView.Address);
            }

            int count1 = 0, count2 = 0, count3 = 0;

            //Contacts
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>()
            {
                new FilterCriteria("LinkedItemUID",distributorMasterView.Org.UID,FilterType.Equal),
            };

            PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact> ContactList = await _contactBL.SelectAllContactDetails(null, 0, 0, filterCriterias, false);
            foreach (var contact in distributorMasterView.Contacts)
            {
                bool isExist = ContactList.PagedData.Any(p => p.UID == contact.UID);
                count1 += isExist ? await _contactBL.UpdateContactDetails(contact) : await _contactBL.CreateContactDetails(contact);
            }
            //Documents
            filterCriterias = new List<FilterCriteria>()
            {
                new FilterCriteria("StoreUID",distributorMasterView.Store.UID,FilterType.Equal),
            };
            PagedResponse<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument> DocuList = await _storeDocumentBL.SelectAllStoreDocumentDetails(null, 0, 0, filterCriterias, false);
            foreach (var document in distributorMasterView.Documents)
            {
                bool isExist = DocuList.PagedData.Any(p => p.UID == document.UID);
                count2 += isExist ? await _storeDocumentBL.UpdateStoreDocumentDetails(document) : await _storeDocumentBL.CreateStoreDocumentDetails(document);
            }
            //Currency
            if (distributorMasterView.OrgCurrencyList != null && distributorMasterView.OrgCurrencyList.Count > 0)
            {
                var CurrencyList = await _currencyDL.GetOrgCurrencyListByOrgUID(distributorMasterView?.Org?.UID);
                foreach (var item in distributorMasterView.OrgCurrencyList)
                {
                    bool isexist = CurrencyList.Any(i => i.UID == item.UID);
                    count3 += isexist ? await _currencyDL.UpdateOrgCurrency(item) : await _currencyDL.CreateOrgCurrency(item);
                }
            }
            retVal = count + count1 + count2 + count3;

            return retVal;
        }
        public async Task<PagedResponse<Winit.Modules.Distributor.Model.Interfaces.IDistributor>> SelectAllDistributors(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _distributorDL.SelectAllDistributors(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);

        }
        public async Task<DistributorMasterView> GetDistributorDetailsByUID(string UID)
        {
            DistributorMasterView distributorMasterView = new();
            distributorMasterView.Org = (Org.Model.Classes.Org)await _orgBL.GetOrgByUID(UID);
            distributorMasterView.Store = (Store.Model.Classes.Store)await _storeDL.SelectStoreByUID(UID);
            distributorMasterView.StoreAdditionalInfo = (Store.Model.Classes.StoreAdditionalInfo)await _storeAdditionalInfoDL.SelectStoreAdditionalInfoByStoreUID(UID);
            distributorMasterView.StoreCredit = (Store.Model.Classes.StoreCredit)await _storeCreditDL.SelectStoreCreditByStoreUID(UID);

            List<FilterCriteria> filterCriterias = new List<FilterCriteria>()
            {
                new FilterCriteria("LinkedItemUID",UID,FilterType.Equal),
            };
            PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact> pagedResponse= await _contactBL.SelectAllContactDetails(null, 0, 0, filterCriterias, true);
            if(pagedResponse?.TotalCount > 0)
            {
                distributorMasterView.Contacts = new();
                foreach (var item in pagedResponse.PagedData)
                {
                    distributorMasterView.Contacts.Add((Contact.Model.Classes.Contact)item);
                }
            }
            

            filterCriterias = new List<FilterCriteria>()
            {
                new FilterCriteria("StoreUid",UID,FilterType.Equal)
            };
            var documents =await _storeDocumentBL.SelectAllStoreDocumentDetails(null, 0, 0, filterCriterias, true);
            if(documents?.TotalCount > 0)
            {
                distributorMasterView.Documents = new();
                foreach (var document in documents.PagedData)
                {
                    distributorMasterView.Documents.Add((StoreDocument.Model.Classes.StoreDocument)document);
                }
            }
            filterCriterias = new List<FilterCriteria>()
            {
                new FilterCriteria("LinkedItemUID",UID,FilterType.Equal)
            };
            var address = await _addressBL.SelectAllAddressDetails(null, 0, 0, filterCriterias, true);
            if (address?.TotalCount > 0)
            {
                 distributorMasterView.Address = (Winit.Modules.Address.Model.Classes.Address?)address?.PagedData?.FirstOrDefault();
            }
                
            var orgcurrency = await _currencyDL.GetOrgCurrencyListByOrgUID(UID);
            distributorMasterView.OrgCurrencyList = new();
            foreach (var item in orgcurrency)
            {
                distributorMasterView.OrgCurrencyList.Add((Winit.Modules.Currency.Model.Classes.OrgCurrency)item);
            }
            return distributorMasterView;
        }
    }
}
