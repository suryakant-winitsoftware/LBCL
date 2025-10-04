using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Currency.DL.Interfaces
{
    public interface ICurrencyDL
    {
        Task<PagedResponse<Winit.Modules.Currency.Model.Interfaces.ICurrency>> GetCurrencyDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Currency.Model.Interfaces.ICurrency> GetCurrencyById(string UID);
        Task<Winit.Modules.Currency.Model.Interfaces.ICurrency> GetCurrencyListByOrgUID(string OrgUID);
        Task<int> CreateCurrency(Winit.Modules.Currency.Model.Interfaces.ICurrency createCurrency);
        Task<int> CreateOrgCurrency(Winit.Modules.Currency.Model.Interfaces.IOrgCurrency createOrgCurrency);
        Task<int> UpdateOrgCurrency(Winit.Modules.Currency.Model.Interfaces.IOrgCurrency createOrgCurrency);
        Task<int> UpdateCurrency(Winit.Modules.Currency.Model.Interfaces.ICurrency updateCurrency);
        Task<int> DeleteCurrency(string UID);
        Task<int> DeleteOrgCurrency(string UID);
        Task<IEnumerable<Model.Interfaces.IOrgCurrency>> GetOrgCurrencyListBySelectedOrg(string orgUID);
        Task<List<IOrgCurrency>> GetOrgCurrencyListByOrgUID(string orgUID);

    }
}
