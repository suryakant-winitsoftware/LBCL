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
    public interface IOrgCurrencyService
    {
        Task<IEnumerable<WINITSharedObjects.Models.OrgCurrency>> GetOrgCurrencyDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

       Task<WINITSharedObjects.Models.OrgCurrency> GetOrgCurrencyByOrgUID(string OrgUID);

        Task<WINITSharedObjects.Models.OrgCurrency> CreateOrgCurrency(OrgCurrency createOrgCurrency);
        Task<int> UpdateOrgCurrency(WINITSharedObjects.Models.OrgCurrency updateOrgCurrency);
        Task<int> DeleteOrgCurrency(string OrgUID);
    }
}
