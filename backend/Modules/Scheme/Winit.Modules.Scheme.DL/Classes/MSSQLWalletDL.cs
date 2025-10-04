using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class MSSQLWalletDL : SqlServerDBManager,IWalletDL
    {
        public MSSQLWalletDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config) { }
        public async Task<PagedResponse<IWallet>> SelectAllWallet(
        List<SortCriteria> sortCriterias,
        int pageNumber,
        int pageSize,
        List<FilterCriteria> filterCriterias,
        bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (select 
                                id,	uid,	created_by	,created_time	modified_by	,modified_time	,server_add_time	,server_modified_time	,ss,	linked_item_uid ,linked_item_type 	,type,
                                currency_uid,	last_updated_on,	last_transaction_type,	last_transaction_uid,	actual_amount,used_amount,	balance_amount
                                 from wallet
                                             ) As SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                 (select 
                                id,	uid,	created_by	,created_time	modified_by	,modified_time	,server_add_time	,server_modified_time	,ss,	linked_item_uid ,linked_item_type 	,type,
                                currency_uid,	last_updated_on,	last_transaction_type,	last_transaction_uid,	actual_amount,used_amount,	balance_amount
                                 from wallet
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
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
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

                var sql = @"select 
                                id,	uid,	created_by	,created_time	modified_by	,modified_time	,server_add_time	,server_modified_time	,ss,	linked_item_uid ,linked_item_type 	,type,
                                currency_uid,	last_updated_on,	last_transaction_type,	last_transaction_uid,	actual_amount,used_amount,	balance_amount
                                 from wallet
                WHERE uid = @UID";

                return await ExecuteSingleAsync<IWallet>(sql, parameters);
            }
            catch (Exception)
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

                var sql = @"select * from wallet where org_uid=@OrgUID";

                return await ExecuteQueryAsync<IWallet>(sql, parameters);
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
                    uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, linked_item_uid ,linked_item_type , type, 
                    currency_uid, last_updated_on, last_transaction_type, last_transaction_uid, actual_amount, used_amount, balance_amount
                ) 
                VALUES (
                    @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Ss, @linked_item_uid,@LinkedItemType, @Type, 
                    @CurrencyUid, @LastUpdatedOn, @LastTransactionType, @LastTransactionUid, @ActualAmount, @UsedAmount, @BalanceAmount
                );";

                return await ExecuteNonQueryAsync(sql, wallet);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateWallet(IWallet wallet)
        {
            var sql = @"
            UPDATE wallet 
                    SET 
                        created_by = @CreatedBy, 
                        created_time = @CreatedTime, 
                        modified_by = @ModifiedBy, 
                        modified_time = @ModifiedTime, 
                        server_add_time = @ServerAddTime, 
                        server_modified_time = @ServerModifiedTime, 
                        ss = @Ss, 
                        linked_item_type =@LinkedItemType,
                        linked_item_uid =@LinkedItemUid,
                        type = @Type, 
                        currency_uid = @CurrencyUid, 
                        last_updated_on = @LastUpdatedOn, 
                        last_transaction_type = @LastTransactionType, 
                        last_transaction_uid = @LastTransactionUid, 
                        actual_amount = @ActualAmount, 
                        used_amount = @UsedAmount, 
                        balance_amount = @BalanceAmount
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

       
        public async Task<int> UpdateWalletAsync(List<IWalletLedger> walletLedgers, IDbConnection? connection = null,
        IDbTransaction? transaction = null)
        {
            if (walletLedgers == null || walletLedgers.Count == 0)
            {
                return 0;
            }
            int retValue = -1;
            if (connection == null)
            {
                using (connection = CreateConnection())
                {
                    connection.Open();

                    using (transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (IWalletLedger walletLedger in walletLedgers)
                            {
                                int noOfRecord = await CreateWalletLedger(walletLedger, connection, transaction);
                            }
                            retValue = 1;
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                // Rollback the transaction if it hasn't been completed
                                if (transaction?.Connection != null)
                                {
                                    transaction?.Rollback();
                                }
                            }
                            catch (Exception rollbackEx)
                            {
                                // Log or handle the rollback exception
                                Console.WriteLine($"Rollback exception: {rollbackEx.Message}");
                            }
                            throw;
                        }
                    }
                }
            }
            else
            {
                try
                {
                    foreach (IWalletLedger walletLedger in walletLedgers)
                    {
                        int noOfRecord = await CreateWalletLedger(walletLedger, connection, transaction);
                    }
                    retValue = 1;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return retValue;
        }
        private async Task<int> CreateWalletLedger(IWalletLedger walletLedger,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retValue = -1;
            try
            {
                if (walletLedger == null)
                {
                    return retValue;
                }
                // Code for inserting data into the sales_order_info table
                var salesOrderQuery = @"
                                INSERT INTO wallet_ledger (
                                  org_uid,uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, linked_item_type, linked_item_uid,
                                  transaction_date_time, source_type, source_uid, document_number, type, amount, balance
                                ) VALUES (
                                  @orguid,@uid, 1, @createdBy, @createdTime, @modifiedBy, @modifiedTime, @serverAddTime, @serverModifiedTime, @LinkedItemType, @LinkedItemUid,
                                  @transaction_date_time, @source_type, @source_uid, @document_number, @type, @amount, @balance
                                );";

                var parameters = new Dictionary<string, object?>
                {
                                { "@orguid", walletLedger.OrgUid },
                                { "@uid", walletLedger.UID },
                                { "@createdBy", walletLedger.CreatedBy },
                                { "@createdTime", walletLedger.CreatedTime },
                                { "@modifiedBy", walletLedger.ModifiedBy },
                                { "@modifiedTime", walletLedger.ModifiedTime },
                                { "@serverAddTime", walletLedger.ServerAddTime },
                                { "@serverModifiedTime", walletLedger.ServerModifiedTime },
                                { "@LinkedItemType", walletLedger.LinkedItemType },
                                { "@LinkedItemUid", walletLedger.LinkedItemUid },
                                { "@transaction_date_time", walletLedger.TransactionDateTime },
                                { "@source_type", walletLedger.SourceType },
                                { "@source_uid", walletLedger.SourceUid },
                                { "@document_number", walletLedger.DocumentNumber },
                                { "@type", walletLedger.Type },
                                //{ "@CreditType", walletLedger.CreditType },
                                { "@amount", walletLedger.Amount },
                                { "@balance", walletLedger.Balance },
                            
                            };
                retValue = await ExecuteNonQueryAsync(salesOrderQuery, connection, transaction, parameters);
                if (retValue > 0)
                {
                    await CreateWalletAvailableInDb(walletLedger, connection, transaction);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return retValue;
        }
        private async Task CreateWalletAvailableInDb(IWalletLedger walletLedger,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int wHStockAvailableStatus = 0;
            Winit.Modules.Scheme.Model.Interfaces.IWallet? walletAvailable = await SelectWalletAvailableIfExists(walletLedger.Type, walletLedger.LinkedItemType, walletLedger.LinkedItemUid, walletLedger.OrgUid, connection, transaction);
            if (walletAvailable != null)
            {
                wHStockAvailableStatus = await UpdateWalletAvailableInDb(walletAvailable, walletLedger.Type, walletLedger.Amount, walletLedger.CreatedBy, walletLedger.UID, connection, transaction);
            }
            else
            {
                wHStockAvailableStatus = await InsertWalletAvailableInDb(walletLedger, connection, transaction);
            }
        }
        private async Task<Winit.Modules.Scheme.Model.Interfaces.IWallet?> SelectWalletAvailableIfExists(string type, string linkedItemType, string linkedItemUid,string OrgUid,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            Winit.Modules.Scheme.Model.Interfaces.IWallet? walletAvailable = null;
            
                string query = string.Format($@"select uid UID,linked_item_uid,linked_item_type, type Type ,currency_uid CurrencyUid,
                                            last_updated_on LastUpdatedOn,
                                            last_transaction_type LastTransactionType,
                                            last_transaction_uid LastTransactionUid,
                                            actual_amount ActualAmount,
                                            used_amount UsedAmount,balance_amount BalanceAmount,org_uid from wallet where
                                            type=@type and linked_item_uid=@linked_item_uid and org_uid=@org_uid");
            try
            {
                var parameters = new Dictionary<string, object?>
                            {
                    { "@type", type },
                                { "@linked_item_type", linkedItemType },
                                { "@linked_item_uid", linkedItemUid },
                                { "@org_uid", OrgUid }
                                
                            };
                walletAvailable = await ExecuteSingleAsync<Winit.Modules.Scheme.Model.Interfaces.IWallet>(query, parameters, null,
                    connection, transaction);
            }
            catch (Exception ex)
            {
                throw;
            }
            return walletAvailable;
        }
        private async Task<int> UpdateWalletAvailableInDb(Winit.Modules.Scheme.Model.Interfaces.IWallet wHStockAvailable,
            string type, decimal amount, string? createdBy, string lastWallerLedgerUID,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int returnValue = 0;
            decimal finalAmount= wHStockAvailable.ActualAmount + (amount);
            string query = string.Format(@"UPDATE wallet SET actual_amount = @actual_amount, 
                                        modified_by = @ModifiedBy, modified_time = @ModifiedTime, last_transaction_uid = @last_transaction_uid
                                        WHERE uid = @UID");

            var parameters = new Dictionary<string, object?>
                            {
                                { "@actual_amount", finalAmount },
                                { "@ModifiedBy", createdBy},
                                { "@ModifiedTime", DateTime.Now},
                                { "@last_transaction_uid", lastWallerLedgerUID},
                                { "@UID", wHStockAvailable.UID }
                            };

            try
            {
                returnValue = await ExecuteNonQueryAsync(query, connection, transaction, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
            return returnValue;
        }
        private async Task<int> InsertWalletAvailableInDb(IWalletLedger walletLedger,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int returnValue = 0;

            string query = string.Format(@"INSERT INTO wallet (uid,created_by,created_time,modified_by,modified_time,server_add_time
                                    ,server_modified_time,linked_item_uid,linked_item_type,type,currency_uid,last_updated_on,last_transaction_type,last_transaction_uid,actual_amount
                                    ,org_uid) 
                VALUES(@uid,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime
                                    ,@ServerModifiedTime,@LinkedItemUid,@LinkedItemType,@Type,@CurrencyUid,@LastUpdatedOn,@LastTransactionType,@LastTransactionUid,@ActualAmount,@OrgUid
                                    )");
            // Here ss = 0 because we are not sending wh_stock_available to server

            var parameters = new Dictionary<string, object?>
                            {
                                { "@uid", Guid.NewGuid() },
                                { "@CreatedBy", walletLedger.CreatedBy },
                                { "@CreatedTime", DateTime.Now },
                                { "@ModifiedBy", walletLedger.CreatedBy},
                                { "@ModifiedTime", DateTime.Now },
                                { "@ServerAddTime", DateTime.Now },
                                { "@ServerModifiedTime", DateTime.Now },
                                { "@OrgUID", walletLedger.OrgUid },
                                { "@Type", walletLedger.Type },
                                { "@CurrencyUid", ""},
                                { "@LastUpdatedOn", DateTime.Now },
                                { "@LastTransactionType", walletLedger.Type },
                                { "@LastTransactionUid", walletLedger.UID },
                                { "@LinkedItemUid", walletLedger.LinkedItemUid },
                                { "@LinkedItemType", walletLedger.LinkedItemType },
                                { "@ActualAmount", walletLedger.Amount },
                                { "@OrgUid", walletLedger.OrgUid },
                            };

            try
            {
                returnValue = await ExecuteNonQueryAsync(query, connection, transaction, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
            return returnValue;
        }


    }
}
