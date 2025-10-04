using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Winit.Modules.Bank.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Bank.DL.Classes
{
    public class PGSQLBankDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IBankDL
    {
        public PGSQLBankDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
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
                            public.bank B LEFT JOIN public.Location L ON B.Country_UID = L.UID )as sub_query");

                StringBuilder sqlCount = new();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select * from(SELECT 
                            B.id AS Id,
                            B.uid AS UID,
                            B.company_uid AS CompanyUID,
                            B.bank_name AS BankName,
                            B.bank_code AS BankCode,
                            B.country_uid AS CountryUID,
                            B.cheque_fee AS ChequeFee,
                            B.ss AS SS,
                            B.created_time AS CreatedTime,
                            B.modified_time AS ModifiedTime,
                            B.server_add_time AS ServerAddTime,
                            B.server_modified_time AS ServerModifiedTime,
                            L.Name as CountryName
                        FROM 
                            bank B LEFT JOIN public.Location L ON B.Country_UID = L.UID )as sub_query");
                }
                Dictionary<string, object> parameters = new();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new();
                    _ = sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Bank.Model.Interfaces.IBank>(filterCriterias, sbFilterCriteria, parameters);
                    _ = sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        _ = sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    _ = sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IBank>().GetType();
                IEnumerable<Winit.Modules.Bank.Model.Interfaces.IBank> BankDetails = await ExecuteQueryAsync<Winit.Modules.Bank.Model.Interfaces.IBank>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Bank.Model.Interfaces.IBank> pagedResponse = new()
                {
                    PagedData = BankDetails,
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
            Dictionary<string, object> parameters = new()
            {
                {"UID",  UID}
            };
            string sql = @"SELECT 
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
                    public.bank
                 WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IBank>().GetType();
            Winit.Modules.Bank.Model.Interfaces.IBank BankDetails = await ExecuteSingleAsync<Winit.Modules.Bank.Model.Interfaces.IBank>(sql, parameters, type);
            return BankDetails;
        }

        public async Task<int> CreateBankDetails(Winit.Modules.Bank.Model.Interfaces.IBank bank)
        {
            string sql = @$"INSERT INTO bank (uid, company_uid, bank_name,bank_code, country_uid, cheque_fee, ss, created_time, 
                modified_time, server_add_time, server_modified_time)
                VALUES (@uid, @company_uid, @bank_name,@bank_code, @country_uid, @cheque_fee, @ss, @created_time, @modified_time, 
                @server_add_time, @server_modified_time);";

            Dictionary<string, object> parameters = new()
            {
                    {"uid", bank.UID},
                    {"company_uid", bank.CompanyUID},
                    {"bank_name", bank.BankName},
                    {"country_uid", bank.CountryUID},
                    {"cheque_fee", bank.ChequeFee},
                    {"ss", bank.SS},
                    {"created_time", bank.CreatedTime},
                    {"modified_time", bank.ModifiedTime},
                    {"server_add_time", bank.ServerAddTime},
                    {"server_modified_time", bank.ServerModifiedTime},
                    {"bank_code", bank.BankCode}
                };

            return await ExecuteNonQueryAsync(sql, parameters);

        }

        public async Task<int> UpdateBankDetails(Winit.Modules.Bank.Model.Interfaces.IBank bankDetails)
        {
            try
            {
                string sql = @"UPDATE bank 
                        SET 
                            company_uid = @company_uid, 
                            bank_name = @bank_name, 
                            bank_code = @bank_code, 
                            country_uid = @country_uid, 
                            cheque_fee = @cheque_fee, 
                            ss = @ss, 
                            modified_time = @modified_time, 
                            server_modified_time = @server_modified_time 
                        WHERE 
                            uid = @uid;
                        ";

                Dictionary<string, object> parameters = new()
                {
                    {"company_uid", bankDetails.CompanyUID},
                    {"bank_name", bankDetails.BankName},
                    {"bank_code", bankDetails.BankCode},
                    {"country_uid", bankDetails.CountryUID},
                    {"cheque_fee", bankDetails.ChequeFee},
                    {"ss", bankDetails.SS},
                    {"modified_time", bankDetails.ModifiedTime},
                    {"server_modified_time", bankDetails.ServerModifiedTime},
                    {"uid", bankDetails.UID}
                };

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteBankDetail(string UID)
        {
            Dictionary<string, object> parameters = new()
            {
                {"UID",  UID}
            };
            string sql = @"DELETE  FROM bank WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
