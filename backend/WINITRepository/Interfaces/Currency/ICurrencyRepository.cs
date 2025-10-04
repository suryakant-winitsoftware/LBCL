using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Bank.PostgreSQLBankRepository;

namespace WINITRepository.Interfaces.Currency
{
    public interface ICurrencyRepository
    {
        Task<IEnumerable<WINITSharedObjects.Models.Currency>> GetCurrencyDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

        Task<WINITSharedObjects.Models.Currency> GetCurrencyById(string UID);

        Task<WINITSharedObjects.Models.Currency> CreateCurrency(WINITSharedObjects.Models.Currency createCurrency);
        Task<int> UpdateCurrency(WINITSharedObjects.Models.Currency updateCurrency);
        Task<int> DeleteCurrency(string UID);
    }
}
