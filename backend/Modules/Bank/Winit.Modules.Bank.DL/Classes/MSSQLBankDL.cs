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
    public class MSSQLBankDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IBankDL
    {
        public MSSQLBankDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Bank.Model.Interfaces.IBank>> GetBankDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                StringBuilder sql = new(@"select * from(SELECT 
                            B.id AS Id,
                            B.uid AS UID,
                            B.company_uid AS CompanyUID,
                            B.bank_name AS BankName,
                            B.bank_Code AS BankCode,
                            B.country_uid AS CountryUID,
                            B.cheque_fee AS ChequeFee,
                            B.ss AS SS,
                            B.created_time AS CreatedTime,
                            B.modified_time AS ModifiedTime,
                            B.server_add_time AS ServerAddTime,
                            B.server_modified_time AS ServerModifiedTime,
                            L.Name as CountryName
                        FROM 
                            bank B LEFT JOIN Location L ON B.Country_UID = L.UID )as sub_query");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                            B.id AS Id,
                            B.uid AS UID,
                            B.company_uid AS CompanyUID,
                            B.bank_name AS BankName,
                            B.bank_Code AS BankCode,
                            B.country_uid AS CountryUID,
                            B.cheque_fee AS ChequeFee,
                            B.ss AS SS,
                            B.created_time AS CreatedTime,
                            B.modified_time AS ModifiedTime,
                            B.server_add_time AS ServerAddTime,
                            B.server_modified_time AS ServerModifiedTime,
                            L.Name as CountryName
                        FROM 
                            bank B LEFT JOIN Location L ON B.Country_UID = L.UID )as sub_query");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Bank.Model.Interfaces.IBank>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.Bank.Model.Interfaces.IBank> bankDetails = await ExecuteQueryAsync<Winit.Modules.Bank.Model.Interfaces.IBank>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Bank.Model.Interfaces.IBank> pagedResponse = new PagedResponse<Winit.Modules.Bank.Model.Interfaces.IBank>
                {
                    PagedData = bankDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
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
            var sql = @"SELECT 
                    id AS Id,
                    uid AS UID,
                    company_uid AS CompanyUID,
                    bank_name AS BankName,
                    bank_code AS BankCode,
                    country_uid AS CountryUID,
                    cheque_fee AS ChequeFee,
                    ss AS SS,
                    created_time AS CreatedTime,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime
                FROM 
                    bank
                 WHERE uid = @UID";
            Winit.Modules.Bank.Model.Interfaces.IBank? bankDetails = await ExecuteSingleAsync<Winit.Modules.Bank.Model.Interfaces.IBank>(sql, parameters);
            return bankDetails;
        }
        public async Task<int> CreateBankDetails(Winit.Modules.Bank.Model.Interfaces.IBank bank)
        {
            try
            {
                var sql = @"INSERT INTO bank (id, uid, company_uid, bank_name, country_uid, cheque_fee, ss, created_time, modified_time,
                            server_add_time, server_modified_time, bank_code) VALUES (@Id, @UID, @CompanyUID, @BankName, @CountryUID,
                            @ChequeFee, @SS, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @BankCode)";
                return await ExecuteNonQueryAsync(sql, bank);
            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<int> UpdateBankDetails(Winit.Modules.Bank.Model.Interfaces.IBank bankDetails)
        {
            try
            {
                var sql = @"UPDATE bank SET company_uid = @CompanyUID, bank_name = @BankName, country_uid = @CountryUID, 
                            cheque_fee = @ChequeFee, ss = @SS, modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime,
                            bank_code = @BankCode WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, bankDetails);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteBankDetail(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = "DELETE  FROM bankWHERE uid = @UID";

            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }


}
