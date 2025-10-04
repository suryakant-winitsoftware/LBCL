using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Nest;
using System.Linq;
using System.Text;
using System.Transactions;
using Winit.Modules.FileSys.DL.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.FileSys.DL.Classes;

public class SQLiteFileSysDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IFileSysDL
{
    public SQLiteFileSysDL(IServiceProvider serviceProvider) : base(serviceProvider)
    {

    }
    public async Task<PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSys>> SelectAllFileSysDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(@"SELECT * FROM (SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            linked_item_type AS LinkedItemType,
                            linked_item_uid AS LinkedItemUid,
                            file_sys_type AS FileSysType,
                            file_type AS FileType,
                            parent_file_sys_uid AS ParentFileSysUid,
                            is_directory AS IsDirectory,
                            file_name AS FileName,
                            display_name AS DisplayName,
                            file_size AS FileSize,
                            relative_path AS RelativePath,
                            latitude AS Latitude,
                            longitude AS Longitude,
                            created_by_job_position_uid AS CreatedByJobPositionUid,
                            created_by_emp_uid AS CreatedByEmpUid,
                            is_default AS IsDefault,
                            ss AS SS
                        FROM 
                            file_sys) as SubQuery");
            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            linked_item_type AS LinkedItemType,
                            linked_item_uid AS LinkedItemUid,
                            file_sys_type AS FileSysType,
                            file_type AS FileType,
                            parent_file_sys_uid AS ParentFileSysUid,
                            is_directory AS IsDirectory,
                            file_name AS FileName,
                            display_name AS DisplayName,
                            file_size AS FileSize,
                            relative_path AS RelativePath,
                            latitude AS Latitude,
                            longitude AS Longitude,
                            created_by_job_position_uid AS CreatedByJobPositionUid,
                            created_by_emp_uid AS CreatedByEmpUid,
                            is_default AS IsDefault,
                            ss AS SS
                        FROM 
                            file_sys) as SubQuery");
            }
            Dictionary<string, object?> parameters = new();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }
            //if (pageNumber > 0 && pageSize > 0)
            //{
            //    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            //}
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IFileSys>().GetType();
            IEnumerable<Winit.Modules.FileSys.Model.Interfaces.IFileSys> FileSysDetails = await ExecuteQueryAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSys>(sql.ToString(), parameters, type);
            int totalCount = -1;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSys> pagedResponse = new()
            {
                PagedData = FileSysDetails,
                TotalCount = totalCount
            };
            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<Winit.Modules.FileSys.Model.Interfaces.IFileSys> GetFileSysByUID(string UID)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"UID",  UID}
        };
        string sql = @"(SELECT 
                    id AS Id,
                    uid AS Uid,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    linked_item_type AS LinkedItemType,
                    linked_item_uid AS LinkedItemUid,
                    file_sys_type AS FileSysType,
                    file_type AS FileType,
                    parent_file_sys_uid AS ParentFileSysUid,
                    is_directory AS IsDirectory,
                    file_name AS FileName,
                    display_name AS DisplayName,
                    file_size AS FileSize,
                    relative_path AS RelativePath,
                    latitude AS Latitude,
                    longitude AS Longitude,
                    created_by_job_position_uid AS CreatedByJobPositionUid,
                    created_by_emp_uid AS CreatedByEmpUid,
                    is_default AS IsDefault,
                    ss AS SS
                FROM 
                    public.file_sys) as SubQuery WHERE UID = @UID";
        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IFileSys>().GetType();
        Winit.Modules.FileSys.Model.Interfaces.IFileSys FileSysDetails = await ExecuteSingleAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSys>(sql, parameters, type);
        return FileSysDetails;
    }
    public async Task<int> CreateFileSys(Winit.Modules.FileSys.Model.Interfaces.IFileSys createFileSys)
    {

        try
        {
            string sql = @"INSERT INTO file_sys ( uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, linked_item_type, 
                           linked_item_uid, file_sys_type, file_type, parent_file_sys_uid, is_directory,
                        file_name, display_name, file_size, relative_path, latitude, longitude, created_by_job_position_uid, created_by_emp_uid,ss) 
                        VALUES ( @UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime ,@ServerModifiedTime ,@LinkedItemType 
                         ,@LinkedItemUID ,@FileSysType ,@FileType ,@ParentFileSysUID ,@IsDirectory ,@FileName ,@DisplayName ,@FileSize ,@RelativePath ,@Latitude ,@Longitude 
                        ,@CreatedByJobPositionUID ,@CreatedByEmpUID,@SS)";
            Dictionary<string, object?> parameters = new()
            {
                {"UID",createFileSys.UID},
                {"CreatedBy",createFileSys.CreatedBy},
                {"CreatedTime",createFileSys.CreatedTime},
                {"ModifiedBy",createFileSys.ModifiedBy},
                {"ModifiedTime",createFileSys.ModifiedTime},
                {"ServerAddTime",createFileSys.ServerAddTime},
                {"ServerModifiedTime",createFileSys.ServerModifiedTime},
                {"LinkedItemType",createFileSys.LinkedItemType},
                {"LinkedItemUID",createFileSys.LinkedItemUID},
                {"FileSysType",createFileSys.FileSysType},
                {"FileType",createFileSys.FileType},
                {"ParentFileSysUID",createFileSys.ParentFileSysUID},
                {"IsDirectory",createFileSys.IsDirectory},
                {"FileName",createFileSys.FileName},
                {"DisplayName",createFileSys.DisplayName},
                {"FileSize",createFileSys.FileSize},
                {"RelativePath",createFileSys.RelativePath},
                {"Latitude",createFileSys.Latitude},
                {"Longitude",createFileSys.Longitude},
                {"CreatedByJobPositionUID",createFileSys.CreatedByJobPositionUID},
                {"CreatedByEmpUID",createFileSys.CreatedByEmpUID},
                {"SS",createFileSys.SS},
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> UpdateFileSys(Winit.Modules.FileSys.Model.Interfaces.IFileSys updateFileSys)
    {
        try
        {
            string sql = @"UPDATE file_sys SET 
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime,
                    linked_item_type = @LinkedItemType,
                    linked_item_uid = @LinkedItemUID,
                    file_sys_type = @FileSysType,
                    file_type = @FileType,
                    parent_file_sys_uid = @ParentFileSysUID,
                    is_directory = @IsDirectory,
                    file_name = @FileName,
                    display_name = @DisplayName,
                    file_size = @FileSize,
                    relative_path = @RelativePath,
                    latitude = @Latitude,
                    longitude = @Longitude,
                    created_by_job_position_uid = @CreatedByJobPositionUID,
                    created_by_emp_uid = @CreatedByEmpUID,
                    ss = @SS
                WHERE 
                    uid = @UID";

            Dictionary<string, object?> parameters = new()
            {
                {"UID",updateFileSys.UID},
                {"ModifiedBy",updateFileSys.ModifiedBy ?? ""},
                {"ModifiedTime",updateFileSys.ModifiedTime},
                {"ServerModifiedTime",updateFileSys.ServerModifiedTime},
                {"LinkedItemType",updateFileSys.LinkedItemType},
                {"LinkedItemUID",updateFileSys.LinkedItemUID},
                {"FileSysType",updateFileSys.FileSysType},
                {"FileType",updateFileSys.FileType},
                {"ParentFileSysUID",updateFileSys.ParentFileSysUID},
                {"IsDirectory",updateFileSys.IsDirectory},
                {"FileName",updateFileSys.FileName},
                {"DisplayName",updateFileSys.DisplayName},
                {"FileSize",updateFileSys.FileSize},
                {"RelativePath",updateFileSys.RelativePath},
                {"Latitude",updateFileSys.Latitude},
                {"Longitude",updateFileSys.Longitude},
                {"CreatedByJobPositionUID",updateFileSys.CreatedByJobPositionUID},
                {"CreatedByEmpUID",updateFileSys.CreatedByEmpUID},
                {"SS", updateFileSys.SS}
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public async Task<int> DeleteFileSys(string UID)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"UID" , UID}
        };
        string sql = @"DELETE  FROM FileSys WHERE UID = @UID";

        return await ExecuteNonQueryAsync(sql, parameters);
    }

    public async Task<int> UpdateSKUImageIsDefault(List<SKUImage> sKUImageList)
    {
        int retVal = -1;
        try
        {
            if (sKUImageList != null && sKUImageList.Count > 0)
            {
                foreach (SKUImage image in sKUImageList)
                {
                    string sql = @"UPDATE FileSys SET 
                            server_modified_time  = @ServerModifiedTime,
                            is_default = @IsDefault
                            WHERE uid = @FileSysUID";

                    Dictionary<string, object?> parameters = new()
                    {
                {"ServerModifiedTime", DateTime.Now},
                {"SKUUID", image.SKUUID},
                {"IsDefault", image.IsDefault},
                {"FileSysUID" , image.FileSysUID},
            };
                    retVal = await ExecuteNonQueryAsync(sql, parameters);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }

        return retVal;
    }

    public async Task<int> CreateFileSysForBulk(List<IFileSys> fileSysList)
    {
        if (fileSysList != null && fileSysList.Count > 0)
        {
            try
            {
                List<string> uidList = fileSysList.Select(po => po.UID).ToList();

                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.WHStockRequestLine, uidList);

                List<IFileSys>? newRecords = null;
                if (existingUIDs != null && existingUIDs.Count > 0)
                {
                    newRecords = fileSysList.Where(sol => !existingUIDs.Contains(sol.UID)).ToList();
                }
                else
                {
                    newRecords = fileSysList;
                }

                if(newRecords == null || newRecords.Count() == 0)
                {
                    return 0;
                }

                StringBuilder strQuery = new();
                string commandText = @"INSERT INTO File_Sys 
                        (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                        linked_item_type, linked_item_uid, file_sys_type, file_type, parent_file_sys_uid, is_directory, 
                        file_name, display_name, file_size, relative_path, latitude, longitude, created_by_job_position_uid
                        , created_by_emp_uid,ss)";
                _ = strQuery.Append(commandText);
                strQuery.Append(" Values ");
                Dictionary<string, object?> parameters = new();
                foreach (IFileSys fileSys in newRecords)
                {
                    int index = newRecords.IndexOf(fileSys);
                    parameters.Add($"UID_{index}", fileSys.UID);
                    parameters.Add($"ModifiedTime_{index}", fileSys.ModifiedTime);
                    parameters.Add($"ModifiedBy_{index}", fileSys.ModifiedBy);
                    parameters.Add($"ServerAddTime_{index}", fileSys.ServerAddTime);
                    parameters.Add($"ServerModifiedTime_{index}", fileSys.ServerModifiedTime);
                    parameters.Add($"CreatedBy_{index}", fileSys.CreatedBy);
                    parameters.Add($"CreatedTime_{index}", fileSys.CreatedTime);
                    parameters.Add($"LinkedItemType_{index}", fileSys.LinkedItemType);
                    parameters.Add($"LinkedItemUID_{index}", fileSys.LinkedItemUID);
                    parameters.Add($"FileSysType_{index}", fileSys.FileSysType);
                    parameters.Add($"FileType_{index}", fileSys.FileType);
                    parameters.Add($"ParentFileSysUID_{index}", fileSys.ParentFileSysUID);
                    parameters.Add($"IsDirectory_{index}", fileSys.IsDirectory);
                    parameters.Add($"FileName_{index}", fileSys.FileName);
                    parameters.Add($"DisplayName_{index}", fileSys.DisplayName);
                    parameters.Add($"FileSize_{index}", fileSys.FileSize);
                    parameters.Add($"RelativePath_{index}", fileSys.RelativePath);
                    parameters.Add($"Latitude_{index}", fileSys.Latitude);
                    parameters.Add($"Longitude_{index}", fileSys.Longitude);
                    parameters.Add($"CreatedByJobPositionUID_{index}", fileSys.CreatedByJobPositionUID);
                    parameters.Add($"CreatedByEmpUID_{index}", fileSys.CreatedByEmpUID);
                    parameters.Add($"ss_{index}", fileSys.SS);
                    strQuery.Append($@"(@UID_{index},@CreatedBy_{index},@CreatedTime_{index},@ModifiedBy_{index},
                        @ModifiedTime_{index},@ServerAddTime_{index},
                    @ServerModifiedTime_{index},@LinkedItemType_{index},
                    @LinkedItemUID_{index},@FileSysType_{index},@FileType_{index},@ParentFileSysUID_{index},
                    @IsDirectory_{index},@FileName_{index},@DisplayName_{index},@FileSize_{index},@RelativePath_{index},
                    @Latitude_{index},@Longitude_{index},@CreatedByJobPositionUID_{index},@CreatedByEmpUID_{index},
                     @ss_{index})");
                    if (newRecords.Last() != fileSys) strQuery.Append(", ");
                }
                return await ExecuteNonQueryAsync(strQuery.ToString(), parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        else return 0;
    }

    public Task<List<CommonUIDResponse>> CreateFileSysForBulk(List<Model.Classes.FileSys> createFileSys)
    {
        throw new NotImplementedException();
    }
    public async Task<List<IFileSys>?> GetFileSysByLinkedItemType(string LinkedItemType, string FileSysType, List<string>? LinkedItemUIDs = null)
    {
        string linkedItemUIDCommaSeparated = string.Empty;
        if (LinkedItemUIDs != null && LinkedItemUIDs.Count > 0)
        {
            linkedItemUIDCommaSeparated = string.Join("','", LinkedItemUIDs);
        }

        Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"LinkedItemType",  LinkedItemType},
                {"FileSysType",  FileSysType},
                {"LinkedItemUIDs",  linkedItemUIDCommaSeparated}
            };

        StringBuilder sql = new StringBuilder();
        sql.Append(@"SELECT uid AS Uid,linked_item_type AS LinkedItemType, linked_item_uid AS LinkedItemUID, file_sys_type AS FileSysType,file_type AS FileType, file_name AS FileName,display_name AS  DisplayName, relative_path AS RelativePath,is_default AS IsDefault 
                            FROM file_sys 
                            WHERE linked_item_type = @LinkedItemType AND file_sys_type = @FileSysType ");

        if (!string.IsNullOrEmpty(linkedItemUIDCommaSeparated))
        {
            sql.Append($" AND linked_item_uid IN (@LinkedItemUIDs)");
        }
        return await ExecuteQueryAsync<IFileSys>(sql.ToString(), parameters);
    }

    public async Task<Winit.Modules.FileSys.Model.Interfaces.IFileSys>
        SelecyFileSysByLinkedItemUID(string linkedItemUID)
    {
        try
        {
            Dictionary<string, object> Parameters = new Dictionary<string, object>
                {
                    { "UID", linkedItemUID },
                };
            var fileSysSql = new StringBuilder(@"SELECT 
                        id AS Id,
                        uid AS UID,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        linked_item_uid AS LinkedItemUid,
                        linked_item_type AS LinkedItemType,
                        file_sys_type AS FileSysType,
                        file_type AS FileType,
                        parent_file_sys_uid AS ParentFileSysUid,
                        is_directory AS IsDirectory,
                        file_name AS FileName,
                        display_name AS DisplayName,
                        file_size AS FileSize,
                        relative_path AS RelativePath,
                        --temp_path AS TempPath,
                        latitude AS Latitude,
                        longitude AS Longitude,
                        created_by_job_position_uid AS CreatedByJobPositionUid,
                        created_by_emp_uid AS CreatedByEmpUid,
                        is_default AS IsDefault
                FROM file_sys
                WHERE linked_item_uid = @UID;");

            Type fileSystype = _serviceProvider.GetRequiredService<Winit.Modules.FileSys.Model.Interfaces.IFileSys>().GetType();
            Winit.Modules.FileSys.Model.Interfaces.IFileSys fileSys = await ExecuteSingleAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSys>(fileSysSql.ToString(), Parameters, fileSystype);

            return fileSys;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> CUDFileSys(Winit.Modules.FileSys.Model.Interfaces.IFileSys fileSys)
    {
        int count = -1;
        try
        {
            bool exists = false;
            var existingRec = await GetFileSysByUID(fileSys.UID);

            if (existingRec != null)
            {
                if (exists = (existingRec.UID == fileSys.UID))
                    count = exists ? await UpdateFileSys(fileSys) : await CreateFileSys(fileSys);
            }
        }
        catch
        {
            throw;
        }

        return count;
    }

    Task<bool> IFileSysDL.CreateFileSysForList(List<List<Model.Classes.FileSys>> createFileSys)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Model.Interfaces.IFileSys>> GetPendingFileSyToUpload(string UID)
    {
        string query = """
                   SELECT * FROM file_sys WHERE SS = -1
                   """;

        if (!string.IsNullOrWhiteSpace(UID))
        {
            query += " AND linked_item_uid = @UID";
        }

        var parameters = new { UID };

        return await ExecuteQueryAsync<IFileSys>(query, parameters);
    }


    public async Task<bool> UpdateUploadStatus(string FileSysUID)
    {
        bool retValue = false;

        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            {"UID", FileSysUID}
        };

        string query = """
                     Update file_sys Set ss=1 where UID = @FileSysUID

                     """;
        int retVal = await ExecuteNonQueryAsync(query, parameters);
        return retVal > 1;
    }


}
