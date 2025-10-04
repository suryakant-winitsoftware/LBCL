using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ListHeader.DL.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ListHeader.DL.Classes
{
    public class SQLiteListHeaderDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IListHeaderDL
    {
        public SQLiteListHeaderDL(IServiceProvider serviceProvider ) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListHeader>> GetListHeaders(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT UID, Name,Code FROM (SELECT 
                                id AS Id,
                                uid AS Uid,
                                created_by AS CreatedBy,
                                created_time AS CreatedTime,
                                modified_by AS ModifiedBy,
                                modified_time AS ModifiedTime,
                                server_add_time AS ServerAddTime,
                                server_modified_time AS ServerModifiedTime,
                                company_uid AS CompanyUid,
                                org_uid AS OrgUid,
                                code AS Code,
                                name AS Name,
                                is_editable AS IsEditable,
                                is_visible_in_ui AS IsVisibleInUi
                            FROM 
                                list_header) AS SubQuery");
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
                                company_uid AS CompanyUid,
                                org_uid AS OrgUid,
                                code AS Code,
                                name AS Name,
                                is_editable AS IsEditable,
                                is_visible_in_ui AS IsVisibleInUi
                            FROM 
                                list_header) AS SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sql.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sql, parameters);
                    sql.Append(sbFilterCriteria);
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

                Type type = _serviceProvider.GetRequiredService<Winit.Modules.ListHeader.Model.Interfaces.IListHeader>().GetType();

                IEnumerable<Winit.Modules.ListHeader.Model.Interfaces.IListHeader> listHeaderDetails = await ExecuteQueryAsync<Winit.Modules.ListHeader.Model.Interfaces.IListHeader>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListHeader> pagedResponse = new PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListHeader>
                {
                    PagedData = listHeaderDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetListItemsByCodes(List<string> Codes, bool isCountRequired)
        {
        

            Codes = Codes.Select(code => code.Trim()).ToList();
            string commaSeparatedCodes = string.Join("','", Codes);
            //string[] codeArray = Codes.Select(code => $"'{code}'").ToArray();
            //string codeFilter = string.Join(",", codeArray);

            

            StringBuilder sql = new StringBuilder(@"SELECT LI.uid As UId, LI.code As Code, LI.name AS Name, LI.serial_no As SerialNo,  LI.is_editable As IsEditable, LI.list_header_uid AS ListHeaderUID
                            FROM list_item LI
                            INNER JOIN list_header LH ON LH.uid = LI.list_header_uid WHERE 1=1");
            if(Codes!=null && Codes.Any())
            {
                sql.Append($" AND LH.code IN ({string.Join(",", Codes.Select((_, i) => $"@Code{i}"))});");
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            for (int i = 0; i < Codes.Count; i++)
            {
                parameters.Add($"@Code{i}", Codes[i]);
            }

            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM  list_item LI
                      INNER JOIN list_header LH ON LH.uid = LI.list_header_uid WHERE 1=1");
                if (Codes != null && Codes.Any())
                {
                    sqlCount.Append($" AND LH.Code IN ({string.Join(",", Codes.Select((_, i) => $"@Code{i}"))});");
                }
            }
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.ListHeader.Model.Interfaces.IListItem>().GetType();

            IEnumerable<Winit.Modules.ListHeader.Model.Interfaces.IListItem> listItemList = await ExecuteQueryAsync<Winit.Modules.ListHeader.Model.Interfaces.IListItem>(sql.ToString(), parameters, type);
            int totalCount = -1;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem> pagedResponse = new PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>
            {
                PagedData = listItemList,
                TotalCount = totalCount
            };
            return pagedResponse;
        }


        public async Task<int> CreateListItem(Winit.Modules.ListHeader.Model.Interfaces.IListItem listItem)
        {
            try
            {
                 
                var sql = @"INSERT INTO list_item (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, code, name, is_editable , serial_no , list_header_uid )VALUES(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,
                            @ServerModifiedTime,@Code,@Name,@IsEditable,@SerialNo,@ListHeaderUID);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",listItem.UID},
                   {"CreatedBy",listItem.CreatedBy},
                   {"CreatedTime",listItem.CreatedTime},
                   {"ModifiedBy",listItem.ModifiedBy},
                   {"ModifiedTime",listItem.ModifiedTime},
                   {"ServerAddTime",listItem.ServerAddTime},
                   {"ServerModifiedTime",listItem.ServerModifiedTime},
                   {"Name",listItem.Name},
                   {"Code",listItem.Code},
                   {"IsEditable",listItem.IsEditable},
                   {"SerialNo",listItem.SerialNo},
                   {"ListHeaderUID",listItem.ListHeaderUID},

                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<int> UpdateListItem(Winit.Modules.ListHeader.Model.Interfaces.IListItem listItem)
        {
            try
            {
                    var sql = @"UPDATE list_item SET 
                 modified_by = @ModifiedBy,
                 modified_time = @ModifiedTime,
                 server_modified_time = @ServerModifiedTime,
                 is_editable = @IsEditable,
                 serial_no = @SerialNo,
                 name = @Name
                 WHERE uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",listItem.UID},
                    {"ModifiedBy",listItem.ModifiedBy},
                    {"ModifiedTime",listItem.ModifiedTime},
                    {"ServerModifiedTime",listItem.ServerModifiedTime},
                    {"IsEditable",listItem.IsEditable},
                    {"SerialNo",listItem.SerialNo},
                    {"Name",listItem.Name},

                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteListItemByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM list_item WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IEnumerable<IListItem>> GetListItemsByHeaderUID(string headerUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"headerUID",  headerUID} 
            };
            
                  var sql = @"
                    SELECT  uid As UID, code As Code, name As Name, serial_no As  SerialNo, is_editable As IsEditable,list_header_uid As ListHeaderUID FROM list_item
                    WHERE list_header_uid = @headerUID";

            Type type = _serviceProvider.GetRequiredService<IListItem>().GetType();
            IEnumerable<IListItem> ListItem = await ExecuteQueryAsync<IListItem>(sql, parameters, type);
            return ListItem;
        }
        public async Task<IListItem> GetListItemsByUID(string UID)
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
                code AS Code,
                name AS Name,
                is_editable AS IsEditable,
                serial_no AS SerialNo,
                list_header_uid AS ListHeaderUid
            FROM 
                list_item  WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IListItem>().GetType();
            Winit.Modules.ListHeader.Model.Interfaces.IListItem listItem = await ExecuteSingleAsync<Winit.Modules.ListHeader.Model.Interfaces.IListItem>(sql, parameters, type);
            return listItem;
        }
        public async Task<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetListItemsByListHeaderCodes(
    List<string> codes, List<SortCriteria> sortCriterias, int pageNumber, int pageSize,
    List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
           throw new NotImplementedException();
        }




    }
}
