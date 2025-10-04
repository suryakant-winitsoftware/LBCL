using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes
{
    public class MSSQLSkuSequenceDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISkuSequenceDL
    {
        public MSSQLSkuSequenceDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISkuSequence>> SelectAllSkuSequenceDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired,string SeqType)
        {
            try
            {
              
                var sql = new StringBuilder(@"Select * From (SELECT id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    bu_org_uid AS BUOrgUID,
                    franchisee_org_uid AS FranchiseeOrgUID,
                    seq_type AS SeqType,
                    sku_uid AS SKUUID,
                    serial_no AS SerialNo
                FROM sku_sequence
                WHERE seq_type = @SeqType)AS SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    bu_org_uid AS BUOrgUID,
                    franchisee_org_uid AS FranchiseeOrgUID,
                    seq_type AS SeqType,
                    sku_uid AS SKUUID,
                    serial_no AS SerialNo
                FROM sku_sequence
                WHERE seq_type = @SeqType)AS SubQuery");
                }
                var parameters = new Dictionary<string, object>()
                {
                    { "@SeqType",SeqType}
                };

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" Where ");
                    AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISkuSequence>(filterCriterias, sbFilterCriteria, parameters);

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

                IEnumerable<Model.Interfaces.ISkuSequence> SkuSequenceList = await ExecuteQueryAsync<Model.Interfaces.ISkuSequence>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISkuSequence> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISkuSequence>
                {
                    PagedData = SkuSequenceList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CUDSkuSequence( List<Winit.Modules.SKU.Model.Classes.SkuSequence> skuSequencesList)
        {
            int count = 0;

            if (skuSequencesList == null || skuSequencesList.Count == 0)
            {
                return count;
            }
            try
            {
                List<string> uidList = skuSequencesList.Select(po => po.UID).ToList();
                List<string> deletedUidList = skuSequencesList.Where(S => S.ActionType == ActionType.Delete).Select(S => S.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.SKUSequence, uidList);
                List<SkuSequence>? newSkuSequences=null;
                List<SkuSequence>? existSkuSequence=null;
                if (existingUIDs != null && existingUIDs.Count > 0 && skuSequencesList.Any(ssl => ssl.ActionType == ActionType.Add))  {
                    newSkuSequences=skuSequencesList.Where(ssl=>!existingUIDs.Contains(ssl.UID)).ToList();
                    existSkuSequence=skuSequencesList.Where(ssl => existingUIDs.Contains(ssl.UID)).ToList();
                }
                else
                {
                    newSkuSequences = skuSequencesList;
                }
                if (skuSequencesList.Any(ssl => ssl.ActionType == ActionType.Add))
                {
                    if (newSkuSequences != null && newSkuSequences.Any())
                    {
                        count += await CreateSkuSequence(newSkuSequences.Cast<ISkuSequence>().ToList());
                    }
                    if (existSkuSequence != null && existSkuSequence.Any())
                    {
                        count += await UpdateSkuSequence(existSkuSequence.Cast<ISkuSequence>().ToList());
                    }
                }
                if (skuSequencesList.Any(ssl => ssl.ActionType == ActionType.Delete))
                {
                    count += await DeleteSkuSequence(deletedUidList);
                }


            }
            catch
            {
                throw;
            }

            return count;
        }
        public async Task<IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISkuSequence>> SelectSkuSequenceByUID(List<string> UIDs)
        {
            try
            {
                var parameters = new { UIDS = UIDs };
                var sql = @"SELECT id AS Id,
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                bu_org_uid AS BUOrgUID,
                franchisee_org_uid AS FranchiseeOrgUID,
                seq_type AS SeqType,
                sku_uid AS SKUUID,
                serial_no AS SerialNo
            FROM sku_sequence
            WHERE uid IN @UIDS";
                return  await ExecuteQueryAsync<Winit.Modules.SKU.Model.Interfaces.ISkuSequence>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> UpdateSkuSequence(List<Winit.Modules.SKU.Model.Interfaces.ISkuSequence> skuSequenceList)
        {
            int count = -1;
            try
            {
            var Query = @"UPDATE sku_sequence
            SET modified_by = @ModifiedBy, 
                modified_time = @ModifiedTime, 
                server_modified_time = @ServerModifiedTime, 
                seq_type = @SeqType, 
                serial_no = @SerialNo
            WHERE uid = @UID;";

            count= await ExecuteNonQueryAsync(Query, skuSequenceList);
            }
            catch
            {
                throw;
            }
            return count;
        }
        private async Task<int> CreateSkuSequence(List<Winit.Modules.SKU.Model.Interfaces.ISkuSequence> skuSequenceList)
        {int count = -1;    
            try
            {
                var Query = @"INSERT INTO sku_sequence (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, bu_org_uid, franchisee_org_uid, seq_type, sku_uid, serial_no) VALUES (@UID, @CreatedBy, @CreatedTime,
                            @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @BUOrgUID, @FranchiseeOrgUID, @SeqType, @SKUUID, @SerialNo);";
               
                count= await ExecuteNonQueryAsync(Query, skuSequenceList);
            }
            catch (Exception Ex)
            {
                throw;
            }
            return count;
        }
        public async Task<int> CreateGeneralSKUSequenceForSKU(string BUOrgUID, string SKUUID)
        {
            try
            {
                var checkSql = "SELECT 1 FROM sku_sequence WHERE bu_org_uid = @BUOrgUID AND seq_type = 'General' AND sku_uid = @SKUUID;";

                var maxSerialNoSql = "SELECT COALESCE(MAX(serial_no), 0) FROM sku_sequence WHERE seq_type = 'General';";

                var insertSql = @"
                INSERT INTO sku_sequence (
                    uid, bu_org_uid, sku_uid, seq_type, serial_no, created_by, created_time, modified_by, modified_time
                ) VALUES (
                    @UID, @BUOrgUID, @SKUUID, 'General', @SerialNo, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime
                );";

                Dictionary<string, object> checkParameters = new Dictionary<string, object>
                {
                    {"BUOrgUID", BUOrgUID},
                    {"SKUUID", SKUUID}
                };

                var exists = await ExecuteScalarAsync<int?>(checkSql, checkParameters);

                if (exists == null)
                {
                    var maxSerialNo = (int)(await ExecuteScalarAsync<int>(maxSerialNoSql, null));
                    var newSerialNo = maxSerialNo + 1;

                    Dictionary<string, object> insertParameters = new Dictionary<string, object>
                    {
                        {"UID", Guid.NewGuid().ToString() },
                        {"BUOrgUID", BUOrgUID},
                        {"SKUUID", SKUUID},
                        {"SerialNo", newSerialNo},
                        {"CreatedBy", "ADMIN"}, 
                        {"CreatedTime", DateTime.UtcNow},
                        {"ModifiedBy", "ADMIN"}, 
                        {"ModifiedTime", DateTime.UtcNow}
                    };

                    return await ExecuteNonQueryAsync(insertSql, insertParameters);
                }

                return 0;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<int> DeleteSkuSequence(List<string> UIDs)

        {
            try
            {
                var Parameters = new { UIDS = UIDs };
                var sql = @"DELETE FROM sku_sequence WHERE uid IN @UIDS";
                return await ExecuteNonQueryAsync(sql, Parameters);
            }
            catch
            {
                throw;
            }
            
            
        }
    }


}
