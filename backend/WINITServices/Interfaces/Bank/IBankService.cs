using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
//using static WINITRepository.Classes.Customers.PostgreSQLCustomerRepository;

namespace WINITServices.Interfaces
{
    public interface IBankService
    {
        Task<IEnumerable<WINITSharedObjects.Models.Bank>> GetBankDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

        Task<WINITSharedObjects.Models.Bank> GetBankDetailsByUID(string UID);

        Task<WINITSharedObjects.Models.Bank> CreateBankDetails(Bank bank);
        Task<int> UpdateBankDetails(Bank bankDetails);
        Task<int> DeleteBankDetail(String UID);
    }
}
