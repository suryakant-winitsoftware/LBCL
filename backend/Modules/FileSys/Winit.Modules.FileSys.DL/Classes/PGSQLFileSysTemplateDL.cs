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
    public  class PGSQLFileSysTemplateDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IFileSysTemplateDL
    {
        public PGSQLFileSysTemplateDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate>> SelectAllFileSysTemplateDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM(SELECT 
                                            fs.id AS Id, fs.uid AS UID, fs.created_by AS CreatedBy, fs.created_time AS CreatedTime,
                                            fs.modified_by AS ModifiedBy, fs.modified_time AS ModifiedTime, fs.server_add_time AS ServerAddTime,
                                            fs.server_modified_time AS ServerModifiedTime, fs.file_sys_type AS FileSysType,
                                            fs.folder AS Folder, fs.relative_path_format AS RelativePathFormat, fs.is_mobile AS IsMobile,
                                            fs.is_server AS IsServer FROM 
                                            file_sys_template fs)AS SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                            fs.id AS Id, fs.uid AS UID, fs.created_by AS CreatedBy, fs.created_time AS CreatedTime,
                                            fs.modified_by AS ModifiedBy, fs.modified_time AS ModifiedTime, fs.server_add_time AS ServerAddTime,
                                            fs.server_modified_time AS ServerModifiedTime, fs.file_sys_type AS FileSysType,
                                            fs.folder AS Folder, fs.relative_path_format AS RelativePathFormat, fs.is_mobile AS IsMobile,
                                            fs.is_server AS IsServer FROM 
                                            file_sys_template fs)AS SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate> fileSysTemplateDetails = await ExecuteQueryAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate> pagedResponse = new PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate>
                {
                    PagedData = fileSysTemplateDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
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
            var sql = @"SELECT 
                            fs.id AS Id, fs.uid AS UID, fs.created_by AS CreatedBy, fs.created_time AS CreatedTime,
                            fs.modified_by AS ModifiedBy, fs.modified_time AS ModifiedTime, fs.server_add_time AS ServerAddTime,
                            fs.server_modified_time AS ServerModifiedTime, fs.file_sys_type AS FileSysType,
                            fs.folder AS Folder, fs.relative_path_format AS RelativePathFormat, fs.is_mobile AS IsMobile,
                            fs.is_server AS IsServer
                        FROM 
                            file_sys_template fs WHERE uid = @UID";
            Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate? fileSysTemplateDetails = await ExecuteSingleAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate>(sql, parameters);
            return fileSysTemplateDetails;
        }
        public async Task<int> CreateFileSysTemplate(Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate createFileSysTemplate)
        {
            try
            {
                var sql = @"INSERT INTO file_sys_template (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, file_sys_type,  folder, relative_path_format, is_mobile, is_server) 
                            VALUES 
                            (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                            @FileSysType, @Folder, @RelativePathFormat, @IsMobile, @IsServer);";
                return await ExecuteNonQueryAsync(sql, createFileSysTemplate);
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
                var sql = @"UPDATE file_sys_template SET 
                            modified_by = @ModifiedBy,
                            modified_time = @ModifiedTime,
                            server_modified_time = @ServerModifiedTime,
                            file_sys_type = @FileSysType,
                            folder = @Folder,
                            relative_path_format = @RelativePathFormat,
                            is_mobile = @IsMobile,
                            is_server = @IsServer
                        WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, updateFileSysTemplate);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteFileSysTemplate(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
                var sql = @"DELETE  FROM file_sys_template WHERE uid = @UID";

                return await ExecuteNonQueryAsync(sql, parameters);

            }
            catch
            {
                throw;
            }
           
        }
    }
}
