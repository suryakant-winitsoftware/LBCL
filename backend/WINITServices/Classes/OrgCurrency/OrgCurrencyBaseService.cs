using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Bank.PostgreSQLBankRepository;

namespace WINITServices.Classes.OrgCurrency
{
    public abstract class OrgCurrencyBaseService : Interfaces.IOrgCurrencyService
    {
        protected readonly WINITRepository.Interfaces.OrgCurrency.IOrgCurrencyRepository _orgcurrencyRepository;
        public OrgCurrencyBaseService(WINITRepository.Interfaces.OrgCurrency.IOrgCurrencyRepository orgcurrencyRepository)
        {
            _orgcurrencyRepository = orgcurrencyRepository;
        }
      public abstract Task<IEnumerable<WINITSharedObjects.Models.OrgCurrency>> GetOrgCurrencyDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

        public abstract Task<WINITSharedObjects.Models.OrgCurrency> GetOrgCurrencyByOrgUID(string OrgUID);

        public abstract Task<WINITSharedObjects.Models.OrgCurrency> CreateOrgCurrency(WINITSharedObjects.Models.OrgCurrency createOrgCurrency);
        public abstract Task<int> UpdateOrgCurrency(WINITSharedObjects.Models.OrgCurrency updateOrgCurrency);
        public abstract Task<int> DeleteOrgCurrency(string OrgUID);

    }
}
