using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Settings.PostgreSQLSettingsRepository;

namespace WINITRepository.Classes.Settings
{
    public class SQLServerSettingsRepository : Interfaces.Settings.ISettingsRepository
    {
        private readonly string _connectionString;
        public SQLServerSettingsRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SqlServer");
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.Settings>> GetSettingDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.Settings> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Settings>(_connectionString);

                var sql = new StringBuilder("SELECT * FROM Setting");
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

                IEnumerable<WINITSharedObjects.Models.Settings> bankDetails = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);

                return bankDetails;
            }
            catch (Exception ex)
            {
                throw;
            }


        }


        public async Task<WINITSharedObjects.Models.Settings> GetSettingById(int  Id)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Settings> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Settings>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Id",  Id}
            };

            var sql = @"SELECT * FROM SETTING WHERE Id = @Id";

            WINITSharedObjects.Models.Settings SettingDetails = await dbManager.ExecuteSingleAsync(sql, parameters);
            return SettingDetails;
        }


        public async Task<WINITSharedObjects.Models.Settings> CreateSetting(WINITSharedObjects.Models.Settings createSetting)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Settings> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Settings>(_connectionString);
            try
            {
                var sql = "INSERT INTO Setting ([Type], [Name],  [Value],[DataType],[IsEditable],[SS],[CreatedTime],[ModifiedTime],[ServerAddTime],[ServerModifiedTime])" +
                              " VALUES (@Type, @Name, @Value,@DataType ,@IsEditable,@SS,@CreatedTime,@ModifiedTime,@ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"Type", createSetting.Type},
                   {"Name", createSetting.Name},
                   {"Value", createSetting.Value},
                   {"DataType", createSetting.DataType},
                   {"IsEditable", createSetting.IsEditable},
                   {"SS", createSetting.SS},
                   {"CreatedTime", createSetting.CreatedTime},
                   {"ModifiedTime", createSetting.ModifiedTime},
                   {"ServerAddTime", createSetting.ServerAddTime},
                   {"ServerModifiedTime", createSetting.ServerModifiedTime},

             };
                await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return createSetting;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<int> UpdateSetting(WINITSharedObjects.Models.Settings updateSetting)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.Settings> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Settings>(_connectionString);

                var sql = "UPDATE Setting SET [Type] = @Type, [Name] = @Name, [Value] = @Value," +
                    " [DataType] = @DataType,[IsEditable] = @IsEditable,[SS] = @SS,[ModifiedTime] = @ModifiedTime,[ServerModifiedTime] = @ServerModifiedTime WHERE [Id] = @Id;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                   {"Type", updateSetting.Type},
                   {"Name", updateSetting.Name},
                   {"Value", updateSetting.Value},
                   {"DataType", updateSetting.DataType},
                   {"IsEditable", updateSetting.IsEditable},
                   {"SS", updateSetting.SS},
                   {"ModifiedTime", updateSetting.ModifiedTime},
                   {"ServerModifiedTime", updateSetting.ServerModifiedTime},
                   {"Id", updateSetting.Id},
                 };
                var updateDetails = await dbManager.ExecuteNonQueryAsync(sql, parameters);

                return updateDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> DeleteSetting(int Id)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Settings> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Settings>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Id",Id}
            };
            var sql = "DELETE  FROM Setting WHERE Id = @Id";

            var Details = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return Details;
        }
    }
}
