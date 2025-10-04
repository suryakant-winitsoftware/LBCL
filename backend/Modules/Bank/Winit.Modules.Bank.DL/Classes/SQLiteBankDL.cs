using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Bank.DL.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Bank.DL.Classes
{
    public class SQLiteBankDL:Winit.Modules.Base.DL.DBManager.SqliteDBManager, IBankDL
    {
        public SQLiteBankDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Bank.Model.Interfaces.IBank>> GetBankDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from(SELECT 
                    id AS Id,
                    uid AS Uid,
                    company_uid AS CompanyUid,
                    bank_name AS BankName,
                    country_uid AS CountryUid,
                    cheque_fee AS ChequeFee,
                    ss AS Ss,
                    created_time AS CreatedTime,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime
                FROM 
                    bank) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                    id AS Id,
                    uid AS Uid,
                    company_uid AS CompanyUid,
                    bank_name AS BankName,
                    country_uid AS CountryUid,
                    cheque_fee AS ChequeFee,
                    ss AS Ss,
                    created_time AS CreatedTime,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime
                FROM 
                    bank) as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
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
                //if (pageNumber > 0 && pageSize > 0)
                //{
                //    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IBank>().GetType();
                IEnumerable<Winit.Modules.Bank.Model.Interfaces.IBank> BankDetails = await ExecuteQueryAsync<Winit.Modules.Bank.Model.Interfaces.IBank>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Bank.Model.Interfaces.IBank> pagedResponse = new PagedResponse<Winit.Modules.Bank.Model.Interfaces.IBank>
                {
                    PagedData = BankDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Bank.Model.Interfaces.IBank> GetBankDetailsByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT * FROM SELECT 
                    id AS Id,
                    uid AS Uid,
                    company_uid AS CompanyUid,
                    bank_name AS BankName,
                    country_uid AS CountryUid,
                    cheque_fee AS ChequeFee,
                    ss AS Ss,
                    created_time AS CreatedTime,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime
                FROM 
                    bank) as SubQuery WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IBank>().GetType();
            Winit.Modules.Bank.Model.Interfaces.IBank BankDetails = await ExecuteSingleAsync<Winit.Modules.Bank.Model.Interfaces.IBank>(sql, parameters, type);
            return BankDetails;
        }
        public async Task<int> CreateBankDetails(Winit.Modules.Bank.Model.Interfaces.IBank bank)
        {
            try
            {
                var sql = "INSERT INTO (Bankid, uid, company_uid, bank_name, country_uid, cheque_fee, ss, created_time, modified_time, server_add_time, server_modified_time) VALUES (@UID, @CompanyUID, @BankName,@CountryUID ,@ChequeFee,@SS,@CreatedTime,@ModifiedTime,@ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"UID", bank.UID},
                   {"CompanyUID", bank.CompanyUID},
                   {"BankName", bank.BankName},
                   {"CountryUID", bank.CountryUID},
                   {"ChequeFee", bank.ChequeFee},
                   {"SS", bank.SS},
                   {"CreatedTime", bank.CreatedTime},
                   {"ModifiedTime", bank.ModifiedTime},
                   {"ServerAddTime", bank.ServerAddTime},
                   {"ServerModifiedTime", bank.ServerModifiedTime},

             };
               return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<int> UpdateBankDetails(Winit.Modules.Bank.Model.Interfaces.IBank bankDetails)
        {
            try
            {
                var sql = @"UPDATE bank SET 
                    company_uid = @CompanyUID, 
                    bank_name = @BankName, 
                    country_uid = @CountryUID,
                    cheque_fee = @ChequeFee,
                    ss = @SS,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime
                WHERE
                    uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                   {"CompanyUID", bankDetails.CompanyUID},
                   {"BankName", bankDetails.BankName},
                   {"CountryUID", bankDetails.CountryUID},
                   {"ChequeFee", bankDetails.ChequeFee},
                   {"SS", bankDetails.SS},
                   {"ModifiedTime", bankDetails.ModifiedTime},
                   {"ServerModifiedTime", bankDetails.ServerModifiedTime},
                   {"UID", bankDetails.UID},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> DeleteBankDetail(string UID)
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = "DELETE  FROM bank WHERE uid = @UID";

           return await ExecuteNonQueryAsync(sql, parameters);
        }
    }

    
}
