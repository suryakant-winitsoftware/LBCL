using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.FileSys.DL.Interfaces;
using Winit.Shared.Models.Common;

using Winit.Shared.Models.Enums;

namespace Winit.Modules.FileSys.DL.Classes
{
    public  class SQLiteFileSysTemplateDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IFileSysTemplateDL
    {
        public SQLiteFileSysTemplateDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        public async Task<PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate>> SelectAllFileSysTemplateDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, file_sys_type AS FileSysType, folder AS Folder, relative_path_format AS RelativePathFormat, is_mobile AS IsMobile, is_server AS IsServer
FROM file_sys_template) As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, file_sys_type AS FileSysType, folder AS Folder, relative_path_format AS RelativePathFormat, is_mobile AS IsMobile, is_server AS IsServer
FROM file_sys_template) As SubQuery");
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
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IFileSysTemplate>().GetType();
                IEnumerable<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate> FileSysTemplateDetails = await ExecuteQueryAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate> pagedResponse = new PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate>
                {
                    PagedData = FileSysTemplateDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate> GetFileSysTemplateByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, file_sys_type AS FileSysType, folder AS Folder, relative_path_format AS RelativePathFormat, is_mobile AS IsMobile, is_server AS IsServer
            FROM file_sys_template WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IFileSysTemplate>().GetType();
            Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate FileSysTemplateDetails = await ExecuteSingleAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate>(sql, parameters, type);
            return FileSysTemplateDetails;
        }
        public async Task<int> CreateFileSysTemplate(Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate createFileSysTemplate)
        {
            try
            {
                var sql = @"INSERT INTO FileSysTemplate ( uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, file_sys_type, folder, relative_path_format, is_mobile, is_server) VALUES (@UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime ,@ServerModifiedTime ,
                            @FileSysType ,@Folder ,@RelativePathFormat ,@IsMobile ,@IsServer)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",createFileSysTemplate.UID},
                    {"CreatedBy",createFileSysTemplate.CreatedBy},
                    {"CreatedTime",createFileSysTemplate.CreatedTime},
                    {"ModifiedBy",createFileSysTemplate.ModifiedBy},
                    {"ModifiedTime",createFileSysTemplate.ModifiedTime},
                    {"ServerAddTime",createFileSysTemplate.ServerAddTime},
                    {"ServerModifiedTime",createFileSysTemplate.ServerModifiedTime},
                    {"FileSysType",createFileSysTemplate.FileSysType},
                    {"Folder",createFileSysTemplate.Folder},
                    {"RelativePathFormat",createFileSysTemplate.RelativePathFormat},
                    {"IsMobile",createFileSysTemplate.IsMobile},
                    {"IsServer",createFileSysTemplate.IsServer},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateFileSysTemplate(Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate updateFileSysTemplate)
        {
            try
            {
                var sql = @"UPDATE FileSysTemplate SET 
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        file_sys_type = @FileSysType,
                        folder = @Folder,
                        relative_path_format = @RelativePathFormat,
                        is_mobile = @IsMobile,
                        is_server = @IsServer
                    WHERE UID = @UID";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",updateFileSysTemplate.UID},
                    {"ModifiedBy",updateFileSysTemplate.ModifiedBy},
                    {"ModifiedTime",updateFileSysTemplate.ModifiedTime},
                    {"ServerModifiedTime",updateFileSysTemplate.ServerModifiedTime},
                    {"FileSysType",updateFileSysTemplate.FileSysType},
                    {"Folder",updateFileSysTemplate.Folder},
                    {"RelativePathFormat",updateFileSysTemplate.RelativePathFormat},
                    {"IsMobile",updateFileSysTemplate.IsMobile},
                    {"IsServer",updateFileSysTemplate.IsServer}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteFileSysTemplate(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM FileSysTemplate WHERE UID = @UID";

            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
