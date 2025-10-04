using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CreditLimit.BL.Interfaces
{
    public interface ITemporaryCreditBL
    {
        Task<PagedResponse<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit>> SelectTemporaryCreditDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string jobPositionUID);
        Task<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit> GetTemporaryCreditByUID(string UID);
        Task<int> CreateTemporaryCreditDetails(Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit temporaryCredit);
        Task<int> UpdateTemporaryCreditDetails(Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit temporaryCredit);
        Task<int> DeleteTemporaryCreditDetails(String UID);
    }
}
