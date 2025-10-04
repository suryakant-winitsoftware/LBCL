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
    public class MSSQLListHeaderDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IListHeaderDL
    {
        public MSSQLListHeaderDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListHeader>> GetListHeaders(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (select 
                                                    uid as UID,
                                                    created_by as CreatedBy,
                                                    created_time as CreatedTime,
                                                    modified_by as ModifiedBy,
                                                    modified_time as ModifiedTime,
                                                    server_add_time as ServerAddTime,
                                                    server_modified_time as ServerModifiedTime,
                                                    company_uid as CompanyUID,
                                                    org_uid as OrgUID,
                                                    code as Code,
                                                    name as Name,
                                                    is_editable as IsEditable,
                                                    is_visible_in_ui as IsVisibleInUI
                                                from 
                                                    list_header where is_visible_in_ui = 1)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (select 
                                                    uid as UID,
                                                    created_by as CreatedBy,
                                                    created_time as CreatedTime,
                                                    modified_by as ModifiedBy,
                                                    modified_time as ModifiedTime,
                                                    server_add_time as ServerAddTime,
                                                    server_modified_time as ServerModifiedTime,
                                                    company_uid as CompanyUID,
                                                    org_uid as OrgUID,
                                                    code as Code,
                                                    name as Name,
                                                    is_editable as IsEditable,
                                                    is_visible_in_ui as IsVisibleInUI
                                                from 
                                                    list_header where is_visible_in_ui=1)as subquery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.ListHeader.Model.Interfaces.IListHeader>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.ListHeader.Model.Interfaces.IListHeader> listHeaderDetails = await ExecuteQueryAsync<Winit.Modules.ListHeader.Model.Interfaces.IListHeader>(sql.ToString(), parameters);
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
        public async Task<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetListItemsByCodes(List<string> codes, bool isCountRequired)
        {
            try
            {
                var parameters = new { Codes = codes };
                var sql = new StringBuilder(@"SELECT * FROM(
                                        SELECT 
                                            li.uid AS UID,
                                            li.created_by AS CreatedBy,
                                            li.created_time AS CreatedTime,
                                            li.modified_by AS ModifiedBy,
                                            li.modified_time AS ModifiedTime,
                                            li.server_add_time AS ServerAddTime,
                                            li.server_modified_time AS ServerModifiedTime,
                                            li.code AS Code,
                                            li.name AS Name,
                                            li.serial_no AS SerialNo,
                                            li.is_editable AS IsEditable,
                                            li.list_header_uid AS ListHeaderUID
                                        FROM 
                                            list_item li
                                            INNER JOIN list_header lh ON lh.uid = li.list_header_uid
                                        WHERE 
                                            lh.code In @Codes)As SubQUery ORDER BY SerialNo DESC");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"
                                            SELECT COUNT(1) AS Cnt 
                                            FROM (SELECT 
                                            li.uid AS UID,
                                            li.created_by AS CreatedBy,
                                            li.created_time AS CreatedTime,
                                            li.modified_by AS ModifiedBy,
                                            li.modified_time AS ModifiedTime,
                                            li.server_add_time AS ServerAddTime,
                                            li.server_modified_time AS ServerModifiedTime,
                                            li.code AS Code,
                                            li.name AS Name,
                                            li.serial_no AS SerialNo,
                                            li.is_editable AS IsEditable,
                                            li.list_header_uid AS ListHeaderUID
                                        FROM 
                                            list_item li
                                            INNER JOIN list_header lh ON lh.uid = li.list_header_uid
                                        WHERE 
                                            lh.code IN @Codes)As SubQUery");
                }

                IEnumerable<Winit.Modules.ListHeader.Model.Interfaces.IListItem> listItemList = await ExecuteQueryAsync<Winit.Modules.ListHeader.Model.Interfaces.IListItem>(sql.ToString(), parameters);
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
            catch (Exception)
            {
                throw;
            }
        }



        public async Task<int> CreateListItem(Winit.Modules.ListHeader.Model.Interfaces.IListItem listItem)
        {
            try
            {
                var sql = @"insert into list_item (uid,created_by,created_time,modified_by,modified_time,server_add_time,server_modified_time,
                       code,name,is_editable,serial_no,list_header_uid)
                        values (@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,
                                @ServerModifiedTime,@Code,@Name,@IsEditable,@SerialNo,@ListHeaderUID);";
                return await ExecuteNonQueryAsync(sql, listItem);
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
                var sql = @"update list_item set 
                                modified_by = @ModifiedBy,
                                modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime,
                                is_editable = @IsEditable,
                                serial_no = @SerialNo,
                                name = @Name,
                                code = @Code
                            where 
                                uid = @UID;";
                return await ExecuteNonQueryAsync(sql, listItem);
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
            var sql = @"delete from list_item where uid = @UID;";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<IListItem> GetListItemsByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"select 
                                                    li.uid as UID,
                                                    li.created_by as CreatedBy,
                                                    li.created_time as CreatedTime,
                                                    li.modified_by as ModifiedBy,
                                                    li.modified_time as ModifiedTime,
                                                    li.server_add_time as ServerAddTime,
                                                    li.server_modified_time as ServerModifiedTime,
                                                    li.code as Code,
                                                    li.name as Name,
                                                    li.serial_no as SerialNo,
                                                    li.is_editable as IsEditable,
                                                    li.list_header_uid as ListHeaderUID
                                                from 
                                                    list_item li WHERE li.UID = @UID";
            Winit.Modules.ListHeader.Model.Interfaces.IListItem listItem = await ExecuteSingleAsync<Winit.Modules.ListHeader.Model.Interfaces.IListItem>(sql, parameters);
            return listItem;
        }

        public async Task<IEnumerable<IListItem>> GetListItemsByHeaderUID(string headerUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  headerUID}
            };
            var sql = @"select 
                                                    li.uid as UID,
                                                    li.created_by as CreatedBy,
                                                    li.created_time as CreatedTime,
                                                    li.modified_by as ModifiedBy,
                                                    li.modified_time as ModifiedTime,
                                                    li.server_add_time as ServerAddTime,
                                                    li.server_modified_time as ServerModifiedTime,
                                                    li.code as Code,
                                                    li.name as Name,
                                                    li.serial_no as SerialNo,
                                                    li.is_editable as IsEditable,
                                                    li.list_header_uid as ListHeaderUID
                                                from 
                                                    list_item li WHERE li.list_header_uid = @UID";
            IEnumerable<Winit.Modules.ListHeader.Model.Interfaces.IListItem> listItemList = await ExecuteQueryAsync<Winit.Modules.ListHeader.Model.Interfaces.IListItem>(sql, parameters);
            return listItemList;
        }
        public async Task<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetListItemsByListHeaderCodes(
     List<string> codes, List<SortCriteria> sortCriterias, int pageNumber, int pageSize,
     List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>()
                {
                    {
                        "Codes",codes
                    }
                };

               // parameters.Add("Codes", codes);

                var sql = new StringBuilder(@"
            SELECT * FROM (
                SELECT 
                    li.uid AS UID,
                    li.created_by AS CreatedBy,
                    li.created_time AS CreatedTime,
                    li.modified_by AS ModifiedBy,
                    li.modified_time AS ModifiedTime,
                    li.server_add_time AS ServerAddTime,
                    li.server_modified_time AS ServerModifiedTime,
                    li.code AS Code,
                    li.name AS Name,
                    li.serial_no AS SerialNo,
                    li.is_editable AS IsEditable,
                    li.list_header_uid AS ListHeaderUID
                FROM 
                    list_item li
                INNER JOIN list_header lh ON lh.uid = li.list_header_uid
                WHERE 
                    lh.code IN @Codes
            ) AS SubQuery
            ");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"
                SELECT COUNT(1) AS Cnt 
                FROM (
                    SELECT 
                        li.uid AS UID,
                        li.created_by AS CreatedBy,
                        li.created_time AS CreatedTime,
                        li.modified_by AS ModifiedBy,
                        li.modified_time AS ModifiedTime,
                        li.server_add_time AS ServerAddTime,
                        li.server_modified_time AS ServerModifiedTime,
                        li.code AS Code,
                        li.name AS Name,
                        li.serial_no AS SerialNo,
                        li.is_editable AS IsEditable,
                        li.list_header_uid AS ListHeaderUID
                    FROM 
                        list_item li
                    INNER JOIN list_header lh ON lh.uid = li.list_header_uid
                    WHERE 
                        lh.code IN @Codes
                ) AS SubQuery");
                }

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");

                    AppendFilterCriteria<Winit.Modules.ListHeader.Model.Interfaces.IListItem>(filterCriterias, sbFilterCriteria, parameters);

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
                        // sql.Append($@" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                        sql.Append($@" ORDER BY SerialNo OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");

                    }
                }
                IEnumerable<Winit.Modules.ListHeader.Model.Interfaces.IListItem> listItemList = await ExecuteQueryAsync<Winit.Modules.ListHeader.Model.Interfaces.IListItem>(sql.ToString(), parameters);

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
            catch (Exception ex)
            {
                throw new Exception("Error fetching list items by codes", ex);
            }
        }


    }
}
