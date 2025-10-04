using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreAdditionalInfoCMIBL   
    {
        Task<int> CreateStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<int> UpdateStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<int> UpdateBusinessDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<int> UpdateShowroomDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<int> UpdateEmployeeDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<int> UpdateKartaInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<int> UpdateDistBusinessDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<int> UpdateAreaOfDistAgreedInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<int> UpdateBankersDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<int> UpdateEarlierWorkWithCMIInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<int> UpdateTermAndCondInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<Model.Interfaces.IStoreAdditionalInfoCMI> SelectStoreAdditionalInfoCMIByUID(string UID);
        Task<int> UpdateAreaofOperationAgreedInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model);
        Task<int> UpdateServiceCenterDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model);

    }
}

