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
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes
{
    public class PGSQLSkuSequenceDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISkuSequenceDL
    {
        public PGSQLSkuSequenceDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISkuSequence>> SelectAllSkuSequenceDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired,string SeqType)
        {
            try
            {

                var sql = new StringBuilder(@"SELECT
                    ss.id AS Id,
                    ss.uid AS UID,
                    ss.created_by AS CreatedBy,
                    ss.created_time AS CreatedTime,
                    ss.modified_by AS ModifiedBy,
                    ss.modified_time AS ModifiedTime,
                    ss.server_add_time AS ServerAddTime,
                    ss.server_modified_time AS ServerModifiedTime,
                    ss.bu_org_uid AS BUOrgUID,
                    ss.franchisee_org_uid AS FranchiseeOrgUID,
                    ss.seq_type AS SeqType,
                    ss.sku_uid AS SKUUID,
                    ss.serial_no AS SerialNo,
                    s.code AS SKUCode,
                    s.long_name AS SKUName
                FROM sku_sequence ss
                LEFT JOIN sku s ON ss.sku_uid = s.uid
                WHERE ss.seq_type = @SeqType");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM sku_sequence WHERE seq_type =@SeqType");
                }
                var parameters = new Dictionary<string, object>()
                {
                    { "@SeqType",SeqType}
                };

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" AND ");
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
                    sql.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISkuSequence>().GetType();
                IEnumerable<Model.Interfaces.ISkuSequence> SkuSequenceList = await ExecuteQueryAsync<Model.Interfaces.ISkuSequence>(sql.ToString(), parameters, type);
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
            List<string> uidList = skuSequencesList.Select(po => po.UID).ToList();
            List<string> deletedUidList = skuSequencesList.Where(S => S.ActionType == ActionType.Delete).Select(S => S.UID).ToList();

            try
            {
                IEnumerable<ISkuSequence> existingRec = await SelectSkuSequenceByUID(uidList);

                foreach (SkuSequence skuSequence in skuSequencesList)
                {
                    switch (skuSequence.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            bool exists = existingRec.Any(po => po.UID == skuSequence.UID);
                            count += exists ?
                                await UpdateSkuSequence(skuSequence) :
                                await CreateSkuSequence(skuSequence);
                            break;

                        case Shared.Models.Enums.ActionType.Delete:
                            count += await DeleteSkuSequence(deletedUidList);
                            break;
                    }
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
            string commaSeperatedUIDs = string.Join(",", UIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UIDs",  commaSeperatedUIDs}
            };
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
            WHERE uid = ANY(string_to_array(@UIDs, ','));";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.SKU.Model.Interfaces.ISkuSequence>().GetType();
            IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISkuSequence> SkuSequenceDetails = await ExecuteQueryAsync<Winit.Modules.SKU.Model.Interfaces.ISkuSequence>(sql, parameters, type);
            return SkuSequenceDetails;
        }
        private async Task<int> UpdateSkuSequence(Winit.Modules.SKU.Model.Interfaces.ISkuSequence skuSequence)
        {
            var Query = @"UPDATE sku_sequence
            SET modified_by = @ModifiedBy,
                modified_time = @ModifiedTime,
                server_modified_time = @ServerModifiedTime,
                seq_type = @SeqType,
                serial_no = @SerialNo
            WHERE uid = @UID;";
            var Parameters = new Dictionary<string, object>
                        {
                          { "@UID", skuSequence.UID },
                          { "@CreatedTime", skuSequence.CreatedTime },
                          { "@ModifiedBy", skuSequence.ModifiedBy },
                          { "@ModifiedTime", skuSequence.ModifiedTime },
                          { "@ServerModifiedTime", skuSequence.ServerModifiedTime },
                          { "@SeqType", skuSequence.SeqType },
                          { "@SerialNo", skuSequence.SerialNo },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
        private async Task<int> CreateSkuSequence(Winit.Modules.SKU.Model.Interfaces.ISkuSequence skuSequence)
        {
            try
            {
                var Query = @"INSERT INTO sku_sequence (uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, bu_org_uid, franchisee_org_uid, seq_type, sku_uid, serial_no) VALUES (@UID, @CreatedBy, @CreatedTime,
                            @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @BUOrgUID, @FranchiseeOrgUID, @SeqType, @SKUUID, @SerialNo);";
                var Parameters = new Dictionary<string, object>
                        {
                          { "@UID", skuSequence.UID },
                          { "@CreatedBy", skuSequence.CreatedBy },
                          { "@CreatedTime", skuSequence.CreatedTime },
                          { "@ModifiedBy", skuSequence.ModifiedBy },
                          { "@ModifiedTime", skuSequence.ModifiedTime },
                          { "@ServerAddTime", skuSequence.ServerAddTime },
                          { "@ServerModifiedTime", skuSequence.ServerModifiedTime },
                          { "@BUOrgUID", skuSequence.BUOrgUID },
                          { "@FranchiseeOrgUID", skuSequence.FranchiseeOrgUID },
                          { "@SeqType", skuSequence.SeqType },
                          { "@SKUUID", skuSequence.SKUUID },
                          { "@SerialNo", skuSequence.SerialNo },
            };
                return await ExecuteNonQueryAsync(Query, Parameters);
            }
            catch (Exception Ex)
            {
                throw;
            }
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
             string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM sku_sequence WHERE uid = ANY(string_to_array(@UID, ','));";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UID", commaSeperatedUIDs },
                        };
            return await ExecuteNonQueryAsync(sql,Parameters);
        }
    }


}
