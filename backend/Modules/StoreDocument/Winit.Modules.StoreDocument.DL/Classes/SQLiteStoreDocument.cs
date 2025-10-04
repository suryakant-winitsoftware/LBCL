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
    public class SQLiteStoreDocument : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IStoreDocumentDL
    {
        public SQLiteStoreDocument(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider)
        {

        }
        public async Task<PagedResponse<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument>> SelectAllStoreDocumentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                id AS Id,
                                uid AS Uid,
                                created_by AS CreatedBy,
                                created_time AS CreatedTime,
                                modified_by AS ModifiedBy,
                                modified_time AS ModifiedTime,
                                server_add_time AS ServerAddTime,
                                server_modified_time AS ServerModifiedTime,
                                store_uid AS StoreUid,
                                document_type AS DocumentType,
                                document_no AS DocumentNo,
                                valid_from AS ValidFrom,
                                valid_up_to AS ValidUpTo
                            FROM 
                                store_document) As SubQuery");
                var sqlCount = new StringBuilder();
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
                                store_uid AS StoreUid,
                                document_type AS DocumentType,
                                document_no AS DocumentNo,
                                valid_from AS ValidFrom,
                                valid_up_to AS ValidUpTo
                            FROM 
                                store_document) As SubQuery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);

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
                    AppendSortCriteria(sortCriterias, sql);
                }

                //if (pageNumber > 0 && pageSize > 0)
                //{
                //    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}
                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreDocument>().GetType();
                IEnumerable<Model.Interfaces.IStoreDocument> storeDocuments = await ExecuteQueryAsync<Model.Interfaces.IStoreDocument>(sql.ToString(), parameters, type);
                //Count
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
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

           var sql = @"SELECT 
                    id AS Id,
                    uid AS Uid,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    store_uid AS StoreUid,
                    document_type AS DocumentType,
                    document_no AS DocumentNo,
                    valid_from AS ValidFrom,
                    valid_up_to AS ValidUpTo
                FROM 
                    public.store_document WHERE UID = @UID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreDocument>().GetType();
            Model.Interfaces.IStoreDocument StoreDocumentList = await ExecuteSingleAsync<Model.Interfaces.IStoreDocument>(sql, parameters, type);
            return StoreDocumentList;
        }

        public async Task<int> CreateStoreDocumentDetails(Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument createStoreDocument)
        {
            var sql = @"INSERT INTO StoreDocument (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, store_uid, document_type, document_no, valid_from, valid_up_to) " +
                     "VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @StoreUID, @DocumentType, @DocumentNo, @ValidFrom, @ValidUpTo);";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                {"UID", createStoreDocument.UID},
                {"CreatedTime", createStoreDocument.CreatedTime},
                {"ModifiedTime", createStoreDocument.ModifiedTime},
                {"ServerAddTime", createStoreDocument.ServerAddTime},
                {"ServerModifiedTime", createStoreDocument.ServerModifiedTime},
                {"CreatedBy", createStoreDocument.CreatedBy},
                {"ModifiedBy", createStoreDocument.ModifiedBy},
                {"StoreUID", createStoreDocument.StoreUID},
                {"DocumentType", createStoreDocument.DocumentType},
                {"DocumentNo", createStoreDocument.DocumentNo},
                {"ValidFrom", createStoreDocument.ValidFrom},
                {"ValidUpTo", createStoreDocument.ValidUpTo},               
             };
            return await ExecuteNonQueryAsync(sql, parameters);

        }

        public async Task<int> UpdateStoreDocumentDetails(Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument updateStoreDocument)
        {
            try
            {
                var sql = @"UPDATE store_document SET
                    created_by = @CreatedBy,                       
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,                       
                    server_modified_time = @ServerModifiedTime,
                    store_uid = @StoreUID,
                    document_type = @DocumentType,
                    document_no = @DocumentNo,
                    valid_from = @ValidFrom,
                    valid_up_to = @ValidUpTo
                  WHERE uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID", updateStoreDocument.UID},
                    {"CreatedBy", updateStoreDocument.CreatedBy},
                    {"ModifiedBy", updateStoreDocument.ModifiedBy},
                    {"ModifiedTime", updateStoreDocument.ModifiedTime},
                    {"ServerModifiedTime", updateStoreDocument.ServerModifiedTime},
                    {"StoreUID", updateStoreDocument.StoreUID},
                    {"DocumentType", updateStoreDocument.DocumentType},
                    {"DocumentNo", updateStoreDocument.DocumentNo},
                    {"ValidFrom", updateStoreDocument.ValidFrom},
                    {"ValidUpTo", updateStoreDocument.ValidUpTo}
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw ;
            }
        }
        public async Task<int> DeleteStoreDocumentDetails(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"DELETE  FROM store_document WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
