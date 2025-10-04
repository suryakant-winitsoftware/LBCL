using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Store.DL.Classes
{
    public class PGSQLStoreCheckReportDL: Winit.Modules.Base.DL.DBManager.PostgresDBManager, IStoreCheckReportDL
    {
        protected readonly Winit.Modules.FileSys.DL.Interfaces.IFileSysDL _fileSysDL;

        public PGSQLStoreCheckReportDL(IServiceProvider serviceProvider, IConfiguration config, FileSys.DL.Interfaces.IFileSysDL fileSysDL) : base(serviceProvider, config)
        {
           
            _fileSysDL = fileSysDL;
        }
        public async Task<PagedResponse<IStoreCheckReport>> GetStoreCheckReportDetails(
           List<SortCriteria> sortCriterias,
           int pageNumber,
           int pageSize,
           List<FilterCriteria> filterCriterias,
           bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (
                    select
                        sch.uid as UID,
                        '[' || r.code || '] ' || r.name as RouteCode,
                        '[' || e.code || '] ' || e.name as SalesmanCode,
                        '[' || s.code || '] ' || s.name as CustomerCode,
                        fs.relative_path || '/' || fs.file_name AS ImagePath,
                           sch.store_check_date as Date
                    from store_check_history sch
                    inner join route r on r.uid = sch.route_uid
                    inner join store s on s.uid = sch.store_uid
                    inner join emp e on e.uid = sch.emp_uid
                    left join file_sys fs on fs.linked_item_uid = sch.uid
                ) as sub_query");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select count(1) as Cnt from (
                        select
                            sch.uid as UID,
                            '[' || r.code || '] ' || r.name as RouteCode,
                            '[' || e.code || '] ' || e.name as SalesmanCode,
                            '[' || s.code || '] ' || s.name as CustomerCode,
                            fs.relative_path || '/' || fs.file_name AS ImagePath,
                           sch.store_check_date as Date
                        from store_check_history sch
                        inner join route r on r.uid = sch.route_uid
                        inner join store s on s.uid = sch.store_uid
                        inner join emp e on e.uid = sch.emp_uid
                        left join file_sys fs on fs.linked_item_uid = sch.uid
                    ) as sub_query");
                }


                var parameters = new Dictionary<string, object>();
      
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    
                    AppendFilterCriteria<IStoreCheckReport>(filterCriterias, sbFilterCriteria, parameters);
                    
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($@" ORDER BY Date DESC OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                Type type = _serviceProvider.GetRequiredService<IStoreCheckReport>().GetType();
                IEnumerable<IStoreCheckReport> storeCheckReports = await ExecuteQueryAsync<IStoreCheckReport>(sql.ToString(), parameters, type);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<IStoreCheckReport> pagedResponse = new PagedResponse<IStoreCheckReport>
                {
                    PagedData = storeCheckReports,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<IStoreCheckReportItem>> GetStoreCheckReportItems(string uid)
        {
            var sql = @"
        SELECT
            sci.sku_code AS SKU,
            sci.uom AS UOM,
            sci.suggested_qty AS SuggestedQty,
            sci.store_qty AS StoreQty,
            sci.backstore_qty AS BackstoreQty,
            sci.to_fill_qty AS ToFillQty,
            sci.is_available AS IsAvailable,
            sci.is_dre_selected AS IsDRESelected
        FROM store_check_item_history sci
        INNER JOIN sku ON sku.uid = sci.sku_uid
        WHERE sci.store_check_history_uid = @uid
    ";
            var parameters = new Dictionary<string, object> { { "uid", uid } };
            var result = await ExecuteQueryAsync<IStoreCheckReportItem>(sql, parameters);
            return result.Cast<IStoreCheckReportItem>().ToList();
        }

    }
}

