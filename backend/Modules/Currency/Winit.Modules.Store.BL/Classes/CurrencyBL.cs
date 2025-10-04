using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Currency.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Currency.BL.Classes
{
    public class CurrencyBL : ICurrencyBL
    {
        protected readonly DL.Interfaces.ICurrencyDL _currencyRepository = null;
        public CurrencyBL(DL.Interfaces.ICurrencyDL currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }
        public async Task<PagedResponse<Winit.Modules.Currency.Model.Interfaces.ICurrency>> GetCurrencyDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _currencyRepository.GetCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Currency.Model.Interfaces.ICurrency> GetCurrencyById(string UID)
        {
            return await _currencyRepository.GetCurrencyById(UID);
        }
        public async Task<Winit.Modules.Currency.Model.Interfaces.ICurrency> GetCurrencyListByOrgUID(string OrgUID)
        {
            return await _currencyRepository.GetCurrencyListByOrgUID(OrgUID);
        }
        public async Task<int> CreateCurrency(Winit.Modules.Currency.Model.Interfaces.ICurrency createCurrency)
        {
            return await _currencyRepository.CreateCurrency(createCurrency);
        }
        public async Task<int> CreateOrgCurrency(Winit.Modules.Currency.Model.Interfaces.IOrgCurrency createOrgCurrency)
        {
            return await _currencyRepository.CreateOrgCurrency(createOrgCurrency);
        }
        public async Task<int> UpdateOrgCurrency(Winit.Modules.Currency.Model.Interfaces.IOrgCurrency createOrgCurrency)
        {
            return await _currencyRepository.UpdateOrgCurrency(createOrgCurrency);
        }
        public async Task<int> UpdateCurrency(Winit.Modules.Currency.Model.Interfaces.ICurrency updateCurrency)
        {
            return await _currencyRepository.UpdateCurrency(updateCurrency);
        }
        public async Task<int> DeleteCurrency(string UID)
        {
            return await _currencyRepository.DeleteCurrency(UID);
        }
        public async Task<int> DeleteOrgCurrency(string UID)
        {
            return await _currencyRepository.DeleteOrgCurrency(UID);
        }
        public async Task<IEnumerable<Model.Interfaces.IOrgCurrency>> GetOrgCurrencyListBySelectedOrg(string orgUID)
        {
            return await _currencyRepository.GetOrgCurrencyListBySelectedOrg(orgUID);
        }
    }
}
