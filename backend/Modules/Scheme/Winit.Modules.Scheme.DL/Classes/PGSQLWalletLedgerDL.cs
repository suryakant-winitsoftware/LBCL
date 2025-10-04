using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
    public class PGSQLWalletLedgerDL:PostgresDBManager,IWalletLedgerDL
    {
       public PGSQLWalletLedgerDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config) { }

        public async Task<PagedResponse<IWalletLedger>> SelectAllWalletLedger(
        List<SortCriteria> sortCriterias,
        int pageNumber,
        int pageSize,
        List<FilterCriteria> filterCriterias,
        bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                                 org_uid, transaction_date_time, source_type, 
                                                 source_uid, document_number, type, CreditType, 
                                                 amount, balance
                                             FROM 
                                                 wallet_ledger
                                             ) As SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                 (SELECT 
                                         org_uid, transaction_date_time, source_type, 
                                         source_uid, document_number, type, CreditType, 
                                         amount, balance
                                     FROM 
                                         wallet_ledger
                                 ) As SubQuery");
                }

                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<IWalletLedger>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY transaction_date_time OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                IEnumerable<IWalletLedger> walletLedgers = await ExecuteQueryAsync<IWalletLedger>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<IWalletLedger> pagedResponse = new PagedResponse<IWalletLedger>
                {
                    PagedData = walletLedgers,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IWalletLedger> GetWalletLedgerByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "UID", UID }
            };

                var sql = @"SELECT 
                    org_uid, transaction_date_time, source_type, source_uid, 
                    document_number, type, CreditType, amount, balance
                FROM 
                    wallet_ledger
                WHERE uid = @UID";

                return await ExecuteSingleAsync<IWalletLedger>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateWalletLedger(IWalletLedger walletLedger)
        {
            try
            {
                var sql = @"
            INSERT INTO wallet_ledger (
                    org_uid, transaction_date_time, source_type, source_uid, 
                    document_number, type, CreditType, amount, balance
                ) VALUES (
                    @OrgUid, @TransactionDateTime, @SourceType, @SourceUid, 
                    @DocumentNumber, @Type, @CreditType, @Amount, @Balance);";

                return await ExecuteNonQueryAsync(sql, walletLedger);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateWalletLedger(IWalletLedger walletLedger)
        {
            var sql = @"
            UPDATE wallet_ledger
                 SET
                     org_uid = @OrgUid,
                     transaction_date_time = @TransactionDateTime,
                     source_type = @SourceType,
                     source_uid = @SourceUid,
                     document_number = @DocumentNumber,
                     type = @Type,
                     CreditType = @CreditType,
                     amount = @Amount,
                     balance = @Balance
             WHERE
                 uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, walletLedger);
        }

        public async Task<int> DeleteWalletLedger(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            {"UID", UID}
        };
            var sql = @"DELETE FROM wallet_ledger WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
