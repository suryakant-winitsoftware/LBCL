using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using WINITRepository.Interfaces.Customers;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;

namespace WINITRepository.Classes.Bank
{
    public class PostgreSQLBankRepository : Interfaces.Bank.IBankRepository
    {
        private readonly string _connectionString;
        public PostgreSQLBankRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("PostgreSQL");
        }
        public async Task<IEnumerable<WINITSharedObjects.Models.Bank>> GetBankDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.Bank> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Bank>(_connectionString);

                var sql = new StringBuilder("SELECT * FROM Bank");
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    sql.Append(" WHERE ");
                    dbManager.AppendFilterCriteria(filterCriterias, sql, parameters);
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    dbManager.AppendSortCriteria(sortCriterias, sql);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                IEnumerable<WINITSharedObjects.Models.Bank> bankDetails = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);

                return bankDetails;
            }
            catch (Exception ex)
            {
                throw;
            }


        }


        public async Task<WINITSharedObjects.Models.Bank> GetBankDetailsByUID(string UID)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Bank> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Bank>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT * FROM BANK WHERE UID = @UID";

            WINITSharedObjects.Models.Bank BankDetails = await dbManager.ExecuteSingleAsync(sql, parameters);
            return BankDetails;
        }

        public async Task<WINITSharedObjects.Models.Bank> CreateBankDetails(WINITSharedObjects.Models.Bank bank)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Bank> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Bank>(_connectionString);

            var sql = "INSERT INTO Bank ([UID], [CompanyUID],  [BankName],[CountryUID],[ChequeFee],[SS],[CreatedTime],[ModifiedTime],[ServerAddTime],[ServerModifiedTime])" +
                " VALUES (@UID, @CompanyUID, @BankName,@CountryUID, @ChequeFee,@SS,@CreatedTime,@ModifiedTime,@ServerAddTime,@ServerModifiedTime);";
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
            await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return bank;
        }

        public async Task<int> UpdateBankDetails(WINITSharedObjects.Models.Bank bankDetails)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.Bank> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Bank>(_connectionString);

                var sql = "UPDATE Bank SET [CompanyUID] = @CompanyUID, [BankName] = @BankName, [CountryUID] = @CountryUID," +
                    " [ChequeFee] = @ChequeFee,[SS] = @SS,[ModifiedTime] = @ModifiedTime,[ServerModifiedTime] = @ServerModifiedTime WHERE [UID] = @UID;";
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
                var updateProducts = await dbManager.ExecuteNonQueryAsync(sql, parameters);

                return updateProducts;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<int> DeleteBankDetail(string UID)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Bank> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Bank>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = "DELETE  FROM Bank WHERE UID = @UID";

            var BankDetails = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return BankDetails;
        }

    }
}








