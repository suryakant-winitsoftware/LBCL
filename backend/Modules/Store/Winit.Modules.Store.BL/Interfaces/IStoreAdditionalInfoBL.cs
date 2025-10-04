using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreAdditionalInfoBL
    {
        Task<PagedResponse<Model.Interfaces.IStoreAdditionalInfo>> SelectAllStoreAdditionalInfo(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Model.Interfaces.IStoreAdditionalInfo> SelectStoreAdditionalInfoByUID(string UID);
        Task<int> CreateStoreAdditionalInfo(Model.Interfaces.IStoreAdditionalInfo storeAdditionalInfo);
        Task<int> UpdateStoreAdditionalInfo(Model.Interfaces.IStoreAdditionalInfo storeAdditionalInfo);
        Task<int> DeleteStoreAdditionalInfo(string UID);

        Task<int> CreatePaymentForMobile(Model.Interfaces.IPayment payment);
        Task<int> UpdatePaymentForMobile(Model.Interfaces.IPayment payment);
        Task<Model.Interfaces.IPayment> SelectPaymentByUID(string UID);
          Task<int> CreateWeekDaysForMobile(Model.Interfaces.IWeekDays payment);
        Task<int> UpdateWeekDaysForMobile(Model.Interfaces.IWeekDays payment);
        Task<Model.Interfaces.IWeekDays> SelectWeekDaysByUID(string UID);

    }
}
