using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Bank.PostgreSQLBankRepository;

namespace WINITServices.Classes.Bank
{
    public abstract class BankBaseService : Interfaces.IBankService
    {
        protected readonly WINITRepository.Interfaces.Bank.IBankRepository _bankRepository;
        public BankBaseService(WINITRepository.Interfaces.Bank.IBankRepository bankRepository)
        {
            _bankRepository = bankRepository;
        }
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Bank>> GetBankDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

        public abstract Task<WINITSharedObjects.Models.Bank> GetBankDetailsByUID(string UID);

        public abstract Task<WINITSharedObjects.Models.Bank> CreateBankDetails(WINITSharedObjects.Models.Bank bank);
        public abstract Task<int> UpdateBankDetails(WINITSharedObjects.Models.Bank bankDetails);
        public abstract Task<int> DeleteBankDetail(String UID);

    }
}
