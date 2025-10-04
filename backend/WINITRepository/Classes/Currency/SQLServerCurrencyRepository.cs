using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Settings.PostgreSQLSettingsRepository;

namespace WINITRepository.Classes.Currency
{
    public class SQLServerCurrencyRepository : Interfaces.Currency.ICurrencyRepository
    {
        private readonly string _connectionString;
        public SQLServerCurrencyRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SqlServer");
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.Currency>> GetCurrencyDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.Currency> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Currency>(_connectionString);

                var sql = new StringBuilder("SELECT * FROM Currency");
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

                IEnumerable<WINITSharedObjects.Models.Currency> currencyDetails = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);

                return currencyDetails;
            }
            catch (Exception ex)
            {
                throw;
            }


        }


        public async Task<WINITSharedObjects.Models.Currency> GetCurrencyById(string UID  )   
            {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Currency> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Currency>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT * FROM Currency WHERE UID = @UID";

            WINITSharedObjects.Models.Currency currencyDetails = await dbManager.ExecuteSingleAsync(sql, parameters);
            return currencyDetails;
        }


        public async Task<WINITSharedObjects.Models.Currency> CreateCurrency(WINITSharedObjects.Models.Currency createCurrency)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Currency> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Currency>(_connectionString);
            try
            {
                var sql = "INSERT INTO Currency ([UID], [Name],  [Symbol],[Digits],[NumberCode],[FractionName],[SS],[CreatedTime],[ModifiedTime],[ServerAddTime],[ServerModifiedTime])" +
                              " VALUES (@UID, @Name, @Symbol,@Digits ,@NumberCode,@FractionName,@SS,@CreatedTime,@ModifiedTime,@ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"UID", createCurrency.UID},
                   {"Name", createCurrency.Name},
                   {"Symbol", createCurrency.Symbol},
                   {"Digits", createCurrency.Digits},
                   {"NumberCode", createCurrency.NumberCode},
                   {"FractionName", createCurrency.FractionName},
                   {"SS", createCurrency.SS},
                   {"CreatedTime", createCurrency.CreatedTime},
                   {"ModifiedTime", createCurrency.ModifiedTime},
                   {"ServerAddTime", createCurrency.ServerAddTime},
                   {"ServerModifiedTime", createCurrency.ServerModifiedTime},

             };
                await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return createCurrency;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<int> UpdateCurrency(WINITSharedObjects.Models.Currency updateCurrency)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.Currency> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Currency>(_connectionString);

                var sql = "UPDATE Currency SET [Name] = @Name, [Symbol] = @Symbol, [Digits] = @Digits," +
                    " [NumberCode] = @NumberCode,[FractionName] = @FractionName,[SS] = @SS,[ModifiedTime] = @ModifiedTime,[ServerModifiedTime] = @ServerModifiedTime WHERE [UID] = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                  {"Name", updateCurrency.Name},
                   {"Symbol", updateCurrency.Symbol},
                   {"Digits", updateCurrency.Digits},
                   {"NumberCode", updateCurrency.NumberCode},
                   {"FractionName", updateCurrency.FractionName},
                   {"SS", updateCurrency.SS},
                   {"ModifiedTime", updateCurrency.ModifiedTime},
                   {"ServerModifiedTime", updateCurrency.ServerModifiedTime},
                   {"UID", updateCurrency.UID},
                 };
                var updateDetails = await dbManager.ExecuteNonQueryAsync(sql, parameters);

                return updateDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> DeleteCurrency(string UID)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Currency> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Currency>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = "DELETE  FROM Currency WHERE UID = @UID";

            var Details = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return Details;
        }
    }
}
