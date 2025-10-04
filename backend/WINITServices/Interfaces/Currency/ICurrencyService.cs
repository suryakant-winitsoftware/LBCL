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
    public interface ICurrencyService
    {
        Task<IEnumerable<WINITSharedObjects.Models.Currency>> GetCurrencyDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

       Task<WINITSharedObjects.Models.Currency> GetCurrencyById(string UID);

        Task<WINITSharedObjects.Models.Currency> CreateCurrency(Currency createCurrency);
        Task<int> UpdateCurrency(Currency updateCurrency);
        Task<int> DeleteCurrency(string UID);
    }
}
