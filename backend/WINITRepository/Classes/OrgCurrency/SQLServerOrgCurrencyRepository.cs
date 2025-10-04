using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Settings.PostgreSQLSettingsRepository;

namespace WINITRepository.Classes.OrgCurrency
{
    public class SQLServerOrgCurrencyRepository : Interfaces.OrgCurrency.IOrgCurrencyRepository
    {
        private readonly string _connectionString;
        public SQLServerOrgCurrencyRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SqlServer");
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.OrgCurrency>> GetOrgCurrencyDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency>(_connectionString);

                var sql = new StringBuilder("SELECT * FROM OrgCurrency");
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

                IEnumerable<WINITSharedObjects.Models.OrgCurrency> orgcurrencyDetails = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);

                return orgcurrencyDetails;
            }
            catch (Exception ex)
            {
                throw;
            }


        }


        public async Task<WINITSharedObjects.Models.OrgCurrency> GetOrgCurrencyByOrgUID(string orgUID  )   
            {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"orgUID",  orgUID}
            };

            var sql = @"SELECT * FROM OrgCurrency WHERE orgUID = @orgUID";

            WINITSharedObjects.Models.OrgCurrency orgcurrencyDetails = await dbManager.ExecuteSingleAsync(sql, parameters);
            return orgcurrencyDetails;
        }


        public async Task<WINITSharedObjects.Models.OrgCurrency> CreateOrgCurrency(WINITSharedObjects.Models.OrgCurrency orgcreateCurrency)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency>(_connectionString);
            try
            {
                var sql = "INSERT INTO OrgCurrency ([OrgUID], [CurrencyUID],  [IsPrimary],[SS],[CreatedTime],[ModifiedTime],[ServerAddTime],[ServerModifiedTime])" +
                              " VALUES (@OrgUID,  @CurrencyUID,@IsPrimary ,@SS,@CreatedTime,@ModifiedTime,@ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"OrgUID", orgcreateCurrency.OrgUID},
                   {"CurrencyUID", orgcreateCurrency.CurrencyUID},
                   {"IsPrimary", orgcreateCurrency.IsPrimary},
                   {"SS", orgcreateCurrency.SS},
                   {"CreatedTime", orgcreateCurrency.CreatedTime},
                   {"ModifiedTime", orgcreateCurrency.ModifiedTime},
                   {"ServerAddTime", orgcreateCurrency.ServerAddTime},
                   {"ServerModifiedTime", orgcreateCurrency.ServerModifiedTime},

             };
                await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return orgcreateCurrency;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<int> UpdateOrgCurrency(WINITSharedObjects.Models.OrgCurrency updateorgCurrency)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency>(_connectionString);

                var sql = "UPDATE OrgCurrency SET [CurrencyUID] = @CurrencyUID,  [IsPrimary] = @IsPrimary," +
                    " [SS] = @SS,[ModifiedTime] = @ModifiedTime,[ServerModifiedTime] = @ServerModifiedTime WHERE [OrgUID] = @OrgUID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                  {"OrgUID", updateorgCurrency.OrgUID},
                   {"CurrencyUID", updateorgCurrency.CurrencyUID},
                   {"Digits", updateorgCurrency.IsPrimary},
                   {"IsPrimary", updateorgCurrency.IsPrimary},
                   {"SS", updateorgCurrency.SS},
                   {"ModifiedTime", updateorgCurrency.ModifiedTime},
                   {"ServerModifiedTime", updateorgCurrency.ServerModifiedTime},
                 };
                var updateDetails = await dbManager.ExecuteNonQueryAsync(sql, parameters);

                return updateDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> DeleteOrgCurrency(string orgUID)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"orgUID" , orgUID}
            };
            var sql = "DELETE  FROM OrgCurrency WHERE orgUID = @orgUID";

            var Details = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return Details;
        }
    }
}
