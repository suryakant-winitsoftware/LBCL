using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes
{
    public class PGSQLSKUPriceDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISKUPriceDL
    {
        public PGSQLSKUPriceDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string? type = null)
        {
            try
            {
                string cte = """
                    WITH StoreCTE AS (
                        SELECT * 
                        FROM VW_StoreImpAttributes 
                        WHERE store_uid = @OrgUID  
                    ),
                    LadderingCTE AS (
                        SELECT 
                            spl.product_category_id, 
                            spl.sales_office, 
                            SUM(percentage_discount) AS percentage_discount,
                            ROW_NUMBER() OVER (
                                PARTITION BY spl.product_category_id 
                                ORDER BY spl.product_category_id, spl.sales_office DESC
                            ) AS row_num
                        FROM sku_price_laddering spl
                        Left JOIN StoreCTE 
                            ON spl.branch = StoreCTE.branch_uid 
                            AND spl.broad_customer_classification = StoreCTE.broad_classification 
                            AND (spl.sales_office = StoreCTE.sales_office_uid OR spl.sales_office IS NULL)
                            AND @Date BETWEEN spl.start_date AND spl.end_date  -- $2 is for @Date
                        GROUP BY spl.product_category_id, spl.sales_office
                    ),
                    SkuPrice AS (
                        SELECT
                            sp.id AS id,
                            sp.uid AS uid,
                            sp.created_by AS created_by,
                            sp.created_time AS created_time,
                            sp.modified_by AS modified_by,
                            sp.modified_time AS modified_time,
                            sp.server_add_time AS server_add_time,
                            sp.server_modified_time AS server_modified_time,
                            sp.sku_price_list_uid AS skupricelistuid,
                            sp.sku_code AS sku_code,
                            sp.uom AS uom,
                            sp.default_ws_price AS default_ws_price,
                            sp.default_ret_price AS default_ret_price,
                            sp.dummy_price AS dummy_price,
                            sp.mrp AS mrp,
                            sp.price_upper_limit AS price_upper_limit,
                            sp.price_lower_limit AS price_lower_limit,
                            sp.status AS status,
                            sp.valid_from AS valid_from,
                            sp.valid_upto AS valid_upto,
                            sp.is_active AS is_active,
                            sp.is_tax_included AS is_tax_included,
                            sp.version_no AS version_no,
                            sp.sku_uid AS SKUUID,
                            sp.is_latest AS is_latest,
                            COALESCE(l.percentage_discount, 0) AS percentage_discount,
                            sp.price - (sp.price * COALESCE(l.percentage_discount, 0) * 0.01) AS Price,
                            COALESCE(l.percentage_discount, 0) AS laddering_percentage,
                            sp.price * COALESCE(l.percentage_discount, 0) * 0.01 AS laddering_amount
                        FROM sku_price sp
                        LEFT JOIN sku_ext_data sed ON sp.sku_uid = sed.sku_uid
                        LEFT JOIN LadderingCTE l ON l.product_category_id = sed.product_category_id AND l.row_num = 1
                    )
                    """;
                var sql = new StringBuilder($"""
                                        {cte}
                                       
                                        SELECT * FROM SkuPrice
                                        """);
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder($"""
                                             {cte}
                                             select count(1) from SkuPrice 
                                             """);
                }
                var parameters = new Dictionary<string, object?>();
                var orgparam = filterCriterias?.Find(e => "OrgUID".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                var dateparam = filterCriterias?.Find(e => "Date".Equals(e.Name, StringComparison.OrdinalIgnoreCase));

                // Add OrgUID parameter (use empty string if not provided to prevent SQL error)
                if (orgparam != null)
                {
                    parameters.Add("OrgUID", orgparam.Value);
                    filterCriterias.Remove(orgparam);
                }
                else
                {
                    parameters.Add("OrgUID", ""); // Default empty string to prevent parameter error
                }

                // Add Date parameter
                if (dateparam != null)
                {
                    parameters.Add("Date", dateparam.Value);
                    filterCriterias.Remove(dateparam);
                }
                else
                {
                    parameters.Add("Date", DateTime.Now);
                }
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>(filterCriterias, sbFilterCriteria, parameters);
                    if (!string.IsNullOrEmpty(type) && type.Equals("Default", StringComparison.OrdinalIgnoreCase))
                    {
                        sbFilterCriteria.Append(" AND SkuPriceListUid = @SkuPriceListUid");
                        parameters.Add("SkuPriceListUid", "DefaultPriceList");
                    }
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(type) && type.Equals("Default", StringComparison.OrdinalIgnoreCase))
                    {
                        sql.Append(" WHERE SkuPriceListUid = @SkuPriceListUid");
                        parameters.Add("SkuPriceListUid", "DefaultPriceList");
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
                        sql.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
                    }
                }
                IEnumerable<Model.Interfaces.ISKUPrice> sKUPrices = await ExecuteQueryAsync<Model.Interfaces.ISKUPrice>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>
                {
                    PagedData = sKUPrices,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetailsV1(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                StringBuilder sqlFilterQuery = new();
                StringBuilder innerFilter = new("""
            WHERE EXISTS (
                SELECT 1
                FROM sku_attributes sa
                WHERE sa.sku_uid = sp.sku_uid
        """);

                var parameters = new Dictionary<string, object?>();

                if (filterCriterias is not null)
                {
                    var isActiveFilter = filterCriterias.Find(e => "IsActive".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                    if (isActiveFilter == null)
                    {
                        sqlFilterQuery.Append(" AND IsActive = true ");
                    }
                    else
                    {
                        filterCriterias.Remove(isActiveFilter);
                    }

                    var skuCodeNameFilter = filterCriterias.Find(e => "skucodeandname".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                    if (skuCodeNameFilter != null)
                    {
                        sqlFilterQuery.Append(" AND (sku_code ILIKE '%' || @skucodeandname || '%' OR SKUName ILIKE '%' || @skucodeandname || '%') ");
                        parameters.Add("skucodeandname", skuCodeNameFilter.Value);
                        filterCriterias.Remove(skuCodeNameFilter);
                    }

                    var divisionFilter = filterCriterias.Find(e => "DivisionUIDs".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                    if (divisionFilter is not null)
                    {
                        sqlFilterQuery.Append(" AND divisionuid = ANY(@DivisonUIDs) ");
                        parameters.Add("DivisonUIDs", JsonConvert.DeserializeObject<List<string>>(divisionFilter.Value.ToString()));
                        filterCriterias.Remove(divisionFilter);
                    }

                    var attributeTypeFilter = filterCriterias.Find(e => "AttributeType".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                    if (attributeTypeFilter is not null)
                    {
                        innerFilter.Append(" AND sa.type = ANY(@AttributeTypes)");
                        parameters.Add("AttributeTypes", JsonConvert.DeserializeObject<List<string>>(attributeTypeFilter.Value.ToString()));
                        filterCriterias.Remove(attributeTypeFilter);
                    }

                    var attributeValueFilter = filterCriterias.Find(e => "AttributeValue".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                    if (attributeValueFilter is not null)
                    {
                        innerFilter.Append(" AND sa.value = ANY(@AttributeValue)");
                        parameters.Add("AttributeValue", JsonConvert.DeserializeObject<List<string>>(attributeValueFilter.Value.ToString()));
                        filterCriterias.Remove(attributeValueFilter);
                    }
                }

                innerFilter.Append(" ) ");

                string cSql = $"""
            SELECT  
                sp.id AS Id,
                sp.uid AS Uid,
                sp.created_by AS CreatedBy,
                sp.created_time AS CreatedTime,
                sp.modified_by AS ModifiedBy,
                sp.modified_time AS ModifiedTime,
                sp.server_add_time AS ServerAddTime,
                sp.server_modified_time AS ServerModifiedTime,
                sp.sku_price_list_uid AS SkuPriceListUid,
                sp.uom AS Uom,
                sp.price AS Price,
                sp.default_ws_price AS DefaultWsPrice,
                sp.default_ret_price AS DefaultRetPrice,
                sp.dummy_price AS DummyPrice,
                sp.mrp AS Mrp,
                sp.price_upper_limit AS PriceUpperLimit,
                sp.price_lower_limit AS PriceLowerLimit,
                sp.status AS Status,
                sp.valid_from AS ValidFrom,
                sp.valid_upto AS ValidUpto,
                sp.is_active AS IsActive,
                sp.is_tax_included AS IsTaxIncluded,
                sp.version_no AS VersionNo,
                sp.sku_uid AS SkuUid,
                sp.is_latest AS IsLatest,
                sp.sku_code AS SkuCode,
                s.long_name AS SKUName,
                s.supplier_org_uid AS divisionuid,
                spl.org_uid AS OrgUID,
                COALESCE(saData.AttributeTypes, '') AS AttributeTypes,
                COALESCE(saData.AttributeValues, '') AS AttributeValues
            FROM 
                sku_price sp
            INNER JOIN sku_price_list spl ON sp.sku_price_list_uid = spl.uid
            INNER JOIN sku s ON sp.sku_uid = s.uid
            CROSS JOIN LATERAL (
                SELECT 
                    STRING_AGG(sa.type, ', ') AS AttributeTypes,
                    STRING_AGG(sa.value, ', ') AS AttributeValues
                FROM sku_attributes sa
                WHERE sa.sku_uid = sp.sku_uid
            ) AS saData
            {innerFilter}
        """;

                StringBuilder sql = new($"""
            SELECT * FROM (
                {cSql}
            ) AS SubQuery
            WHERE 1=1
        """);

                StringBuilder sqlCount = new();
                if (isCountRequired)
                {
                    sqlCount = new($"""
                SELECT COUNT(1) FROM (
                    {cSql}
                ) AS SubQuery
                WHERE 1=1
            """);
                }

                sql.Append(sqlFilterQuery);
                sqlCount.Append(sqlFilterQuery);

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new();
                    sbFilterCriteria.Append(" AND ");
                    AppendFilterCriteria<ISKUPrice>(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                        sqlCount.Append(sbFilterCriteria);
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} LIMIT {pageSize}");
                }


                var sKUPrices = await ExecuteQueryAsync<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                return new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>
                {
                    PagedData = sKUPrices,
                    TotalCount = totalCount
                };
            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> SelectSKUPriceByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"SELECT id AS Id,
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                sku_code AS SKUCode,
                sku_price_list_uid AS SKUPriceListUid,
                uom AS UOM,
                price AS Price,
                default_ws_price AS DefaultWSPrice,
                default_ret_price AS DefaultRetPrice,
                dummy_price AS DummyPrice,
                mrp AS MRP,
                price_upper_limit AS PriceUpperLimit,
                price_lower_limit AS PriceLowerLimit,
                status AS Status,
                valid_from AS ValidFrom,
                valid_upto AS ValidUpto,
                is_active AS IsActive,
                is_tax_included AS IsTaxIncluded,
                version_no AS VersionNo,
                sku_uid AS SKUUID,
                is_latest AS IsLatest
            FROM sku_price
            WHERE uid = @UID;";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUPrice>().GetType();
            Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPriceDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>(sql, parameters, type);
            return sKUPriceDetails;
        }
        public async Task<int> CreateSKUPrice(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice)
        {
            try
            {
                var sql = @"INSERT INTO sku_price (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, sku_price_list_uid,
                       sku_code, uom, price, default_ws_price, default_ret_price, dummy_price, mrp, price_upper_limit, price_lower_limit, status,
                       valid_from, valid_upto, is_active, is_tax_included, version_no, sku_uid) VALUES (@UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime ,@ServerModifiedTime ,@SKUPriceListUID ,
                        @SKUCode ,@UOM ,@Price ,@DefaultWSPrice ,@DefaultRetPrice ,@DummyPrice ,@MRP ,@PriceUpperLimit ,@PriceLowerLimit ,@Status ,@ValidFrom ,@ValidUpto ,@IsActive ,@IsTaxIncluded ,@VersionNo,@SKUUID);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",sKUPrice.UID},
                   {"CreatedBy",sKUPrice.CreatedBy},
                   {"CreatedTime",sKUPrice.CreatedTime},
                   {"ModifiedBy",sKUPrice.ModifiedBy},
                   {"ModifiedTime",sKUPrice.ModifiedTime},
                   {"ServerAddTime",sKUPrice.ServerAddTime},
                   {"ServerModifiedTime",sKUPrice.ServerModifiedTime},
                   {"SKUPriceListUID",sKUPrice.SKUPriceListUID},
                   {"SKUCode",sKUPrice.SKUCode},
                   {"UOM",sKUPrice.UOM},
                   {"Price",sKUPrice.Price},
                   {"DefaultWSPrice",sKUPrice.DefaultWSPrice},
                   {"DefaultRetPrice",sKUPrice.DefaultRetPrice},
                   {"DummyPrice",sKUPrice.DummyPrice},
                   {"MRP",sKUPrice.MRP},
                   {"PriceUpperLimit",sKUPrice.PriceUpperLimit},
                   {"PriceLowerLimit",sKUPrice.PriceLowerLimit},
                   {"Status",sKUPrice.Status},
                   {"ValidFrom",sKUPrice.ValidFrom},
                   {"ValidUpto",sKUPrice.ValidUpto},
                   {"IsActive",sKUPrice.IsActive},
                   {"IsTaxIncluded",sKUPrice.IsTaxIncluded},
                   {"VersionNo",sKUPrice.VersionNo},
                   {"SKUUID",sKUPrice.SKUUID}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSKUPrice(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice)
        {
            try
            {
                var sql = @"UPDATE sku_price
                SET modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime,
                    sku_price_list_uid = @SKUPriceListUID,
                    sku_code = @SKUCode,
                    uom = @UOM,
                    price = @Price,
                    default_ws_price = @DefaultWSPrice,
                    default_ret_price = @DefaultRetPrice,
                    dummy_price = @DummyPrice,
                    mrp = @MRP,
                    price_upper_limit = @PriceUpperLimit,
                    price_lower_limit = @PriceLowerLimit,
                    status = @Status,
                    valid_from = @ValidFrom,
                    valid_upto = @ValidUpto,
                    is_active = @IsActive,
                    is_tax_included = @IsTaxIncluded,
                    version_no = @VersionNo
                WHERE uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",sKUPrice.UID},
                   {"ModifiedBy",sKUPrice.ModifiedBy},
                   {"ModifiedTime",sKUPrice.ModifiedTime},
                   {"ServerModifiedTime",sKUPrice.ServerModifiedTime},
                   {"SKUPriceListUID",sKUPrice.SKUPriceListUID},
                   {"SKUCode",sKUPrice.SKUCode},
                   {"UOM",sKUPrice.UOM},
                   {"Price",sKUPrice.Price},
                   {"DefaultWSPrice",sKUPrice.DefaultWSPrice},
                   {"DefaultRetPrice",sKUPrice.DefaultRetPrice},
                   {"DummyPrice",sKUPrice.DummyPrice},
                   {"MRP",sKUPrice.MRP},
                   {"PriceUpperLimit",sKUPrice.PriceUpperLimit},
                   {"PriceLowerLimit",sKUPrice.PriceLowerLimit},
                   {"Status",sKUPrice.Status},
                   {"ValidFrom",sKUPrice.ValidFrom},
                   {"ValidUpto",sKUPrice.ValidUpto},
                   {"IsActive",sKUPrice.IsActive},
                   {"IsTaxIncluded",sKUPrice.IsTaxIncluded},
                   {"VersionNo",sKUPrice.VersionNo}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)

            {
                throw;
            }
        }
        public async Task<int> UpdateSKUPriceList(List<Winit.Modules.SKU.Model.Classes.SKUPrice> sKUPriceList)
        {
            int retVal = -1;
            using (var connection = PostgreConnection())
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var sKUPrice in sKUPriceList)
                        {
                            switch (sKUPrice.ActionType)
                            {
                                case ActionType.Add:
                                    retVal = await InsertSKUPrice(connection, transaction, sKUPrice);
                                    break;

                                case ActionType.Update:
                                    retVal = await UpdateSKUPrice(connection, transaction, sKUPrice);
                                    break;
                            }

                            if (retVal == -1)
                            {
                                transaction.Rollback();
                                throw new Exception((sKUPrice.ActionType == ActionType.Add ? "Insert " : " Update ") + " Failed for " + sKUPrice.SKUCode);
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            return retVal;
        }
        public async Task<int> DeleteSKUPrice(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE FROM sku_price
                        WHERE uid = @UID;";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<(List<ISKUPriceList>, List<ISKUPrice>, int)> SelectSKUPriceViewByUID(
             List<SortCriteria> sortCriterias,
             int pageNumber,
             int pageSize,
             List<FilterCriteria> filterCriterias,
             bool isCountRequired,
             string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "UID", UID },
                };

                var sKUPriceListSql = new StringBuilder(@"SELECT id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    company_uid AS CompanyUID,
                    code AS Code,
                    name AS Name,
                    type AS Type,
                    org_uid AS OrgUID,
                    distribution_channel_uid AS DistributionChannelUID,
                    priority AS Priority,
                    selection_group AS SelectionGroup,
                    selection_type AS SelectionType,
                    selection_uid AS SelectionUID,
                    is_active AS IsActive,
                    status AS Status,
                    valid_from AS ValidFrom,
                    valid_upto AS ValidUpto
                FROM sku_price_list
                WHERE uid = @UID;");

                Type skuPriceType = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUPriceList>().GetType();
                List<Model.Interfaces.ISKUPriceList> skuPriceList = await ExecuteQueryAsync<Model.Interfaces.ISKUPriceList>(sKUPriceListSql.ToString(), parameters, skuPriceType);

                var sKUPriceSQL = new StringBuilder(@"SELECT sp.id AS Id,
                    sp.uid AS UID,
                    sp.created_by AS CreatedBy,
                    sp.created_time AS CreatedTime,
                    sp.modified_by AS ModifiedBy,
                    sp.modified_time AS ModifiedTime,
                    sp.server_add_time AS ServerAddTime,
                    sp.server_modified_time AS ServerModifiedTime,
                    sp.sku_price_list_uid AS SKUPriceListUID,
                    sp.sku_code AS SKUCode,
                    sp.uom AS UOM,
                    sp.price AS Price,
                    sp.default_ws_price AS DefaultWSPrice,
                    sp.default_ret_price AS DefaultRetPrice,
                    sp.dummy_price AS DummyPrice,
                    sp.mrp AS MRP,
                    sp.price_upper_limit AS PriceUpperLimit,
                    sp.price_lower_limit AS PriceLowerLimit,
                    sp.status AS Status,
                    sp.valid_from AS ValidFrom,
                    sp.valid_upto AS ValidUpto,
                    sp.is_active AS IsActive,
                    sp.is_tax_included AS IsTaxIncluded,
                    sp.version_no AS VersionNo,
                    sp.sku_uid AS SKUUID,
					s.name as SKUName,
                    sp.is_latest AS IsLatest
                FROM sku_price sp
				Left join sku s on s.uid=sp.sku_uid
                WHERE sp.sku_price_list_uid = @UID");

                var sqlCount = new StringBuilder();

                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT sp.id AS Id,
                    sp.uid AS UID,
                    sp.created_by AS CreatedBy,
                    sp.created_time AS CreatedTime,
                    sp.modified_by AS ModifiedBy,
                    sp.modified_time AS ModifiedTime,
                    sp.server_add_time AS ServerAddTime,
                    sp.server_modified_time AS ServerModifiedTime,
                    sp.sku_price_list_uid AS SKUPriceListUID,
                    sp.sku_code AS SKUCode,
                    sp.uom AS UOM,
                    sp.price AS Price,
                    sp.default_ws_price AS DefaultWSPrice,
                    sp.default_ret_price AS DefaultRetPrice,
                    sp.dummy_price AS DummyPrice,
                    sp.mrp AS MRP,
                    sp.price_upper_limit AS PriceUpperLimit,
                    sp.price_lower_limit AS PriceLowerLimit,
                    sp.status AS Status,
                    sp.valid_from AS ValidFrom,
                    sp.valid_upto AS ValidUpto,
                    sp.is_active AS IsActive,
                    sp.is_tax_included AS IsTaxIncluded,
                    sp.version_no AS VersionNo,
                    sp.sku_uid AS SKUUID,
					s.name as SKUName,
                    sp.is_latest AS IsLatest
                FROM sku_price sp
				Left join sku s on s.uid=sp.sku_uid WHERE sku_price_list_uid= @UID");
                }


                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" AND ");
                    AppendFilterCriteria<Model.Interfaces.ISKUPrice>(filterCriterias, sbFilterCriteria, parameters);

                    sKUPriceSQL.Append(sbFilterCriteria);

                    if (isCountRequired)
                    {
                        // Append filter inside the subquery before closing it
                        sqlCount.Append(sbFilterCriteria);
                    }
                }

                // Close the subquery for count after all filters are applied
                if (isCountRequired)
                {
                    sqlCount.Append(") as SubQuery");
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sKUPriceSQL.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sKUPriceSQL);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sKUPriceSQL.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
                }

                int totalCount = 0;

                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                Type sKUPricesType = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUPrice>().GetType();
                List<Model.Interfaces.ISKUPrice> sKUPrices = await ExecuteQueryAsync<Model.Interfaces.ISKUPrice>(sKUPriceSQL.ToString(), parameters, sKUPricesType);

                return (skuPriceList, sKUPrices, totalCount);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateSKUPriceView(Winit.Modules.SKU.Model.Classes.SKUPriceViewDTO sKUPriceViewDTO)
        {
            int count = 0;
            try
            {
                using (var connection = PostgreConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var skuPriceListQuery = @"INSERT INTO sku_price_list (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                                                     company_uid, code, name, type, org_uid, distribution_channel_uid, priority, selection_group,
                                                     selection_type, selection_uid, is_active, status, valid_from, valid_upto) VALUES (@UID,@CreatedBy,@CreatedTime,@ModifiedBy,
                                                    @ModifiedTime,@ServerAddTime,@ServerModifiedTime,@CompanyUID,@Code,@Name,@Type,@OrgUID,@DistributionChannelUID,
                                                    @Priority,@SelectionGroup,@SelectionType,@SelectionUID,@IsActive,@Status,@ValidFrom,@ValidUpto);";
                            var skuPriceListParameters = new Dictionary<string, object>
                    {
                        {"UID", sKUPriceViewDTO.SKUPriceGroup.UID},
                        {"CreatedBy",  sKUPriceViewDTO.SKUPriceGroup.CreatedBy},
                        {"CreatedTime",  sKUPriceViewDTO.SKUPriceGroup.CreatedTime},
                        {"ModifiedBy",  sKUPriceViewDTO.SKUPriceGroup.ModifiedBy},
                        {"ModifiedTime",  sKUPriceViewDTO.SKUPriceGroup.ModifiedTime},
                        {"ServerAddTime",  sKUPriceViewDTO.SKUPriceGroup.ServerAddTime},
                        {"ServerModifiedTime",  sKUPriceViewDTO.SKUPriceGroup.ServerModifiedTime},
                        {"CompanyUID",  sKUPriceViewDTO.SKUPriceGroup.CompanyUID},
                        {"Code",  sKUPriceViewDTO.SKUPriceGroup.Code},
                        {"Name",  sKUPriceViewDTO.SKUPriceGroup.Name},
                        {"Type",  sKUPriceViewDTO.SKUPriceGroup.Type},
                        {"OrgUID",  sKUPriceViewDTO.SKUPriceGroup.OrgUID},
                        {"DistributionChannelUID",  sKUPriceViewDTO.SKUPriceGroup.DistributionChannelUID},
                        {"Priority",  sKUPriceViewDTO.SKUPriceGroup.Priority},
                        {"SelectionGroup", sKUPriceViewDTO.SKUPriceGroup.SelectionGroup},
                        {"SelectionType",  sKUPriceViewDTO.SKUPriceGroup.SelectionType},
                        {"SelectionUID",  sKUPriceViewDTO.SKUPriceGroup.SelectionUID},
                        {"IsActive",  sKUPriceViewDTO.SKUPriceGroup.IsActive},
                        {"Status",  sKUPriceViewDTO.SKUPriceGroup.Status},
                        {"ValidFrom", sKUPriceViewDTO.SKUPriceGroup.ValidFrom},
                        {"ValidUpto",  sKUPriceViewDTO.SKUPriceGroup.ValidUpto},

                    };
                            count += await ExecuteNonQueryAsync(skuPriceListQuery, connection, transaction, skuPriceListParameters);
                            if (count < 0)
                            {
                                transaction.Rollback();
                                throw new Exception("SKUPriceGroup Insert failed");
                            }
                            foreach (var sKUPrice in sKUPriceViewDTO.SKUPriceList)
                            {

                                var skuPriceQuery = @"INSERT INTO sku_price (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                                                       sku_price_list_uid, sku_code, uom, price, default_ws_price, default_ret_price, dummy_price,
                                                       mrp, price_upper_limit, price_lower_limit, status, valid_from, valid_upto,
                                                       is_active, is_tax_included, version_no, sku_uid) VALUES (@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,
                                                        @SKUPriceListUID,@SKUCode,@UOM,@Price,@DefaultWSPrice,@DefaultRetPrice,@DummyPrice,@MRP,@PriceUpperLimit,
                                                        @PriceLowerLimit,@Status,@ValidFrom,@ValidUpto,@IsActive,@IsTaxIncluded,@VersionNo,@SKUUID);";
                                var skuPriceParameters = new Dictionary<string, object>
                        {
                            {"UID", sKUPrice.UID},
                            {"CreatedBy", sKUPrice.CreatedBy},
                            {"CreatedTime", sKUPrice.CreatedTime},
                            {"ModifiedBy", sKUPrice.ModifiedBy},
                            {"ModifiedTime", sKUPrice.ModifiedTime},
                            {"ServerAddTime", sKUPrice.ServerAddTime},
                            {"ServerModifiedTime", sKUPrice.ServerModifiedTime},
                            {"SKUPriceListUID", sKUPrice.SKUPriceListUID},
                            {"Price", sKUPrice.Price},
                            {"DefaultWSPrice", sKUPrice.DefaultWSPrice},
                            {"MRP", sKUPrice.MRP},
                            {"PriceUpperLimit", sKUPrice.PriceUpperLimit},
                            {"PriceLowerLimit", sKUPrice.PriceLowerLimit},
                            {"Status", sKUPrice.Status},
                            {"ValidFrom", sKUPrice.ValidFrom},
                            {"ValidUpto", sKUPrice.ValidUpto},
                            {"IsActive", sKUPrice.IsActive},
                            {"IsTaxIncluded", sKUPrice.IsTaxIncluded},
                            {"VersionNo", sKUPrice.VersionNo},
                            {"SKUUID", sKUPrice.SKUUID},
                            {"SKUCode", sKUPrice.SKUCode},
                            {"UOM", sKUPrice.UOM},
                            {"DefaultRetPrice", sKUPrice.DefaultRetPrice},
                            {"DummyPrice", sKUPrice.DummyPrice},
                        };
                                count += await ExecuteNonQueryAsync(skuPriceQuery, connection, transaction, skuPriceParameters);
                                if (count < 0)
                                {
                                    transaction.Rollback();
                                    throw new Exception("SkuPrice Table Insert Failed");
                                }
                            }
                            transaction.Commit();
                            int total = count;
                            return total;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSKUPriceView(Winit.Modules.SKU.Model.Classes.SKUPriceViewDTO sKUPriceViewDTO)
        {
            int count = 0;
            try
            {
                using (var connection = PostgreConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var skuPriceListQuery = @"UPDATE sku_price_list
                            SET modified_by = @ModifiedBy,
                                modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime,
                                code = @Code,
                                name = @Name,
                                type = @Type,
                                priority = @Priority,
                                selection_group = @SelectionGroup,
                                selection_type = @SelectionType,
                                selection_uid = @SelectionUID,
                                is_active = @IsActive,
                                status = @Status,
                                valid_from = @ValidFrom,
                                valid_upto = @ValidUpto
                            WHERE uid = @UID;";
                            var skuPriceListParameters = new Dictionary<string, object>
                    {
                        {"UID", sKUPriceViewDTO.SKUPriceGroup.UID},
                        {"ModifiedBy",  sKUPriceViewDTO.SKUPriceGroup.ModifiedBy},
                        {"ModifiedTime",  sKUPriceViewDTO.SKUPriceGroup.ModifiedTime},
                        {"ServerModifiedTime",  sKUPriceViewDTO.SKUPriceGroup.ServerModifiedTime},
                        {"Code",  sKUPriceViewDTO.SKUPriceGroup.Code},
                        {"Name",  sKUPriceViewDTO.SKUPriceGroup.Name},
                        {"Type",  sKUPriceViewDTO.SKUPriceGroup.Type},
                        {"Priority",  sKUPriceViewDTO.SKUPriceGroup.Priority},
                        {"SelectionGroup", sKUPriceViewDTO.SKUPriceGroup.SelectionGroup},
                        {"SelectionType",  sKUPriceViewDTO.SKUPriceGroup.SelectionType},
                        {"SelectionUID",  sKUPriceViewDTO.SKUPriceGroup.SelectionUID},
                        {"IsActive",  sKUPriceViewDTO.SKUPriceGroup.IsActive},
                        {"Status",  sKUPriceViewDTO.SKUPriceGroup.Status},
                        {"ValidFrom", sKUPriceViewDTO.SKUPriceGroup.ValidFrom},
                        {"ValidUpto",  sKUPriceViewDTO.SKUPriceGroup.ValidUpto},

                    };
                            count += await ExecuteNonQueryAsync(skuPriceListQuery, connection, transaction, skuPriceListParameters);
                            if (count != 1)
                            {
                                transaction.Rollback();
                                throw new Exception("Return Order Insert failed");
                            }
                            foreach (var sKUPrice in sKUPriceViewDTO.SKUPriceList)
                            {

                                switch (sKUPrice.ActionType)
                                {
                                    case ActionType.Add:
                                        count += await InsertSKUPrice(connection, transaction, sKUPrice);
                                        break;

                                    case ActionType.Update:
                                        count += await UpdateSKUPrice(connection, transaction, sKUPrice);
                                        break;
                                }

                            }
                            transaction.Commit();
                            int total = count;
                            return total;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<int> InsertSKUPrice(NpgsqlConnection connection, NpgsqlTransaction transaction, SKUPrice sKUPrice)
        {
            var skuPriceQuery = @"INSERT INTO sku_price (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                                                        sku_price_list_uid, sku_code, uom, price, default_ws_price, default_ret_price, dummy_price,
                                                        mrp, price_upper_limit, price_lower_limit, status, valid_from, valid_upto,
                                                        is_active, is_tax_included, version_no, sku_uid, is_latest) VALUES (@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,
                                                        @SKUPriceListUID,@SKUCode,@UOM,@Price,@DefaultWSPrice,@DefaultRetPrice,@DummyPrice,@MRP,@PriceUpperLimit,
                                                        @PriceLowerLimit,@Status,@ValidFrom,@ValidUpto,@IsActive,@IsTaxIncluded,@VersionNo,@SKUUID,@IsLatest);";
            var skuPriceParameters = new Dictionary<string, object>
                        {
                            {"UID", sKUPrice.UID},
                            {"CreatedBy", sKUPrice.CreatedBy},
                            {"CreatedTime", sKUPrice.CreatedTime},
                            {"ModifiedBy", sKUPrice.ModifiedBy},
                            {"ModifiedTime", sKUPrice.ModifiedTime},
                            {"ServerAddTime", sKUPrice.ServerAddTime},
                            {"ServerModifiedTime", sKUPrice.ServerModifiedTime},
                            {"SKUPriceListUID", sKUPrice.SKUPriceListUID},
                            {"Price", sKUPrice.Price},
                            {"DefaultWSPrice", sKUPrice.DefaultWSPrice},
                            {"MRP", sKUPrice.MRP},
                            {"PriceUpperLimit", sKUPrice.PriceUpperLimit},
                            {"Status", sKUPrice.Status},
                            {"ValidFrom", sKUPrice.ValidFrom},
                            {"ValidUpto", sKUPrice.ValidUpto},
                            {"IsActive", sKUPrice.IsActive},
                            {"IsTaxIncluded", sKUPrice.IsTaxIncluded},
                            {"VersionNo", sKUPrice.VersionNo},
                            {"SKUUID", sKUPrice.SKUUID},
                            {"SKUCode", sKUPrice.SKUCode},
                            {"uom", sKUPrice.UOM},
                            {"DefaultRetPrice", sKUPrice.DefaultRetPrice},
                            {"DummyPrice", sKUPrice.DummyPrice},
                            {"PriceLowerLimit", sKUPrice.PriceLowerLimit},
                            {"IsLatest", sKUPrice.IsLatest},
                        };
            return await ExecuteNonQueryAsync(skuPriceQuery, connection, transaction, skuPriceParameters);
        }

        private async Task<int> UpdateSKUPrice(NpgsqlConnection connection, NpgsqlTransaction transaction, SKUPrice sKUPrice)
        {
            var skuPriceQuery = @"UPDATE sku_price
            SET modified_by = @ModifiedBy,
                modified_time = @ModifiedTime,
                server_modified_time = @ServerModifiedTime,
                sku_code = @SKUCode,
                uom = @UOM,
                price = @Price,
                default_ws_price = @DefaultWSPrice,
                default_ret_price = @DefaultRetPrice,
                dummy_price = @DummyPrice,
                mrp = @MRP,
                price_upper_limit = @PriceUpperLimit,
                price_lower_limit = @PriceLowerLimit,
                status = @Status,
                valid_from = @ValidFrom,
                valid_upto = @ValidUpto,
                is_active = @IsActive,
                is_tax_included = @IsTaxIncluded,
                is_latest = @IsLatest,
                version_no = @VersionNo
            WHERE uid = @UID;";
            var skuPriceParameters = new Dictionary<string, object>
                        {
                            {"ModifiedBy", sKUPrice.ModifiedBy},
                            {"ModifiedTime", sKUPrice.ModifiedTime},
                            {"ServerModifiedTime", sKUPrice.ServerModifiedTime},
                            {"SKUCode", sKUPrice.SKUCode},
                            {"UOM", sKUPrice.UOM},
                            {"UID", sKUPrice.UID},
                            {"Price", sKUPrice.Price},
                            {"DefaultWSPrice", sKUPrice.DefaultWSPrice},
                            {"DefaultRetPrice", sKUPrice.DefaultRetPrice},
                            {"DummyPrice", sKUPrice.DummyPrice},
                            {"MRP", sKUPrice.MRP},
                            {"PriceUpperLimit", sKUPrice.PriceUpperLimit},
                            {"PriceLowerLimit", sKUPrice.PriceLowerLimit},
                            {"Status", sKUPrice.Status},
                            {"ValidFrom", sKUPrice.ValidFrom},
                            {"ValidUpto", sKUPrice.ValidUpto},
                            {"IsActive", sKUPrice.IsActive},
                            {"IsTaxIncluded", sKUPrice.IsTaxIncluded},
                            {"IsLatest", sKUPrice.IsLatest},
                            {"VersionNo", sKUPrice.VersionNo},
                        };
            return await ExecuteNonQueryAsync(skuPriceQuery, connection, transaction, skuPriceParameters);
        }

        public async Task<int> CreateStandardPriceForSKU(string skuUID)
        {
            try
            {
                var sql = @"INSERT INTO sku_price (uid, created_by, created_time, modified_by, modified_time, 
                            server_add_time, server_modified_time, sku_price_list_uid, sku_code, uom,
                            price, default_ws_price, default_ret_price, dummy_price, mrp, price_upper_limit, price_lower_limit,
                            status, valid_from, valid_upto, is_active, is_tax_included, version_no, sku_uid, is_latest)
                            SELECT SPL.uid || s.uid uid, s.created_by, s.created_time, 
                            s.modified_by, 
                            s.modified_time, s.server_add_time,
                            s.server_modified_time, SPL.uid sku_price_list_uid, 
                            s.code sku_code, s.base_uom uom,
                            0 price, 0 default_ws_price, 0 default_ret_price, 0 dummy_price, 0 mrp, 0 price_upper_limit, 
                            0 price_lower_limit,
                            'Published' status, s.from_date valid_from, '2099-12-31' valid_upto, true is_active, 
                            false is_tax_included, 'V1' version_no, s.uid sku_uid, 
                            1 is_latest
                            FROM sku_price_list SPL 
                            INNER JOIN sku S ON SPL.type = 'FR' AND selection_group = 'Org' 
                            AND selection_type = 'Org' AND Status = 'Published'  
                            AND s.uid = @SKUUID
                            LEFT JOIN sku_price SP on SP.sku_price_list_uid = SPL.uid AND SP.sku_uid = s.uid
                            WHERE sp.id is null";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"SKUUID",skuUID},

                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetails_BySKUUIDs(List<SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, List<string> skuUIDs)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (SELECT 
                                                        sp.id AS Id,
                                                        sp.uid AS Uid,
                                                        sp.created_by AS CreatedBy,
                                                        sp.created_time AS CreatedTime,
                                                        sp.modified_by AS ModifiedBy,
                                                        sp.modified_time AS ModifiedTime,
                                                        sp.server_add_time AS ServerAddTime,
                                                        sp.server_modified_time AS ServerModifiedTime,
                                                        sp.sku_price_list_uid AS SkuPriceListUid,
                                                        sp.sku_code AS SkuCode,S.name as SKUName,
                                                        sp.uom AS Uom,
                                                        sp.price AS Price,
                                                        sp.default_ws_price AS DefaultWsPrice,
                                                        sp.default_ret_price AS DefaultRetPrice,
                                                        sp.dummy_price AS DummyPrice,
                                                        sp.mrp AS Mrp,
                                                        sp.price_upper_limit AS PriceUpperLimit,
                                                        sp.price_lower_limit AS PriceLowerLimit,
                                                        sp.status AS Status,
                                                        sp.valid_from AS ValidFrom,
                                                        sp.valid_upto AS ValidUpto,
                                                        sp.is_active AS IsActive,
                                                        sp.is_tax_included AS IsTaxIncluded,
                                                        sp.version_no AS VersionNo,
                                                        sp.sku_uid AS SkuUid,
                                                        sp.is_latest AS IsLatest
                                                    FROM sku_price sp
                                                      where sp.sku_uid IN @SKUUIDs
                                                    ) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) FROM(SELECT 
                                                        sp.id AS Id,
                                                        sp.uid AS Uid,
                                                        sp.created_by AS CreatedBy,
                                                        sp.created_time AS CreatedTime,
                                                        sp.modified_by AS ModifiedBy,
                                                        sp.modified_time AS ModifiedTime,
                                                        sp.server_add_time AS ServerAddTime,
                                                        sp.server_modified_time AS ServerModifiedTime,
                                                        sp.sku_price_list_uid AS SkuPriceListUid,
                                                        sp.sku_code AS SkuCode,
                                                        sp.uom AS Uom,
                                                        sp.price AS Price,
                                                        sp.default_ws_price AS DefaultWsPrice,
                                                        sp.default_ret_price AS DefaultRetPrice,
                                                        sp.dummy_price AS DummyPrice,
                                                        sp.mrp AS Mrp,
                                                        sp.price_upper_limit AS PriceUpperLimit,
                                                        sp.price_lower_limit AS PriceLowerLimit,
                                                        sp.status AS Status,
                                                        sp.valid_from AS ValidFrom,
                                                        sp.valid_upto AS ValidUpto,
                                                        sp.is_active AS IsActive,
                                                        sp.is_tax_included AS IsTaxIncluded,
                                                        sp.version_no AS VersionNo,
                                                        sp.sku_uid AS SkuUid,
                                                        sp.is_latest AS IsLatest,S.name as SKUName
                                                    FROM sku_price sp
                                                      where sp.sku_uid In @SKUUIDs)AS SUBQUERY");
                }
                var parameters = new Dictionary<string, object>()
                {
                    { "SKUUIDs",skuUIDs }
                };

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>(filterCriterias, sbFilterCriteria, parameters);

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
                        sql.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
                    }
                }
                IEnumerable<Model.Interfaces.ISKUPrice> sKUPrices = await ExecuteQueryAsync<Model.Interfaces.ISKUPrice>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>
                {
                    PagedData = sKUPrices,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<string>> GetApplicablePriceListByStoreUID(string storeUID, string storeType)
        {
            try
            {
                string sql = string.Empty;
                Dictionary<string, object> parameters = new Dictionary<string, object>
             {
             { "storeUID", storeUID }
             };

                if (storeType == WINITSharedObjects.Constants.StoreType.FR)
                {
                    sql = @"SELECT UID FROM sku_price_list 
                    WHERE selection_group = 'Org' 
                      AND selection_type = 'Org' 
                      AND selection_uid = @storeUID 
                      AND type = 'FR' 
                      AND is_active = true 
                      AND status = 'Published' 
                      AND CURRENT_DATE BETWEEN valid_from AND valid_upto;";
                }
                else if (storeType == WINITSharedObjects.Constants.StoreType.FRC)
                {
                    // Need to change
                    sql = @"SELECT UID FROM sku_price_list 
                    WHERE selection_group = 'Org' 
                      AND selection_type = 'Org' 
                      AND selection_uid = @storeUID 
                      AND type = 'FR' 
                      AND is_active = true 
                      AND status = 'Published' 
                      AND CURRENT_DATE BETWEEN valid_from AND valid_upto;";
                }

                return await ExecuteQueryAsync<string>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }

    }
}
