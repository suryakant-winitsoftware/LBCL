using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreAdditionalInfoCMIBL : StoreBaseBL, Interfaces.IStoreAdditionalInfoCMIBL
    {
        protected readonly DL.Interfaces.IStoreAdditionalInfoCMIDL _storeAdditionalInfoCMIRepository;
        public StoreAdditionalInfoCMIBL(DL.Interfaces.IStoreAdditionalInfoCMIDL storeAdditionalInfoCMIRepository)
        {
            _storeAdditionalInfoCMIRepository = storeAdditionalInfoCMIRepository;
        }
        public async Task<int> CreateStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.CreateStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
        
        public async Task<int> UpdateStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
        
        public async Task<int> UpdateBusinessDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateBusinessDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
        public async Task<int> UpdateShowroomDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateShowroomDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
        public async Task<int> UpdateKartaInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateKartaInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
        public async Task<int> UpdateEmployeeDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateEmployeeDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
        public async Task<int> UpdateDistBusinessDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateDistBusinessDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
        public async Task<int> UpdateAreaOfDistAgreedInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateAreaOfDistAgreedInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
        public async Task<int> UpdateBankersDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateBankersDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
        public async Task<int> UpdateEarlierWorkWithCMIInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateEarlierWorkWithCMIInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
        public async Task<int> UpdateTermAndCondInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateTermAndCondInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
        public async Task<Model.Interfaces.IStoreAdditionalInfoCMI> SelectStoreAdditionalInfoCMIByUID(string UID)
        {
            return await _storeAdditionalInfoCMIRepository.SelectStoreAdditionalInfoCMIByUID(UID);
        }
        
        public async Task<int> UpdateAreaofOperationAgreedInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateAreaofOperationAgreedInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }

        public async Task<int> UpdateServiceCenterDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return await _storeAdditionalInfoCMIRepository.UpdateServiceCenterDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
        }
    }
}
