using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Interfaces
{
    public interface IWalletLedgerDL
    {
        Task<PagedResponse<IWalletLedger>> SelectAllWalletLedger(
       List<SortCriteria> sortCriterias,
       int pageNumber,
       int pageSize,
       List<FilterCriteria> filterCriterias,
       bool isCountRequired);

        Task<IWalletLedger> GetWalletLedgerByUID(string UID);

        Task<int> CreateWalletLedger(IWalletLedger walletLedger);

        Task<int> UpdateWalletLedger(IWalletLedger walletLedger);

        Task<int> DeleteWalletLedger(string UID);
    }
}
