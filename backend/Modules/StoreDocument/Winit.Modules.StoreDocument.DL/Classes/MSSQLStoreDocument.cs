using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.StoreDocument.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreDocument.DL.Classes
{
    public class MSSQLStoreDocument : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IStoreDocumentDL
    {
        public MSSQLStoreDocument(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument>> SelectAllStoreDocumentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (select id as Id,
                                               uid as UID,
                                               created_by as CreatedBy,
                                               created_time as CreatedTime,
                                               modified_by as ModifiedBy,
                                               modified_time as ModifiedTime,
                                               server_add_time as ServerAddTime,
                                               server_modified_time as ServerModifiedTime,
                                               store_uid as StoreUid,
                                               document_type as DocumentType,
                                               document_no as DocumentNo,
                                               valid_from as ValidFrom,
                                               valid_up_to as ValidUpTo
                                        from store_document) as sub_query
                                        ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select count(1) as Cnt from (select id as Id,
                                               uid as UID,
                                               created_by as CreatedBy,
                                               created_time as CreatedTime,
                                               modified_by as ModifiedBy,
                                               modified_time as ModifiedTime,
                                               server_add_time as ServerAddTime,
                                               server_modified_time as ServerModifiedTime,
                                               store_uid as StoreUid,
                                               document_type as DocumentType,
                                               document_no as DocumentNo,
                                               valid_from as ValidFrom,
                                               valid_up_to as ValidUpTo
                                        from store_document) as sub_query");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument>(filterCriterias, sbFilterCriteria, parameters);;

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
                IEnumerable<Model.Interfaces.IStoreDocument> storeDocuments = await ExecuteQueryAsync<Model.Interfaces.IStoreDocument>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument> pagedResponse = new PagedResponse<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument>
                {
                    PagedData = storeDocuments,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument> GetStoreDocumentDetailsByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"select id as Id,
                               uid as UID,
                               created_by as CreatedBy,
                               created_time as CreatedTime,
                               modified_by as ModifiedBy,
                               modified_time as ModifiedTime,
                               server_add_time as ServerAddTime,
                               server_modified_time as ServerModifiedTime,
                               store_uid as StoreUid,
                               document_type as DocumentType,
                               document_no as DocumentNo,
                               valid_from as ValidFrom,
                               valid_up_to as ValidUpTo
                        from store_document
                        where uid = @UID";
            Model.Interfaces.IStoreDocument? StoreDocumentList = await ExecuteSingleAsync<Model.Interfaces.IStoreDocument>(sql, parameters);
            return StoreDocumentList;
        }

        public async Task<int> CreateStoreDocumentDetails(Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument createStoreDocument)
        {
            var sql = @"insert into store_document (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                        store_uid, document_type, document_no, valid_from, valid_up_to)
                        values 
                        (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @StoreUID, 
                        @DocumentType, @DocumentNo, @ValidFrom, @ValidUpTo);";
            return await ExecuteNonQueryAsync(sql, createStoreDocument);

        }

        public async Task<int> UpdateStoreDocumentDetails(Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument updateStoreDocument)
        {
            try
            {
                var sql = @"UPDATE store_document
                            SET
                                created_by = @CreatedBy,
                                modified_by = @ModifiedBy,
                                modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime,
                                store_uid = @StoreUID,
                                document_type = @DocumentType,
                                document_no = @DocumentNo,
                                valid_from = @ValidFrom,
                                valid_up_to = @ValidUpTo
                            WHERE
                                uid = @UID;";
                
                return await ExecuteNonQueryAsync(sql, updateStoreDocument);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteStoreDocumentDetails(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"DELETE FROM store_document WHERE uid = @UID;";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
