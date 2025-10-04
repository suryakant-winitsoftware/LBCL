using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class WalletLedgerBL: IWalletLedgerBL
    {
        private readonly IWalletLedgerDL _walletLedgerDL;

        public WalletLedgerBL(IWalletLedgerDL walletLedgerDL)
        {
            _walletLedgerDL = walletLedgerDL;
        }

        public async Task<PagedResponse<IWalletLedger>> SelectAllWalletLedger(
            List<SortCriteria> sortCriterias,
            int pageNumber,
            int pageSize,
            List<FilterCriteria> filterCriterias,
            bool isCountRequired)
        {
            return await _walletLedgerDL.SelectAllWalletLedger(
                sortCriterias,
                pageNumber,
                pageSize,
                filterCriterias,
                isCountRequired
            );
        }

        public async Task<IWalletLedger> GetWalletLedgerByUID(string UID)
        {
            return await _walletLedgerDL.GetWalletLedgerByUID(UID);
        }

        public async Task<int> CreateWalletLedger(IWalletLedger walletLedger)
        {
            return await _walletLedgerDL.CreateWalletLedger(walletLedger);
        }

        public async Task<int> UpdateWalletLedger(IWalletLedger walletLedger)
        {
            return await _walletLedgerDL.UpdateWalletLedger(walletLedger);
        }

        public async Task<int> DeleteWalletLedger(string UID)
        {
            return await _walletLedgerDL.DeleteWalletLedger(UID);
        }
    }
}
