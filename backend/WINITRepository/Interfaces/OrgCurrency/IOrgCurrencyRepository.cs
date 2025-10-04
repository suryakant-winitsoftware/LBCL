using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;

namespace WINITRepository.Interfaces.OrgCurrency
{
    public interface IOrgCurrencyRepository
    {
        Task<IEnumerable<WINITSharedObjects.Models.OrgCurrency>> GetOrgCurrencyDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

        Task<WINITSharedObjects.Models.OrgCurrency> GetOrgCurrencyByOrgUID(string OrgUID);

        Task<WINITSharedObjects.Models.OrgCurrency> CreateOrgCurrency(WINITSharedObjects.Models.OrgCurrency createOrgCurrency);
        Task<int> UpdateOrgCurrency(WINITSharedObjects.Models.OrgCurrency updateOrgCurrency);
        Task<int> DeleteOrgCurrency(string OrgUID);
    }
}
