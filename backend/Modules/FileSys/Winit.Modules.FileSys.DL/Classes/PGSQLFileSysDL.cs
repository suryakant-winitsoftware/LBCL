using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.FileSys.DL.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Common;


using Winit.Shared.Models.Enums;

namespace Winit.Modules.FileSys.DL.Classes
{
    public class PGSQLFileSysDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IFileSysDL
    {
        public PGSQLFileSysDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSys>> SelectAllFileSysDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"
                                                select * from (SELECT fs.id as ID, fs.uid AS Uid,
                                                       fs.created_by AS CreatedBy,
                                                       fs.created_time AS CreatedTime,
                                                       fs.modified_by AS ModifiedBy,
                                                       fs.modified_time AS ModifiedTime,
                                                       fs.server_add_time AS ServerAddTime,
                                                       fs.server_modified_time AS ServerModifiedTime,
                                                       fs.linked_item_type AS LinkedItemType,
                                                       fs.linked_item_uid AS LinkedItemUID,
                                                       fs.file_sys_type AS FileSysType,
                                                       fs.file_type AS FileType,
                                                       fs.parent_file_sys_uid AS ParentFileSysUid,
                                                       fs.is_directory AS IsDirectory,
                                                       fs.file_name AS FileName,
                                                       fs.display_name AS DisplayName,
                                                       fs.file_size AS FileSize,
                                                       fs.relative_path AS RelativePath,
                                                       fs.latitude AS Latitude,
                                                       fs.longitude AS Longitude,
                                                       fs.created_by_job_position_uid AS CreatedByJobPositionUid,
                                                       fs.created_by_emp_uid AS CreatedByEmpUid,
                                                       fs.is_default AS IsDefault,
                                                       fs.ss AS GetStoreUserActivityDetails
                                                   FROM file_sys fs)as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT fs.uid AS Uid,
                                                       fs.created_by AS CreatedBy,
                                                       fs.created_time AS CreatedTime,
                                                       fs.modified_by AS ModifiedBy,
                                                       fs.modified_time AS ModifiedTime,
                                                       fs.server_add_time AS ServerAddTime,
                                                       fs.server_modified_time AS ServerModifiedTime,
                                                       fs.linked_item_type AS LinkedItemType,
                                                       fs.linked_item_uid AS LinkedItemUID,
                                                       fs.file_sys_type AS FileSysType,
                                                       fs.file_type AS FileType,
                                                       fs.parent_file_sys_uid AS ParentFileSysUid,
                                                       fs.is_directory AS IsDirectory,
                                                       fs.file_name AS FileName,
                                                       fs.display_name AS DisplayName,
                                                       fs.file_size AS FileSize,
                                                       fs.relative_path AS RelativePath,
                                                       fs.latitude AS Latitude,
                                                       fs.longitude AS Longitude,
                                                       fs.created_by_job_position_uid AS CreatedByJobPositionUid,
                                                       fs.created_by_emp_uid AS CreatedByEmpUid,
                                                       fs.is_default AS IsDefault,
                                                       fs.ss AS SS
                                                   FROM file_sys fs) as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.FileSys.Model.Interfaces.IFileSys>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSysDetails = await ExecuteQueryAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSys>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSys> pagedResponse = new PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSys>
                {
                    PagedData = fileSysDetails,
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
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT fs.uid AS Uid,
                                                       fs.created_by AS CreatedBy,
                                                       fs.created_time AS CreatedTime,
                                                       fs.modified_by AS ModifiedBy,
                                                       fs.modified_time AS ModifiedTime,
                                                       fs.server_add_time AS ServerAddTime,
                                                       fs.server_modified_time AS ServerModifiedTime,
                                                       fs.linked_item_type AS LinkedItemType,
                                                       fs.linked_item_uid AS LinkedItemUID,
                                                       fs.file_sys_type AS FileSysType,
                                                       fs.file_type AS FileType,
                                                       fs.parent_file_sys_uid AS ParentFileSysUid,
                                                       fs.is_directory AS IsDirectory,
                                                       fs.file_name AS FileName,
                                                       fs.display_name AS DisplayName,
                                                       fs.file_size AS FileSize,
                                                       fs.relative_path AS RelativePath,
                                                       fs.latitude AS Latitude,
                                                       fs.longitude AS Longitude,
                                                       fs.created_by_job_position_uid AS CreatedByJobPositionUid,
                                                       fs.created_by_emp_uid AS CreatedByEmpUid,
                                                       fs.is_default AS IsDefault,
                                                       fs.ss AS SS
                                                   FROM file_sys fs WHERE fs.uid = @UID";
            Winit.Modules.FileSys.Model.Interfaces.IFileSys? FileSysDetails = await ExecuteSingleAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSys>(sql, parameters);
            return FileSysDetails;
        }
        public async Task<int> CreateFileSys(Winit.Modules.FileSys.Model.Interfaces.IFileSys createFileSys)
        {
            int retVal = -1;
            try
            {
                var sql = @"INSERT INTO file_sys (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, linked_item_type,
                            linked_item_uid, file_sys_type, file_type, parent_file_sys_uid, is_directory, file_name, display_name, file_size, 
                            relative_path, latitude, longitude, created_by_job_position_uid, created_by_emp_uid, is_default,ss)
                            VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @LinkedItemType,
                            @LinkedItemUID, @FileSysType, @FileType, @ParentFileSysUID, @IsDirectory, @FileName, @DisplayName, @FileSize, @RelativePath,
                            @Latitude, @Longitude,@CreatedByJobPositionUID, @CreatedByEmpUID, @IsDefault,@SS)";
                retVal = await ExecuteNonQueryAsync(sql, createFileSys);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        public async Task<int> UpdateFileSys(Winit.Modules.FileSys.Model.Interfaces.IFileSys updateFileSys)
        {
            int retVal = -1;
            try
            {
                var sql = @"UPDATE file_sys SET 
                            modified_by = @ModifiedBy,
                            modified_time = @ModifiedTime,
                            server_modified_time = @ServerModifiedTime,
                            linked_item_type = @LinkedItemType,
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
                            is_default = @IsDefault,
                            uid = @UID
                        WHERE linked_item_uid = @LinkedItemUID;";

                retVal = await ExecuteNonQueryAsync(sql, updateFileSys);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        public async Task<int> DeleteFileSys(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM file_sys WHERE uid = @UID";

            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<int> UpdateSKUImageIsDefault(List<SKUImage> sKUImageList)
        {
            int retVal = -1;
            try
            {
                if (sKUImageList != null && sKUImageList.Count > 0)
                {
                    var sql = @"UPDATE File_Sys SET 
                            Server_Modified_Time = @ServerModifiedTime,
                            Is_Default = @IsDefault
                            WHERE uid = @FileSysUID";
                    retVal = await ExecuteNonQueryAsync(sql, sKUImageList);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }

        public async Task<List<CommonUIDResponse>> CreateFileSysForBulk(List<Winit.Modules.FileSys.Model.Classes.FileSys> createFileSys)
        {
            if (createFileSys == null || createFileSys.Count == 0)
            {
                throw new ArgumentNullException(nameof(createFileSys), "The createFileSys list cannot be null or empty.");
            }
            try
            {
                int retVal = await InsertFileSysList(createFileSys);
                if (retVal < 1)
                {
                    throw new InvalidOperationException($"Failed to create file systems. Expected return value 1, but got {retVal}.");
                }

                List<string> uids = createFileSys.Select(cf => cf.UID).ToList();
                IEnumerable<IFileSys> existingFileSys = await SelectFileSysByUids(uids);

                return existingFileSys.Select(fs => new CommonUIDResponse
                {
                    UID = fs.UID,
                    Id = fs.Id
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating file systems for bulk operation.", ex);
            }
        }
        public async Task<bool> CreateFileSysForList(List<List<Winit.Modules.FileSys.Model.Classes.FileSys>> createFileSys)
        {
            if (createFileSys == null || createFileSys.Count == 0)
            {
                throw new ArgumentNullException(nameof(createFileSys), "The createFileSys list cannot be null or empty.");
            }
            try
            {
                bool IsSuccess = true;

                foreach (var createFileSysList in createFileSys)
                {
                    List<Winit.Modules.FileSys.Model.Classes.FileSys> fileSysList = createFileSysList.Where(x => x.Id < 0).ToList();
                    if (fileSysList.Count > 0)
                    {
                        int retVal = await InsertFileSysList(fileSysList);
                        if (retVal < 1)
                        {
                            IsSuccess = false;
                            throw new Exception($"Failed to create file systems. Expected return value 1, but got {retVal}.");
                        }
                    }
                }
                return IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating file systems for list operation.", ex);
            }
        }

        private async Task<int> InsertFileSysList(List<Winit.Modules.FileSys.Model.Classes.FileSys> fileSys)
        {
            int retVal = -1;
            try
            {
                var sql = @"INSERT INTO file_sys (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, linked_item_type,
                            linked_item_uid, file_sys_type, file_type, parent_file_sys_uid, is_directory, file_name, display_name, file_size, 
                            relative_path, latitude, longitude, created_by_job_position_uid, created_by_emp_uid, is_default,ss)
                            VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @LinkedItemType,
                            @LinkedItemUID, @FileSysType, @FileType, @ParentFileSysUID, @IsDirectory, @FileName, @DisplayName, @FileSize, @RelativePath,
                            @Latitude, @Longitude,@CreatedByJobPositionUID, @CreatedByEmpUID, @IsDefault,@SS)";
                retVal = await ExecuteNonQueryAsync(sql, fileSys);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        private async Task<IEnumerable<IFileSys>> SelectFileSysByUids(List<string> uids)
        {
            try
            {
                string commSeperatedUIDs = string.Join(",", uids);
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "UIDs", commSeperatedUIDs }
                };
                var sql = new StringBuilder(@"SELECT id as Id, uid as UID FROM file_sys WHERE uid In(@UIDs);");
                IEnumerable<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSysDetails = await ExecuteQueryAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSys>(sql.ToString(), parameters);
                return fileSysDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<int> CreateFileSysForBulk(List<IFileSys> FileSysList)
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

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"LinkedItemType",  LinkedItemType},
                {"FileSysType",  FileSysType},
                {"LinkedItemUIDs",  linkedItemUIDCommaSeparated}
            };

            StringBuilder sql = new StringBuilder();
            sql.Append(@"SELECT uid, linked_item_type, linked_item_uid, file_sys_type, file_type, file_name, display_name, relative_path, is_default
                            FROM file_sys 
                            WHERE linked_item_type = @LinkedItemType AND file_sys_type = @FileSysType ");

            if (!string.IsNullOrEmpty(linkedItemUIDCommaSeparated))
            {
                sql.Append($" AND LinkedItemUID In(@LinkedItemUIDs)");
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
                        linked_item_uid AS LinkedItemUID,
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

                Winit.Modules.FileSys.Model.Interfaces.IFileSys? fileSys = await ExecuteSingleAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSys>(fileSysSql.ToString(), Parameters);

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
                var existingRec = await SelecyFileSysByLinkedItemUID(fileSys.LinkedItemUID);

                count = (existingRec != null && existingRec.UID == fileSys.UID)
                    ? await UpdateFileSys(fileSys)
                    : await CreateFileSys(fileSys);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while processing FileSys operation.", ex);
            }

            return count;
        }


    }



}
