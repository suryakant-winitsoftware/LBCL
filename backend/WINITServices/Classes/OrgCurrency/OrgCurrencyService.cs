using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WINITRepository.Interfaces.Products;
using WINITServices.Classes.Currency;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Settings.PostgreSQLSettingsRepository;

namespace WINITServices.Classes.OrgCurrency
{
    public class OrgCurrencyService : OrgCurrencyBaseService
    {
        public OrgCurrencyService(WINITRepository.Interfaces.OrgCurrency.IOrgCurrencyRepository orgcurrencyRepository) : base(orgcurrencyRepository)
        {

        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.OrgCurrency>> GetOrgCurrencyDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _orgcurrencyRepository.GetOrgCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        }

        public async override Task<WINITSharedObjects.Models.OrgCurrency> GetOrgCurrencyByOrgUID(string OrgUID)
        {
            return await _orgcurrencyRepository.GetOrgCurrencyByOrgUID(OrgUID);
        }
        public async override Task<WINITSharedObjects.Models.OrgCurrency> CreateOrgCurrency(WINITSharedObjects.Models.OrgCurrency createOrgCurrency)
        {
            return await _orgcurrencyRepository.CreateOrgCurrency(createOrgCurrency);
        }

        public async override Task<int> UpdateOrgCurrency(WINITSharedObjects.Models.OrgCurrency updateOrgCurrency)
        {
            return await _orgcurrencyRepository.UpdateOrgCurrency(updateOrgCurrency);
        }

        public async override Task<int> DeleteOrgCurrency(string OrgUID)
        {
            return await _orgcurrencyRepository.DeleteOrgCurrency(OrgUID);
        }
    }
}
