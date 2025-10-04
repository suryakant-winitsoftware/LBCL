using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreAdditionalInfoBL : StoreBaseBL, Interfaces.IStoreAdditionalInfoBL
    {
        protected readonly DL.Interfaces.IStoreAdditionalInfoDL _storeAdditionalInfoRepository;
        public StoreAdditionalInfoBL(DL.Interfaces.IStoreAdditionalInfoDL storeAdditionalInfoRepository)
        {
            _storeAdditionalInfoRepository = storeAdditionalInfoRepository;
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreAdditionalInfo>> SelectAllStoreAdditionalInfo(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeAdditionalInfoRepository.SelectAllStoreAdditionalInfo(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Model.Interfaces.IStoreAdditionalInfo> SelectStoreAdditionalInfoByUID(string UID)
        {
            return await _storeAdditionalInfoRepository.SelectStoreAdditionalInfoByUID(UID);
        }
        public async Task<int> CreateStoreAdditionalInfo(Model.Interfaces.IStoreAdditionalInfo storeAdditionalInfo)
        {
            return await _storeAdditionalInfoRepository.CreateStoreAdditionalInfo(storeAdditionalInfo);
        }
        public async Task<int> UpdateStoreAdditionalInfo(Model.Interfaces.IStoreAdditionalInfo storeAdditionalInfo)
        {
            return await _storeAdditionalInfoRepository.UpdateStoreAdditionalInfo(storeAdditionalInfo);
        }
        public async Task<int> DeleteStoreAdditionalInfo(string storeUID)
        {
            return await _storeAdditionalInfoRepository.DeleteStoreAdditionalInfo(storeUID);

        }
        public async Task<int> CreatePaymentForMobile(Model.Interfaces.IPayment payment)
        {
            return await _storeAdditionalInfoRepository.CreatePaymentForMobile(payment);
        }
        public async Task<int> UpdatePaymentForMobile(Model.Interfaces.IPayment payment)
        {
            return await _storeAdditionalInfoRepository.UpdatePaymentForMobile(payment);
        }
        public async Task<Model.Interfaces.IPayment> SelectPaymentByUID(string UID)
        {
            return await _storeAdditionalInfoRepository.SelectPaymentByUID(UID);
        }

        public async Task<int> CreateWeekDaysForMobile(Model.Interfaces.IWeekDays weekDays)
        {
            return await _storeAdditionalInfoRepository.CreateWeekDaysForMobile(weekDays);
        }
        public async Task<int> UpdateWeekDaysForMobile(Model.Interfaces.IWeekDays WeeDays)
        {
            return await _storeAdditionalInfoRepository.UpdateWeekDaysForMobile(WeeDays);
        }
        public async Task<Model.Interfaces.IWeekDays> SelectWeekDaysByUID(string UID)
        {
            return await _storeAdditionalInfoRepository.SelectWeekDaysByUID(UID);
        }



    }
}
