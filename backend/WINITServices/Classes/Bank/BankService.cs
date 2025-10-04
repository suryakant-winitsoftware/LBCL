using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WINITRepository.Interfaces.Products;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Commission.PostgreSQLCommissionRepository;

namespace WINITServices.Classes.Bank
{
    public class BankService : BankBaseService
    {
        public BankService(WINITRepository.Interfaces.Bank.IBankRepository bankRepository) : base(bankRepository)
        {

        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.Bank>> GetBankDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _bankRepository.GetBankDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        }

        public async override Task<WINITSharedObjects.Models.Bank> GetBankDetailsByUID(string UID)
        {
            return await _bankRepository.GetBankDetailsByUID(UID);
        }
        public async override Task<WINITSharedObjects.Models.Bank> CreateBankDetails(WINITSharedObjects.Models.Bank bank)
        {
            return await _bankRepository.CreateBankDetails(bank);
        }

        public async override Task<int> UpdateBankDetails(WINITSharedObjects.Models.Bank bankDetails)
        {
            return await _bankRepository.UpdateBankDetails(bankDetails);
        }

        public async override Task<int> DeleteBankDetail(String UID)
        {
            return await _bankRepository.DeleteBankDetail(UID);
        }
    }
}
