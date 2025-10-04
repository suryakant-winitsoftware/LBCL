using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes;

public class MSSQLSKUPriceDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISKUPriceDL
{
    protected ISKUPriceListDL _sKUPriceListDL;
    public MSSQLSKUPriceDL(IServiceProvider serviceProvider, IConfiguration config, ISKUPriceListDL sKUPriceListDL) : base(serviceProvider, config)
    {
        _sKUPriceListDL = sKUPriceListDL;
    }

    public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string? type = null)
    {
        try
        {
            var sql = new StringBuilder("""

                                        WITH StoreCTE AS(
                                        	SELECT * FROM VW_StoreImpAttributes WHERE store_uid = @OrgUID
                                         ),
                                         LadderingCTE AS(
                                        	 SELECT spl.product_category_id, spl.sales_office, SUM(percentage_discount) AS PercentageDiscount,
                                        	 ROW_NUMBER() OVER (PARTITION BY product_category_id ORDER BY product_category_id, sales_office desc) AS RowNum
                                        	 FROM sku_price_laddering spl 
                                        	 JOIN StoreCTE ON 
                                        	 spl.branch = StoreCTE.branch_uid AND spl.broad_customer_classification = StoreCTE.broad_classification 
                                        	 AND (spl.sales_office = StoreCTE.sales_office_uid OR spl.sales_office is null ) 
                                        	 AND @Date BETWEEN start_date AND end_date 
                                        	 GROUP BY product_category_id, spl.sales_office
                                         ),
                                         SkuPrice as
                                        (SELECT 
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
                                        l.PercentageDiscount,sp.price-(sp.price* isnull(l.PercentageDiscount,0)*0.01) as Price ,
                                        l.PercentageDiscount as LadderingPercentage,
                                        sp.price*l.PercentageDiscount*0.01 as LadderingAmount
                                        from sku_price sp
                                        INNER JOIN sku_ext_data sed on sp.sku_uid = sed.sku_uid
                                        INNER JOIN LadderingCTE l ON l.product_category_id =  sed.product_category_id
                                        WHERE l.RowNum = 1)

                                        select * from SkuPrice 
                                        """);
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder("""
                                             WITH StoreCTE AS(
                                             	SELECT * FROM VW_StoreImpAttributes WHERE store_uid = @OrgUID
                                              ),
                                              LadderingCTE AS(
                                             	 SELECT spl.product_category_id, spl.sales_office, SUM(percentage_discount) AS PercentageDiscount,
                                             	 ROW_NUMBER() OVER (PARTITION BY product_category_id ORDER BY product_category_id, sales_office desc) AS RowNum
                                             	 FROM sku_price_laddering spl 
                                             	 JOIN StoreCTE ON 
                                             	 spl.branch = StoreCTE.branch_uid AND spl.broad_customer_classification = StoreCTE.broad_classification 
                                             	 AND (spl.sales_office = StoreCTE.sales_office_uid OR spl.sales_office is null ) 
                                             	 AND @Date BETWEEN start_date AND end_date 
                                             	 GROUP BY product_category_id, spl.sales_office
                                              ),
                                              SkuPrice as
                                             (SELECT 
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
                                             l.PercentageDiscount,sp.price-(sp.price* isnull(l.PercentageDiscount,0)*0.01) as Price ,
                                             l.PercentageDiscount as LadderingPercentage,
                                             sp.price*l.PercentageDiscount*0.01 as LadderingAmount
                                             from sku_price sp
                                             INNER JOIN sku_ext_data sed on sp.sku_uid = sed.sku_uid
                                             INNER JOIN LadderingCTE l ON l.product_category_id =  sed.product_category_id
                                             WHERE l.RowNum = 1)

                                             select count(1) from SkuPrice 
                                             """);
            }
            var parameters = new Dictionary<string, object?>();
            var orgparam = filterCriterias.Find(e => "OrgUID".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
            var dateparam = filterCriterias.Find(e => "Date".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
            if (orgparam != null)
            {
                parameters.Add("OrgUID", orgparam.Value);
                filterCriterias.Remove(orgparam);
            }
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
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
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
                PagedData = sKUPrices, TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetailsByBroadClassification(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string broadClassification, string branchUID, string? type = null)
    {
        try
        {
            var sql = new StringBuilder(
            """   
                WITH Laddering AS (
                SELECT 
                    product_category_id, 
                    SUM(percentage_discount) AS PercentageDiscount 
                FROM sku_price_laddering spl 
                WHERE spl.broad_customer_classification = @BroadClassification
                    AND (@BranchUID IS NULL OR @BranchUID = '' OR spl.branch = @BranchUID) 
                    AND @Date BETWEEN spl.start_date AND spl.end_date 
            		AND spl.sales_office IS  NULL
                GROUP BY product_category_id
            ),
            SkuPrice as  (

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
                sp.sku_code AS SkuCode,
                sp.uom AS Uom,
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
                COALESCE(l.PercentageDiscount, 0) AS LadderingPercentage,
                sp.price - (sp.price  * COALESCE(l.PercentageDiscount, 0) *  0.01) AS Price,
                sp.price * COALESCE(l.PercentageDiscount, 0) * 0.01 AS LadderingAmount
            FROM sku_price sp
            INNER JOIN sku_ext_data sed ON sp.sku_uid = sed.sku_uid
            Inner JOIN Laddering l ON l.product_category_id = sed.product_category_id)
            SELECT * FROM SkuPrice
            """);
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder("""
                                             WITH Laddering AS (
                                                 SELECT 
                                                     product_category_id, 
                                                     SUM(percentage_discount) AS PercentageDiscount 
                                                 FROM sku_price_laddering spl 
                                                 WHERE spl.broad_customer_classification = @BroadClassification
                                                     AND (@BranchUID IS NULL OR @BranchUID = '' OR spl.branch = @BranchUID) 
                                                     AND @Date BETWEEN spl.start_date AND spl.end_date 
                                             		AND spl.sales_office IS  NULL
                                                 GROUP BY product_category_id
                                             ),
                                             SkuPrice as  (

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
                                                 sp.sku_code AS SkuCode,
                                                 sp.uom AS Uom,
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
                                                 COALESCE(l.PercentageDiscount, 0) AS LadderingPercentage,
                                                 sp.price - (sp.price  * COALESCE(l.PercentageDiscount, 0) *  0.01) AS Price,
                                                 sp.price * COALESCE(l.PercentageDiscount, 0) * 0.01 AS LadderingAmount
                                             FROM sku_price sp
                                             INNER JOIN sku_ext_data sed ON sp.sku_uid = sed.sku_uid
                                             Inner JOIN Laddering l ON l.product_category_id = sed.product_category_id)
                                             SELECT count(1) FROM SkuPrice
                                             """);
            }
            var parameters = new Dictionary<string, object?>();
            var dateparam = filterCriterias.Find(e => "Date".Equals(e.Name, StringComparison.OrdinalIgnoreCase));

            parameters.Add("BroadClassification", broadClassification);
            parameters.Add("BranchUID", branchUID);

            if (dateparam != null)
            {
                parameters.Add("Date", (CommonFunctions.GetDate(Convert.ToString(dateparam.Value))).Date);
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
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
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
                PagedData = sKUPrices, TotalCount = totalCount
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
            StringBuilder sqlFilterQuery = new StringBuilder();
            StringBuilder innerfilter = new StringBuilder(
            """
                    WHERE    EXISTS (
                SELECT 1
                FROM sku_attributes sa
                WHERE sa.sku_uid = sp.sku_uid
                
            
            """);
            Dictionary<string, object?> parameters = new Dictionary<string, object?>();
            if (filterCriterias is not null)
            {
                var isActiveFilter = filterCriterias.Find(e => "IsActive".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (isActiveFilter == null)
                {
                    sqlFilterQuery.Append(" AND IsActive = 1 ");
                }
                else
                {
                    filterCriterias.Remove(isActiveFilter);
                }
                var skuCodeNameFilter = filterCriterias.Find(e => "skucodeandname".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (skuCodeNameFilter != null)
                {
                    sqlFilterQuery.Append(" AND (skucode like '%' + @skucodeandname+ '%' or  skuname like '%' + @skucodeandname+ '%') ");
                    parameters.Add("skucodeandname", skuCodeNameFilter.Value);
                    filterCriterias.Remove(skuCodeNameFilter);
                }

                var divisionFilter = filterCriterias.Find(e => "DivisionUIDs".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (divisionFilter is not null)
                {
                    sqlFilterQuery.Append(" AND divisionuid IN @DivisonUIDs ");
                    parameters.Add("DivisonUIDs", JsonConvert.DeserializeObject<List<string>>(divisionFilter.Value.ToString()));
                    filterCriterias.Remove(divisionFilter);
                }

                var attributeTypeFilter = filterCriterias.Find(e => "AttributeType".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (attributeTypeFilter is not null)
                {
                    innerfilter.Append(" AND sa.[type] IN  @AttributeTypes");
                    parameters.Add("AttributeTypes", JsonConvert.DeserializeObject<List<string>>(attributeTypeFilter.Value.ToString()));
                    filterCriterias.Remove(attributeTypeFilter);
                }

                var attributeValueFilter = filterCriterias.Find(e => "AttributeValue".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (attributeValueFilter is not null)
                {
                    innerfilter.Append(" AND sa.value IN  @AttributeValue");
                    parameters.Add("AttributeValue", JsonConvert.DeserializeObject<List<string>>(attributeValueFilter.Value.ToString()));
                    filterCriterias.Remove(attributeValueFilter);
                }
            }
            innerfilter.Append(" ) ");
            var sql = new StringBuilder(
            $"""
                 select * from (
                                  
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
                
                 s.supplier_org_uid as divisionuid,
                 ISNULL(saData.AttributeTypes, '') AS AttributeTypes,
                 ISNULL(saData.AttributeValues, '') AS AttributeValues
             FROM 
                 sku_price sp
             INNER JOIN 
                 sku s ON sp.sku_uid = s.uid
             CROSS APPLY (
                 SELECT 
                     STRING_AGG(sa.[type], ', ') AS AttributeTypes,
                     STRING_AGG(sa.value, ', ') AS AttributeValues
                 FROM 
                     sku_attributes sa
                 WHERE 
                     sa.sku_uid = sp.sku_uid 
             ) AS saData {innerfilter.ToString()}
             ) as SubQuery WHERE 1=1 
             """);
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(
                $"""
                 SELECT COUNT(1) FROM(
                   select  sp.id AS Id,
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
                   
                     s.supplier_org_uid as divisionuid,
                     ISNULL(saData.AttributeTypes, '') AS AttributeTypes,
                     ISNULL(saData.AttributeValues, '') AS AttributeValues
                 FROM 
                     sku_price sp
                 INNER JOIN 
                     sku s ON sp.sku_uid = s.uid
                 CROSS APPLY (
                     SELECT 
                         STRING_AGG(sa.[type], ', ') AS AttributeTypes,
                         STRING_AGG(sa.value, ', ') AS AttributeValues
                     FROM 
                         sku_attributes sa
                     WHERE 
                         sa.sku_uid = sp.sku_uid 
                 ) AS saData {innerfilter.ToString()}
                 )AS SUBQUERY WHERE 1=1 
                 """);
            }
            sql.Append(sqlFilterQuery);
            sqlCount.Append(sqlFilterQuery);
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" AND ");
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
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
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
                PagedData = sKUPrices, TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetailsByOrgUID(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string orgUID)
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
                                                    inner JOIN sku S ON SP.sku_uid = S.uid
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
                                                    inner JOIN sku S ON SP.sku_uid = S.uid)AS SUBQUERY");
            }
            var parameters = new Dictionary<string, object>();

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
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
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
                PagedData = sKUPrices, TotalCount = totalCount
            };

            return pagedResponse;
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
            {
                "UID", UID
            }
        };
        var sql = @"SELECT id AS Id,
                                  uid AS UID,
                                  created_by AS CreatedBy,
                                  created_time AS CreatedTime,
                                  modified_by AS ModifiedBy,
                                  modified_time AS ModifiedTime,
                                  server_add_time AS ServerAddTime,
                                  server_modified_time AS ServerModifiedTime,
                                  sku_price_list_uid AS SKUPriceListUid,
                                  sku_code AS SKUCode,
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
        Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPriceDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>(sql, parameters);
        return sKUPriceDetails;
    }
    public async Task<int> CreateSKUPrice(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice)
    {
        try
        {
            var sql = @"INSERT INTO sku_price (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, sku_price_list_uid,
                            sku_code, uom, price, default_ws_price, default_ret_price, dummy_price, mrp, price_upper_limit, price_lower_limit, status,
                            valid_from, valid_upto, is_active, is_tax_included, version_no, sku_uid) VALUES (@UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime,
                            @ServerAddTime ,@ServerModifiedTime ,@SKUPriceListUID ,@SKUCode ,@UOM ,@Price ,@DefaultWSPrice ,@DefaultRetPrice ,@DummyPrice ,@MRP ,
                            @PriceUpperLimit ,@PriceLowerLimit ,@Status ,@ValidFrom ,@ValidUpto ,@IsActive ,@IsTaxIncluded ,@VersionNo,@SKUUID);";
            return await ExecuteNonQueryAsync(sql, sKUPrice);
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
            return await ExecuteNonQueryAsync(sql, sKUPrice);
        }
        catch (Exception)

        {
            throw;
        }
    }

    public async Task<int> DeleteSKUPrice(string UID)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            {
                "UID", UID
            }
        };
        var sql = @"DELETE FROM sku_price
                        WHERE uid = @UID;";
        return await ExecuteNonQueryAsync(sql, parameters);
    }

    public async Task<(List<ISKUPriceList>, List<ISKUPrice>, int)> SelectSKUPriceViewByUID(List<SortCriteria> sortCriterias,
        int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string UID)
    {
        try
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {
                    "UID", UID
                },
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

            List<Model.Interfaces.ISKUPriceList> skuPriceList = await ExecuteQueryAsync<Model.Interfaces.ISKUPriceList>(sKUPriceListSql.ToString(), parameters);

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
				Left join sku s on s.uid=sp.sku_uid WHERE sku_price_list_uid= @UID) as SubQuery  ");
            }


            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" Where ");
                AppendFilterCriteria<Model.Interfaces.ISKUPrice>(filterCriterias, sbFilterCriteria, parameters);

                sKUPriceSQL.Append(sbFilterCriteria);

                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                sKUPriceSQL.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sKUPriceSQL);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sKUPriceSQL.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sKUPriceSQL.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
            }

            int totalCount = 0;

            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            List<Model.Interfaces.ISKUPrice> sKUPrices = await ExecuteQueryAsync<Model.Interfaces.ISKUPrice>(sKUPriceSQL.ToString(), parameters);

            return (skuPriceList, sKUPrices, totalCount);
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
                    List<Winit.Modules.SKU.Model.Classes.SKUPrice> sKUPriceListForAdding = new();
                    List<Winit.Modules.SKU.Model.Classes.SKUPrice> sKUPriceListForUpdate = new();
                    if (sKUPriceList.Any(spl => spl.ActionType == ActionType.Add))
                    {
                        sKUPriceListForAdding = sKUPriceList.Where(spl => spl.ActionType == ActionType.Add).ToList();
                    }
                    if (sKUPriceList.Any(spl => spl.ActionType == ActionType.Update))
                    {
                        sKUPriceListForUpdate = sKUPriceList.Where(spl => spl.ActionType == ActionType.Update).ToList();
                    }
                    if (sKUPriceListForAdding != null && sKUPriceListForAdding.Any())
                    {
                        retVal = await InsertSKUPriceList(sKUPriceListForAdding, connection, transaction);
                    }
                    if (sKUPriceListForUpdate != null && sKUPriceListForUpdate.Any())
                    {
                        retVal = await UpdateSKUPriceList(sKUPriceListForUpdate, connection, transaction);
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
    public async Task<int> CreateSKUPriceView(Winit.Modules.SKU.Model.Classes.SKUPriceViewDTO sKUPriceViewDTO)
    {
        int count = -1;
        try
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        count += await _sKUPriceListDL.CreateSKUPriceList(sKUPriceViewDTO.SKUPriceGroup, connection, transaction);
                        if (count < 0)
                        {
                            transaction.Rollback();
                            throw new Exception("SKUPriceList Insert failed");
                        }

                        if (sKUPriceViewDTO.SKUPriceList != null && sKUPriceViewDTO.SKUPriceList.Any())
                        {
                            count += await InsertSKUPriceList(sKUPriceViewDTO.SKUPriceList, connection, transaction);
                        }
                        if (count < 0)
                        {
                            transaction.Rollback();
                            throw new Exception("SkuPrice Table Insert Failed");
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
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (sKUPriceViewDTO.SKUPriceGroup != null)
                        {
                            count += await _sKUPriceListDL.UpdateSKUPriceList(sKUPriceViewDTO.SKUPriceGroup);
                        }
                        List<Winit.Modules.SKU.Model.Classes.SKUPrice> sKUPriceListForAdding = new();
                        List<Winit.Modules.SKU.Model.Classes.SKUPrice> sKUPriceListForUpdate = new();

                        if (sKUPriceViewDTO.SKUPriceList != null && sKUPriceViewDTO.SKUPriceList.Any())
                        {
                            if (sKUPriceViewDTO.SKUPriceList.Any(spl => spl.ActionType == ActionType.Add))
                            {
                                sKUPriceListForAdding = sKUPriceViewDTO.SKUPriceList.Where(spl => spl.ActionType == ActionType.Add).ToList();
                            }
                            if (sKUPriceViewDTO.SKUPriceList.Any(spl => spl.ActionType == ActionType.Update))
                            {
                                sKUPriceListForUpdate = sKUPriceViewDTO.SKUPriceList.Where(spl => spl.ActionType == ActionType.Update).ToList();
                            }
                            if (sKUPriceListForAdding != null && sKUPriceListForAdding.Any())
                            {
                                count += await InsertSKUPriceList(sKUPriceListForAdding, connection, transaction);
                            }
                            if (sKUPriceListForUpdate != null && sKUPriceListForUpdate.Any())
                            {
                                count += await UpdateSKUPriceList(sKUPriceListForUpdate, connection, transaction);
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
    private async Task<int> InsertSKUPriceList(List<SKUPrice> sKUPriceList, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retVal = -1;
        try
        {
            var skuPriceQuery = @"INSERT INTO sku_price (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                                                        sku_price_list_uid, sku_code, uom, price, default_ws_price, default_ret_price, dummy_price,
                                                        mrp, price_upper_limit, price_lower_limit, status, valid_from, valid_upto,
                                                        is_active, is_tax_included, version_no, sku_uid, is_latest) VALUES 
                                                        (@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,
                                                        @SKUPriceListUID,@SKUCode,@UOM,@Price,@DefaultWSPrice,@DefaultRetPrice,@DummyPrice,@MRP,@PriceUpperLimit,
                                                        @PriceLowerLimit,@Status,@ValidFrom,@ValidUpto,@IsActive,@IsTaxIncluded,@VersionNo,@SKUUID,@IsLatest);";

            retVal = await ExecuteNonQueryAsync(skuPriceQuery, connection, transaction, sKUPriceList);
        }
        catch
        {
            throw;
        }
        return retVal;
    }


    private async Task<int> InsertSKUPrice(SKUPrice sKUPrice, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retVal = -1;
        try
        {
            var skuPriceQuery = @"INSERT INTO sku_price (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                                                        sku_price_list_uid, sku_code, uom, price, default_ws_price, default_ret_price, dummy_price,
                                                        mrp, price_upper_limit, price_lower_limit, status, valid_from, valid_upto,
                                                        is_active, is_tax_included, version_no, sku_uid, is_latest) VALUES 
                                                        (@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,
                                                        @SKUPriceListUID,@SKUCode,@UOM,@Price,@DefaultWSPrice,@DefaultRetPrice,@DummyPrice,@MRP,@PriceUpperLimit,
                                                        @PriceLowerLimit,@Status,@ValidFrom,@ValidUpto,@IsActive,@IsTaxIncluded,@VersionNo,@SKUUID,@IsLatest);";
            retVal = await ExecuteNonQueryAsync(skuPriceQuery, connection, transaction, sKUPrice);
        }
        catch
        {
            throw;
        }
        return retVal;
    }
    private async Task<int> UpdateSKUPrice(SKUPrice sKUPrice, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retVal = -1;
        try
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

            retVal = await ExecuteNonQueryAsync(skuPriceQuery, connection, transaction, sKUPrice);
        }
        catch
        {
            throw;
        }
        return retVal;

    }
    private async Task<int> UpdateSKUPriceList(List<SKUPrice> sKUPriceList, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retVal = -1;
        try
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

            retVal = await ExecuteNonQueryAsync(skuPriceQuery, connection, transaction, sKUPriceList);
        }
        catch
        {
            throw;
        }
        return retVal;

    }

    public async Task<int> CreateStandardPriceForSKU(string skuUID)
    {
        try
        {
            var sql = @"INSERT INTO sku_price (
                            uid, created_by, created_time, modified_by, modified_time, 
                            server_add_time, server_modified_time, sku_price_list_uid, sku_code, uom,
                            price, default_ws_price, default_ret_price, dummy_price, mrp, price_upper_limit, price_lower_limit,
                            status, valid_from, valid_upto, is_active, is_tax_included, version_no, sku_uid, is_latest)
                            SELECT 
                                CONCAT(SPL.uid, s.uid) AS uid,
                                s.created_by, 
                                s.created_time, 
                                s.modified_by, 
                                s.modified_time, 
                                s.server_add_time,
                                s.server_modified_time, 
                                SPL.uid AS sku_price_list_uid, 
                                s.code AS sku_code, 
                                s.base_uom AS uom,
                                0 AS price, 
                                0 AS default_ws_price, 
                                0 AS default_ret_price, 
                                0 AS dummy_price, 
                                0 AS mrp, 
                                0 AS price_upper_limit, 
                                0 AS price_lower_limit,
                                'Published' AS status, 
                                s.from_date AS valid_from, 
                                '2099-12-31' AS valid_upto, 
                                1 AS is_active,
                                0 AS is_tax_included,
                                'V1' AS version_no, 
                                s.uid AS sku_uid, 
                                1 AS is_latest
                            FROM 
                                sku_price_list SPL 
                            INNER JOIN 
                                sku S ON SPL.type = 'FR' 
                                AND SPL.selection_group = 'Org' 
                                AND SPL.selection_type = 'Org' 
                                AND SPL.status = 'Published'  
                                AND s.uid = @SKUUID
                            LEFT JOIN 
                                sku_price SP ON SP.sku_price_list_uid = SPL.uid 
                                AND SP.sku_uid = s.uid
                            WHERE 
                                SP.id IS NULL;";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {
                    "SKUUID", skuUID
                },

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
                                                        sp.is_latest AS IsLatest
                                                    FROM sku_price sp
                                                      where sp.sku_uid In @SKUUIDs)AS SUBQUERY");
            }
            var parameters = new Dictionary<string, object>()
            {
                {
                    "SKUUIDs", skuUIDs
                }
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
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
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
                PagedData = sKUPrices, TotalCount = totalCount
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
                {
                    "storeUID", storeUID
                }
            };

            if (storeType == WINITSharedObjects.Constants.StoreType.FR)
            {
                sql = @"SELECT UID FROM sku_price_list 
                    WHERE selection_group = 'Org' 
                      AND selection_type = 'Org' 
                      AND selection_uid = @storeUID 
                      AND type = 'FR' 
                      AND is_active = 1 
                      AND status = 'Published' 
                      AND getdate() BETWEEN valid_from AND valid_upto;";
            }
            else if (storeType == WINITSharedObjects.Constants.StoreType.FRC)
            {
                // Need to change
                sql = @"SELECT UID FROM sku_price_list 
                    WHERE selection_group = 'Org' 
                      AND selection_type = 'Org' 
                      AND selection_uid = @storeUID 
                      AND type = 'FR' 
                      AND is_active = 1 
                      AND status = 'Published' 
                      AND getdate() BETWEEN valid_from AND valid_upto;";
            }

            return await ExecuteQueryAsync<string>(sql, parameters);
        }
        catch
        {
            throw;
        }
    }

}
