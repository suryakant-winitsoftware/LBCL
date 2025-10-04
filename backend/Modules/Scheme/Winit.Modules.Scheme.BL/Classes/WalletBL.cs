using System;
using System.Collections.Generic;
using System.Data;
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
    public class WalletBL : IWalletBL
    {
        private readonly IWalletDL _walletDL;

        public WalletBL(IWalletDL walletDL)
        {
            _walletDL = walletDL;
        }

        public async Task<PagedResponse<IWallet>> SelectAllWallet(
            List<SortCriteria> sortCriterias,
            int pageNumber,
            int pageSize,
            List<FilterCriteria> filterCriterias,
            bool isCountRequired)
        {
            return await _walletDL.SelectAllWallet(
                sortCriterias,
                pageNumber,
                pageSize,
                filterCriterias,
                isCountRequired
            );
        }

        public async Task<IWallet> GetWalletByUID(string UID)
        {
            return await _walletDL.GetWalletByUID(UID);
        }
        public async Task<List<IWallet>> GetWalletByOrgUID(string OrgUID)
        {
            return await _walletDL.GetWalletByOrgUID(OrgUID);
        }

        public async Task<int> CreateWallet(IWallet wallet)
        {
            return await _walletDL.CreateWallet(wallet);
        }

        public async Task<int> UpdateWallet(IWallet wallet)
        {
            return await _walletDL.UpdateWallet(wallet);
        }

        public async Task<int> DeleteWallet(string UID)
        {
            return await _walletDL.DeleteWallet(UID);
        }
        public async Task<int> UpdateWalletAsync(List<IWalletLedger> walletLedgers, IDbConnection? connection = null,
       IDbTransaction? transaction = null)
        {
            return await _walletDL.UpdateWalletAsync(walletLedgers, connection, transaction);
        }
    }
}
