using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Bank.PostgreSQLBankRepository;

namespace WINITRepository.Interfaces.Bank
{
    public interface IBankRepository
    {
        Task<IEnumerable<WINITSharedObjects.Models.Bank>> GetBankDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

        Task<WINITSharedObjects.Models.Bank> GetBankDetailsByUID(string UID);
        Task<WINITSharedObjects.Models.Bank> CreateBankDetails(WINITSharedObjects.Models.Bank bank);

        Task<int> UpdateBankDetails(WINITSharedObjects.Models.Bank bankDetails);
        Task<int> DeleteBankDetail(String UID);
    }
}
