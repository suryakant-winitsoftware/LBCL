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
    public class MSSQLSettingDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISettingDL
    {
        public MSSQLSettingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Setting.Model.Interfaces.ISetting>> SelectAllSettingDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"Select * from(SELECT s.id AS Id, s.uid AS UID, s.type AS Type, s.name AS Name, s.value AS Value, 
                                            s.data_type AS DataType, s.is_editable AS IsEditable, s.ss AS SS, s.created_time AS CreatedTime,
                                            s.modified_time AS ModifiedTime, s.server_add_time AS ServerAddTime, 
                                            s.server_modified_time AS ServerModifiedTime FROM setting s)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT s.id AS Id, s.uid AS UID, s.type AS Type, s.name AS Name, s.value AS Value, 
                                            s.data_type AS DataType, s.is_editable AS IsEditable, s.ss AS SS, s.created_time AS CreatedTime,
                                            s.modified_time AS ModifiedTime, s.server_add_time AS ServerAddTime, 
                                            s.server_modified_time AS ServerModifiedTime FROM setting s)as subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Setting.Model.Interfaces.ISetting>(filterCriterias, sbFilterCriteria, parameters);

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

                IEnumerable<Model.Interfaces.ISetting> settings = await ExecuteQueryAsync<Model.Interfaces.ISetting>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
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
        public async Task<Winit.Modules.Setting.Model.Interfaces.ISetting> GetSettingByUID(String UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT s.id AS Id, s.uid AS UID, s.type AS Type, s.name AS Name, s.value AS Value, s.data_type AS DataType,
                        s.is_editable AS IsEditable, s.ss AS SS, s.created_time AS CreatedTime, s.modified_time AS ModifiedTime,
                    s.server_add_time AS ServerAddTime, s.server_modified_time AS ServerModifiedTime FROM setting s WHERE s.uid = @UID";
            Winit.Modules.Setting.Model.Interfaces.ISetting SettingDetails = await ExecuteSingleAsync<Winit.Modules.Setting.Model.Interfaces.ISetting>(sql, parameters);
            return SettingDetails;
        }
        public async Task<int> CreateSetting(Winit.Modules.Setting.Model.Interfaces.ISetting createSetting)
        {
            try
            {
                var sql = @"INSERT INTO setting (uid, type, name, value, data_type, is_editable, ss, created_time, modified_time,
                            server_add_time, server_modified_time) VALUES (@UID, @Type, @Name, @Value, @DataType, @IsEditable,
                            @SS, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime)";
               
                return await ExecuteNonQueryAsync(sql, createSetting);
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
                var sql = @"UPDATE setting SET type = @Type, name = @Name, value = @Value, data_type = @DataType, is_editable = @IsEditable,
                            ss = @SS, modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, updateSetting);
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
            var sql = @"DELETE  FROM Setting WHERE UID = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
