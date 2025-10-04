using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface IWalletBL
    {
        Task<PagedResponse<IWallet>> SelectAllWallet(
        List<SortCriteria> sortCriterias,
        int pageNumber,
        int pageSize,
        List<FilterCriteria> filterCriterias,
        bool isCountRequired);

        Task<IWallet> GetWalletByUID(string UID);
        Task<List<IWallet>> GetWalletByOrgUID(string OrgUID);
        Task<int> CreateWallet(IWallet wallet);

        Task<int> UpdateWallet(IWallet wallet);

        Task<int> DeleteWallet(string UID);
        Task<int> UpdateWalletAsync(List<IWalletLedger> walletLedgers, IDbConnection? connection = null,
       IDbTransaction? transaction = null);
    }
}
