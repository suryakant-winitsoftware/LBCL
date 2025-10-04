using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Setting.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Setting.DL.Classes
{
    public class PGSQLSettingDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISettingDL
    {
        public PGSQLSettingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Setting.Model.Interfaces.ISetting>> SelectAllSettingDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (SELECT 
                    id AS Id, uid AS UID, type AS Type, 
                    name AS Name, value AS Value, 
                    data_type AS DataType,
                    is_editable AS IsEditable, ss AS SS, created_time AS CreatedTime, 
                    modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime FROM setting)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM(SELECT 
                    id AS Id, uid AS UID, type AS Type, 
                    name AS Name, value AS Value, 
                    data_type AS DataType,
                    is_editable AS IsEditable, ss AS SS, created_time AS CreatedTime, 
                    modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime FROM setting)as subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" where ");
                    AppendFilterCriteria<Winit.Modules.Setting.Model.Interfaces.ISetting>(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    // If count required then add filters to count
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql,true);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISetting>().GetType();
                IEnumerable<Model.Interfaces.ISetting> settings = await ExecuteQueryAsync<Model.Interfaces.ISetting>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Setting.Model.Interfaces.ISetting> pagedResponse = new PagedResponse<Winit.Modules.Setting.Model.Interfaces.ISetting>
                {
                    PagedData = settings,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Setting.Model.Interfaces.ISetting> GetSettingByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT 
                id AS Id,
                uid AS UID,
                ss AS SS,
                --created_by AS CreatedBy,
                created_time AS CreatedTime,
               -- modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                type AS Type,
                name AS Name,
                value AS Value,
                data_type AS DataType,
                is_editable AS IsEditable
            FROM setting
            WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISetting>().GetType();
            Winit.Modules.Setting.Model.Interfaces.ISetting SettingDetails = await ExecuteSingleAsync<Winit.Modules.Setting.Model.Interfaces.ISetting>(sql, parameters, type);
            return SettingDetails;
        }
        public async Task<int> CreateSetting(Winit.Modules.Setting.Model.Interfaces.ISetting createSetting)
        {
            try
            {
                var sql = @"INSERT INTO setting (uid,type,name,value,data_type,is_editable,ss,created_time,modified_time,server_add_time,server_modified_time) 
                               VALUES (@UID,@Type, @Name, @Value,@DataType ,@IsEditable,@SS,@CreatedTime,@ModifiedTime,@ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", createSetting.UID},
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
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSetting(Winit.Modules.Setting.Model.Interfaces.ISetting updateSetting)
        {
            try
            {
                var sql = @"UPDATE setting SET type = @Type, name = @Name, value = @Value,
                     data_type = @DataType,is_editable = @IsEditable,ss = @SS,modified_time = @ModifiedTime,server_modified_time = @ServerModifiedTime WHERE uid = @UID;";
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
                   {"UID", updateSetting.UID},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteSetting(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",UID}
            };
            var sql = @"DELETE  FROM setting WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
