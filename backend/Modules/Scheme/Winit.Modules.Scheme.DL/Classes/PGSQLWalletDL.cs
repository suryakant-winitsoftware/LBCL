using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class PGSQLWalletDL:PostgresDBManager,IWalletDL
    {
        public PGSQLWalletDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config) { }
        public async Task<PagedResponse<IWallet>> SelectAllWallet(
        List<SortCriteria> sortCriterias,
        int pageNumber,
        int pageSize,
        List<FilterCriteria> filterCriterias,
        bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                                 org_uid, type, amount, currency_uid, 
                                                 last_updated_on, last_transaction_type, 
                                                 last_transaction_uid
                                             FROM 
                                                 wallet
                                             ) As SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                 (SELECT 
                                         org_uid, type, amount, currency_uid, 
                                         last_updated_on, last_transaction_type, 
                                         last_transaction_uid
                                     FROM 
                                         wallet
                                 ) As SubQuery");
                }

                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<IWallet>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY org_uid OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                IEnumerable<IWallet> wallets = await ExecuteQueryAsync<IWallet>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<IWallet> pagedResponse = new PagedResponse<IWallet>
                {
                    PagedData = wallets,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IWallet> GetWalletByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "UID", UID }
            };

                var sql = @"SELECT 
                    org_uid, type, amount, currency_uid, 
                    last_updated_on, last_transaction_type, 
                    last_transaction_uid
                FROM 
                    wallet
                WHERE uid = @UID";

                return await ExecuteSingleAsync<IWallet>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        

        public async Task<int> CreateWallet(IWallet wallet)
        {
            try
            {
                var sql = @"
            INSERT INTO wallet (
                    org_uid, type, amount, currency_uid, 
                    last_updated_on, last_transaction_type, 
                    last_transaction_uid
                ) VALUES (
                    @OrgUid, @Type, @Amount, @CurrencyUid, 
                    @LastUpdatedOn, @LastTransactionType, 
                    @LastTransactionUid);";

                return await ExecuteNonQueryAsync(sql, wallet);
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<IWallet>> GetWalletByOrgUID(string OrgUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "OrgUID", OrgUID }
            };

                var sql = @"SELECT 
                    org_uid, type, amount, currency_uid, 
                    last_updated_on, last_transaction_type, 
                    last_transaction_uid
                FROM 
                    wallet
                WHERE org_uid = @OrgUID";

                return await ExecuteQueryAsync<IWallet>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateWallet(IWallet wallet)
        {
            var sql = @"
            UPDATE wallet
                 SET
                     org_uid = @OrgUid,
                     type = @Type,
                     amount = @Amount,
                     currency_uid = @CurrencyUid,
                     last_updated_on = @LastUpdatedOn,
                     last_transaction_type = @LastTransactionType,
                     last_transaction_uid = @LastTransactionUid
             WHERE
                 uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, wallet);
        }

        public async Task<int> DeleteWallet(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            {"UID", UID}
        };
            var sql = @"DELETE FROM wallet WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateWalletAsync(List<IWalletLedger> walletLedgers, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            throw new NotImplementedException();
        }
    }
}
