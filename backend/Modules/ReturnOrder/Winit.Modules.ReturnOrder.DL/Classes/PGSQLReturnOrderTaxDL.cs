
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ReturnOrder.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.DL.Classes
{
    public class PGSQLReturnOrderTaxDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IReturnOrderTaxDL
    {
        public PGSQLReturnOrderTaxDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax>> SelectAllReturnOrderTaxDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                                    id AS ""Id"",
                                    uid AS ""UID"",
                                    created_by AS ""CreatedBy"",
                                    created_time AS ""CreatedTime"",
                                    modified_by AS ""ModifiedBy"",
                                    modified_time AS ""ModifiedTime"",
                                    server_add_time AS ""ServerAddTime"",
                                    server_modified_time AS ""ServerModifiedTime"",
                                    return_order_uid AS ""ReturnOrderUid"",
                                    return_order_line_uid AS ""ReturnOrderLineUid"",
                                    tax_uid AS ""TaxUid"",
                                    tax_slab_uid AS ""TaxSlabUid"",
                                    tax_amount AS ""TaxAmount"",
                                    tax_name AS ""TaxName"",
                                    applicable_at AS ""ApplicableAt"",
                                    dependent_tax_uid AS ""DependentTaxUid"",
                                    dependent_tax_name AS ""DependentTaxName"",
                                    tax_calculation_type AS ""TaxCalculationType"",
                                    base_tax_rate AS ""BaseTaxRate"",
                                    range_start AS ""RangeStart"",
                                    range_end AS ""RangeEnd"",
                                    tax_rate AS ""TaxRate""
                                FROM 
                                    return_order_tax;");
                                
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT  COUNT(1) AS ""Cnt""FROM return_order_tax;");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax>(filterCriterias, sbFilterCriteria, parameters);;

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

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IReturnOrderTax>().GetType();
                IEnumerable<Model.Interfaces.IReturnOrderTax> returnOrderTaxs = await ExecuteQueryAsync<Model.Interfaces.IReturnOrderTax>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax> pagedResponse = new PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax>
                {
                    PagedData = returnOrderTaxs,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax> SelectReturnOrderTaxByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"Select id AS ""Id"",
                                    uid AS ""UID"",
                                    created_by AS ""CreatedBy"",
                                    created_time AS ""CreatedTime"",
                                    modified_by AS ""ModifiedBy"",
                                    modified_time AS ""ModifiedTime"",
                                    server_add_time AS ""ServerAddTime"",
                                    server_modified_time AS ""ServerModifiedTime"",
                                    return_order_uid AS ""ReturnOrderUid"",
                                    return_order_line_uid AS ""ReturnOrderLineUid"",
                                    tax_uid AS ""TaxUid"",
                                    tax_slab_uid AS ""TaxSlabUid"",
                                    tax_amount AS ""TaxAmount"",
                                    tax_name AS ""TaxName"",
                                    applicable_at AS ""ApplicableAt"",
                                    dependent_tax_uid AS ""DependentTaxUid"",
                                    dependent_tax_name AS ""DependentTaxName"",
                                    tax_calculation_type AS ""TaxCalculationType"",
                                    base_tax_rate AS ""BaseTaxRate"",
                                    range_start AS ""RangeStart"",
                                    range_end AS ""RangeEnd"",
                                    tax_rate AS ""TaxRate""
                                FROM 
                                    return_order_tax;";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IReturnOrderTax>().GetType();
            Model.Interfaces.IReturnOrderTax ReturnOrderTaxList = await ExecuteSingleAsync<Model.Interfaces.IReturnOrderTax>(sql, parameters, type);
            return ReturnOrderTaxList;
        }
        public async Task<int> CreateReturnOrderTax(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax returnOrderTax)
        {
            try
            {
                var sql = @"INSERT INTO return_order_tax (
                        uid, created_by, created_time, modified_by, modified_time, server_add_time,
                        server_modified_time, return_order_uid, return_order_line_uid, tax_uid, tax_slab_uid, tax_amount, tax_name,
                        applicable_at, dependent_tax_uid, dependent_tax_name, tax_calculation_type, base_tax_rate, range_start, 
                        range_end, tax_rate
                    ) VALUES (
                        @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime,
                        @ServerModifiedTime, @ReturnOrderUid, @ReturnOrderLineUid, @TaxUid, @TaxSlabUid, @TaxAmount, @TaxName, @ApplicableAt,
                        @DependentTaxUid, @DependentTaxName, @TaxCalculationType, @BaseTaxRate, @RangeStart, @RangeEnd, @TaxRate
                    );";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@UID", returnOrderTax.UID },
                    { "@CreatedBy", returnOrderTax.CreatedBy },
                    { "@CreatedTime", returnOrderTax.CreatedTime },
                    { "@ModifiedBy", returnOrderTax.ModifiedBy },
                    { "@ModifiedTime", returnOrderTax.ModifiedTime },
                    { "@ServerAddTime", returnOrderTax.ServerAddTime },
                    { "@ServerModifiedTime", returnOrderTax.ServerModifiedTime },
                    { "@ReturnOrderUID", returnOrderTax.ReturnOrderUID },
                    { "@ReturnOrderLineUID", returnOrderTax.ReturnOrderLineUID },
                    { "@TaxUID", returnOrderTax.TaxUID },
                    { "@TaxSlabUID", returnOrderTax.TaxSlabUID },
                    { "@TaxAmount", returnOrderTax.TaxAmount },
                    { "@TaxName", returnOrderTax.TaxName },
                    { "@ApplicableAt", returnOrderTax.ApplicableAt },
                    { "@DependentTaxUID", returnOrderTax.DependentTaxUID },
                    { "@DependentTaxName", returnOrderTax.DependentTaxName },
                    { "@TaxCalculationType", returnOrderTax.TaxCalculationType },
                    { "@BaseTaxRate", returnOrderTax.BaseTaxRate },
                    { "@RangeStart", returnOrderTax.RangeStart },
                    { "@RangeEnd", returnOrderTax.RangeEnd },
                    { "@TaxRate", returnOrderTax.TaxRate },
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateReturnOrderTax(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax returnOrderTax)
        {
            try
            {
                var sql = @"UPDATE return_order_tax SET 
                                modified_by = @ModifiedBy, 
                                modified_time = @ModifiedTime, 
                                server_modified_time = @ServerModifiedTime, 
                                return_order_uid = @ReturnOrderUid, 
                                return_order_line_uid = @ReturnOrderLineUid, 
                                tax_uid = @TaxUid, 
                                tax_slab_uid = @TaxSlabUid, 
                                tax_amount = @TaxAmount, 
                                tax_name = @TaxName, 
                                applicable_at = @ApplicableAt, 
                                dependent_tax_uid = @DependentTaxUid, 
                                dependent_tax_name = @DependentTaxName, 
                                tax_calculation_type = @TaxCalculationType, 
                                base_tax_rate = @BaseTaxRate, 
                                range_start = @RangeStart, 
                                range_end = @RangeEnd, 
                                tax_rate = @TaxRate 
                            WHERE 
                                uid = @UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   { "@UID", returnOrderTax.UID },
                    { "@CreatedBy", returnOrderTax.CreatedBy },
                    { "@CreatedTime", returnOrderTax.CreatedTime },
                    { "@ModifiedBy", returnOrderTax.ModifiedBy },
                    { "@ModifiedTime", returnOrderTax.ModifiedTime },
                    { "@ServerAddTime", returnOrderTax.ServerAddTime },
                    { "@ServerModifiedTime", returnOrderTax.ServerModifiedTime },
                    { "@ReturnOrderUID", returnOrderTax.ReturnOrderUID },
                    { "@ReturnOrderLineUID", returnOrderTax.ReturnOrderLineUID },
                    { "@TaxUID", returnOrderTax.TaxUID },
                    { "@TaxSlabUID", returnOrderTax.TaxSlabUID },
                    { "@TaxAmount", returnOrderTax.TaxAmount },
                    { "@TaxName", returnOrderTax.TaxName },
                    { "@ApplicableAt", returnOrderTax.ApplicableAt },
                    { "@DependentTaxUID", returnOrderTax.DependentTaxUID },
                    { "@DependentTaxName", returnOrderTax.DependentTaxName },
                    { "@TaxCalculationType", returnOrderTax.TaxCalculationType },
                    { "@BaseTaxRate", returnOrderTax.BaseTaxRate },
                    { "@RangeStart", returnOrderTax.RangeStart },
                    { "@RangeEnd", returnOrderTax.RangeEnd },
                    { "@TaxRate", returnOrderTax.TaxRate },
                };
                return await ExecuteNonQueryAsync(sql, parameters);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteReturnOrderTax(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE FROM return_order_tax WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
