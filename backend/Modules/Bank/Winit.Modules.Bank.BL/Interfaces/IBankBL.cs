using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Bank.BL.Interfaces
{
    public interface IBankBL
    {
        Task<PagedResponse<Winit.Modules.Bank.Model.Interfaces.IBank>> GetBankDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Bank.Model.Interfaces.IBank> GetBankDetailsByUID(string UID);
        Task<int> CreateBankDetails(Winit.Modules.Bank.Model.Interfaces.IBank bank);
        Task<int> UpdateBankDetails(Winit.Modules.Bank.Model.Interfaces.IBank bankDetails);
        Task<int> DeleteBankDetail(String UID);
    }
}
