using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Bank.BL.Classes
{
    public class BankBL:BankBaseBL,Interfaces.IBankBL
    {
        protected readonly DL.Interfaces.IBankDL _bankDL = null;
        public BankBL(DL.Interfaces.IBankDL bankDL)
        {
            _bankDL = bankDL;   
        }
        public async  Task<PagedResponse<Winit.Modules.Bank.Model.Interfaces.IBank>> GetBankDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _bankDL.GetBankDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async  Task<Winit.Modules.Bank.Model.Interfaces.IBank> GetBankDetailsByUID(string UID)
        {
            return await _bankDL.GetBankDetailsByUID(UID);
        }
        public async  Task<int> CreateBankDetails(Winit.Modules.Bank.Model.Interfaces.IBank bank)
        {
            return await _bankDL.CreateBankDetails(bank);
        }

        public async  Task<int> UpdateBankDetails(Winit.Modules.Bank.Model.Interfaces.IBank bankDetails)
        {
            return await _bankDL.UpdateBankDetails(bankDetails);
        }

        public async  Task<int> DeleteBankDetail(String UID)
        {
            return await _bankDL.DeleteBankDetail(UID);
        }
    }
}
